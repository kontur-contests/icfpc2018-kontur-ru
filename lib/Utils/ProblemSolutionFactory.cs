using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using lib.Models;
using lib.Strategies;
using lib.Strategies.Features;

namespace lib.Utils
{
    public static class ProblemSolutionFactory
    {
        public static ProblemSolutionPair[] GetTasks()
        {
            //            var solvedProblemNames = ElasticHelper.FetchSolvedProblemNames();

            return
                (from p in GetProblems()
                 from s in GetSolutions(p)
                 where s.CompatibleProblemTypes.Contains(p.Type)
                 select new ProblemSolutionPair
                 {
                     Problem = p,
                     Solution = s
                 }).ToArray();
        }

        public static Problem[] GetProblems()
        {
            return Directory
                .EnumerateFiles(FileHelper.ProblemsDir, "*.mdl")
                .Where(x => !Regex.IsMatch(Path.GetFileName(x) ?? "", "FR.*_src.mdl")) // ignore src for reassemble tasks (use tgt)
                .Select(CreateProblem)
                .ToArray();
        }

        public static Problem LoadProblem(string name)
        {
            return CreateProblem(Path.Combine(FileHelper.ProblemsDir, $"{name}_tgt.mdl"));
        }

        public static Problem CreateProblem(string fullPath)
        {
            var fileName = Path.GetFileName(fullPath) ?? "";
            var type = fileName.StartsWith("FA") ? ProblemType.Assemble : fileName.StartsWith("FD") ? ProblemType.Disassemble : ProblemType.Reassemble;

            // ReSharper disable once PossibleNullReferenceException
            var name = Path.GetFileNameWithoutExtension(fullPath).Split('_')[0]; // no tgt and src suffix
            var directoryName = Path.GetDirectoryName(fullPath) ?? "";
            return new Problem
            {
                FileName = fullPath,
                Name = name,
                SourceMatrix = TryLoadMatrix(Path.Combine(directoryName, $"{name}_src.mdl")),
                TargetMatrix = TryLoadMatrix(Path.Combine(directoryName, $"{name}_tgt.mdl")),
                Type = type
            };
        }

        private static Matrix TryLoadMatrix(string filename)
        {
            if (!File.Exists(filename)) return null;
            return Matrix.Load(File.ReadAllBytes(filename));
        }

        public static readonly Solution gFast = new Solution
        {
            Name = "GS + TH Fast",
            ProblemPrioritizer = p => ProblemPriority.Normal,
            Solver = problem => new GreedyPartialSolver(
                                      problem.TargetMatrix,
                                      problem.SourceMatrix,
                                      new ThrowableHelperFast(problem.TargetMatrix))
        };

        public static readonly Solution gLayers = new Solution
        {
            Name = "GS + Layers",
            ProblemPrioritizer = p => ProblemPriority.Normal,
            Solver = problem => new GreedyPartialSolver(
                                    problem.TargetMatrix,
                                    problem.SourceMatrix,
                                      new ThrowableHelper(problem.TargetMatrix),
                                      new BottomToTopBuildingAround())
        };

        public static readonly Solution columns = new Solution
        {
            Name = "Columns",
            ProblemPrioritizer = p => ProblemPriority.Normal,
            Solver = problem => new DivideAndConquer(problem.TargetMatrix, false),
        };

        public static readonly Solution columnsBbx = new Solution
        {
            Name = "ColumnsBbx",
            ProblemPrioritizer = p => ProblemPriority.Normal,
            Solver = problem => new DivideAndConquer(problem.TargetMatrix, true),
        };

        public static readonly Solution gForLarge = new Solution
        {
            Name = "GreedyForLarge",
            ProblemPrioritizer = p => ProblemPriority.Normal,
            Solver = problem => new GreedyPartialSolver(
                                    problem.TargetMatrix,
                                    problem.SourceMatrix,
                               new ThrowableHelper(problem.TargetMatrix),
                               new NearToFarBottomToTopBuildingAround())
        };

        public static readonly Solution stupidDisassembler = new Solution
        {
            Name = "disasm",
            ProblemPrioritizer = p => ProblemPriority.Normal,
            Solver = problem => new StupidDisassembler(problem.SourceMatrix),
            CompatibleProblemTypes = new[] { ProblemType.Disassemble }
        };

        public static readonly Solution invertorDisassembler = new Solution
        {
            Name = "invertor",
            ProblemPrioritizer = p => ProblemPriority.Normal,
            Solver = problem => new InvertorDisassembler(new GreedyPartialSolver(
                                                             problem.TargetMatrix,
                                                             problem.SourceMatrix,
                                                        new ThrowableHelperFast(problem.SourceMatrix)), problem.SourceMatrix),
            CompatibleProblemTypes = new[] { ProblemType.Disassemble }
        };

        public static readonly Solution invColDisassembler = new Solution
        {
            Name = "invCol",
            ProblemPrioritizer = p => ProblemPriority.Normal,
            Solver = problem => new InvertorDisassembler(new DivideAndConquer(problem.SourceMatrix, true), problem.SourceMatrix),
            CompatibleProblemTypes = new[] { ProblemType.Disassemble }
        };

        public static readonly Solution invSlice6x6Disassembler = new Solution
        {
            Name = "invSlice6x6",
            ProblemPrioritizer = p => ProblemPriority.Normal,
            Solver = problem => new InvertorDisassembler(new HorizontalSlicer(problem.SourceMatrix, 6, 6, true), problem.SourceMatrix),
            CompatibleProblemTypes = new[] { ProblemType.Disassemble }
        };
        public static readonly Solution blockDeconstructor = new Solution
        {
            Name = "BlockDeconstructor",
            ShortName = "bd",
            ProblemPrioritizer = p => ProblemPriority.Normal,
            Solver = problem => new FastDeconstructor(problem.SourceMatrix),
            CompatibleProblemTypes = new[] { ProblemType.Disassemble },
        };

        public static readonly Solution parallelGredy = new Solution
        {
            Name = "ParallelGredy",
            ProblemPrioritizer = p => ProblemPriority.Normal,
            Solver = problem =>
            {
                var state = new DeluxeState(problem.SourceMatrix, problem.TargetMatrix);
                var solver = new Solver(state, new ParallelGredyFill(state, state.Bots.First()));
                return solver;
            }
        };
        private static Solution[] GetSolutions(Problem problem1)
        {
            var slicers = new List<Solution>();
            var slicersParameters = new List<(int, int)>();
            for (var xSize = 2; xSize <= 40; xSize++)
                for (var zSize = 2; zSize <= 40; zSize++)
                {

                    var count = xSize * zSize;
                    if (30 <= count && count <= 40 && count % 2 == 0)
                    {
                        slicers.Add(CreateSlicerAssembler(xSize, zSize));
                        slicersParameters.Add((xSize, zSize));
                    }
                }

            (string name, Func<Matrix, IAmSolver> solver)[] solvers =
                slicersParameters.Select(p => ($"s{p.Item1}x{p.Item2}",
                                                  (Func<Matrix, IAmSolver>)(m => new HorizontalSlicer(m, p.Item1, p.Item2, true))))
                                 .ToArray();

            var raSolutions = new List<Solution>();
            var disassemblers = solvers
                .Select(s => (name: s.name, solver: (Func<Problem, IAmSolver>)(problem => new InvertorDisassembler(s.solver(problem.SourceMatrix), problem.SourceMatrix) as IAmSolver)))
                .Concat(new[]
                    {
                        (name: "bd", solver: blockDeconstructor.Solver)
                    });

            foreach (var disassembler in disassemblers)
                foreach (var assembler in solvers)
                {
                    raSolutions.Add(new Solution
                    {
                        Name = $"RA+{disassembler.name}+{assembler.name}",
                        ProblemPrioritizer = p => ProblemPriority.Normal,
                        Solver = problem => new SimpleReassembler(
                                           disassembler.solver(problem),
                                           assembler.solver(problem.TargetMatrix),
                                           problem.SourceMatrix,
                                           problem.TargetMatrix),
                        CompatibleProblemTypes = new[] { ProblemType.Reassemble }

                    });
                }
            return new[]
                {
//                    stupidDisassembler,
//                    invertorDisassembler,
//                    invColDisassembler,
                    invSlice6x6Disassembler,
//                    gFast,
//                    gLayers,
//                    columns,
//                    columnsBbx,
                    gForLarge,
                    blockDeconstructor,
                }.Concat(raSolutions)
                 .Concat(slicers)
                 .ToArray();
        }

        public static Solution CreateSlicerAssembler(int xSize = 6, int zSize = 6)
        {
            return new Solution
            {
                Name = $"Slicer{xSize}x{zSize}",
                ShortName = $"s{xSize}x{zSize}",
                ProblemPrioritizer = p => ProblemPriority.Normal,
                Solver = problem => new HorizontalSlicer(problem.TargetMatrix, xSize, zSize, true),
            };
        }

        private static IAmSolver CreateGreedy(Matrix matrix)
        {
            return new GreedyPartialSolver(matrix, new ThrowableHelper(matrix), new NearToFarBottomToTopBuildingAround());
        }
        private static IAmSolver CreateColumns(Matrix matrix)
        {
            return new DivideAndConquer(matrix, true);
        }

        public static IAmSolver CreateSlicer6x6(Matrix matrix)
        {
            return new HorizontalSlicer(matrix, 6, 6, true);
        }

        public static Solution CreateInvertingDisassembler(Solution assemblerSolution)
        {
            return new Solution
            {
                Name = $"inv{assemblerSolution.Name}",
                Solver = problem => new InvertorDisassembler(
                    assemblerSolution.Solver(
                        new Problem
                            {
                                FileName = problem.FileName,
                                Name = problem.Name,
                                Type = ProblemType.Assemble,
                                SourceMatrix = null,
                                TargetMatrix = problem.SourceMatrix
                            }),
                    problem.SourceMatrix),
                CompatibleProblemTypes = new[] { ProblemType.Disassemble }
            };
        }

        public static Solution CreateReassembler(Solution disassembler, Solution assembler)
        {
            return new Solution
                {
                    Name = $"RA+{disassembler.ShortName ?? disassembler.Name}+{assembler.ShortName ?? assembler.Name}",
                    ProblemPrioritizer = p => ProblemPriority.Normal,
                    Solver = problem => new SimpleReassembler(
                                            disassembler.Solver(
                                                new Problem
                                                    {
                                                        Type = ProblemType.Disassemble,
                                                        SourceMatrix = problem.SourceMatrix,
                                                        TargetMatrix = null,
                                                        FileName = problem.FileName,
                                                        Name = problem.Name,
                                                    }),
                                            assembler.Solver(
                                                new Problem
                                                    {
                                                        Type = ProblemType.Assemble,
                                                        SourceMatrix = null,
                                                        TargetMatrix = problem.TargetMatrix,
                                                        FileName = problem.FileName,
                                                        Name = problem.Name,
                                                    }),
                                            problem.SourceMatrix,
                                            problem.TargetMatrix),
                    CompatibleProblemTypes = new[] {ProblemType.Reassemble}
                };
        }
    }

    public class Problem
    {
        public string FileName { get; set; }
        public string Name { get; set; }
        public Matrix SourceMatrix { get; set; }
        public int R => SourceMatrix?.R ?? TargetMatrix.R;
        public Matrix TargetMatrix { get; set; }
        public ProblemType Type { get; set; }
    }

    public enum ProblemType
    {
        Assemble,
        Disassemble,
        Reassemble
    }

    public enum ProblemPriority
    {
        High,
        Normal,
        DoNotSolve
    }

    public class Solution
    {
        public string ShortName { get; set; }
        public string Name { get; set; }
        public Func<Problem, IAmSolver> Solver { get; set; }
        public ProblemType[] CompatibleProblemTypes { get; set; } = { ProblemType.Assemble };
        public Func<Problem, ProblemPriority> ProblemPrioritizer { get; set; }
    }

    public class ProblemSolutionPair
    {
        public Problem Problem { get; set; }
        public Solution Solution { get; set; }
    }
}