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
            var solvedProblemNames = ElasticHelper.FetchSolvedProblemNames();
            
            return
                (from p in GetProblems()
                 from s in GetSolutions(p, solvedProblemNames)
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

        private static Solution[] GetSolutions(Problem problem, HashSet<string> solvedProblemNames)
        {
            var R = problem.TargetMatrix?.R ?? problem.SourceMatrix.R;

            var gFast = new Solution
            {
                Name = "GS + TH Fast",
                ProblemPrioritizer = p => solvedProblemNames.Contains(p.Name) ? ProblemPriority.Normal : ProblemPriority.High,
                Solver = () => new GreedyPartialSolver(
                                          problem.TargetMatrix.Voxels,
                                          new bool[R, R, R],
                                          new Vec(0, 0, 0),
                                          new ThrowableHelperFast(problem.TargetMatrix))
            };

            var gLayers = new Solution
            {
                Name = "GS + Layers",
                ProblemPrioritizer = p => solvedProblemNames.Contains(p.Name) ? ProblemPriority.Normal : ProblemPriority.High,
                Solver = () => new GreedyPartialSolver(
                                          problem.TargetMatrix.Voxels,
                                          new bool[R, R, R],
                                          new Vec(0, 0, 0),
                                          new ThrowableHelper(problem.TargetMatrix),
                                          new BottomToTopBuildingAround())
            };

            var columns = new Solution
            {
                Name = "Columns",
                ProblemPrioritizer = p => solvedProblemNames.Contains(p.Name) ? ProblemPriority.Normal : ProblemPriority.High,
                Solver = () => new DivideAndConquer(problem.TargetMatrix, false),
            };

            var columnsBbx = new Solution
            {
                Name = "ColumnsBbx",
                ProblemPrioritizer = p => solvedProblemNames.Contains(p.Name) ? ProblemPriority.Normal : ProblemPriority.High,
                Solver = () => new DivideAndConquer(problem.TargetMatrix, true),
            };

            var gForLarge = new Solution
            {
                Name = "GreedyForLarge",
                ProblemPrioritizer = p => solvedProblemNames.Contains(p.Name) ? ProblemPriority.Normal : ProblemPriority.High,
                Solver = () => new GreedyPartialSolver(
                                   problem.TargetMatrix.Voxels,
                                   new bool[R, R, R],
                                   new Vec(0, 0, 0),
                                   new ThrowableHelper(problem.TargetMatrix),
                                   new NearToFarBottomToTopBuildingAround())
            };

            var stupidDisassembler = new Solution
            {
                Name = "disasm",
                ProblemPrioritizer = p => solvedProblemNames.Contains(p.Name) ? ProblemPriority.Normal : ProblemPriority.High,
                Solver = () => new StupidDisassembler(problem.SourceMatrix),
                CompatibleProblemTypes = new[] { ProblemType.Disassemble }
            };
            
            var invertorDisassembler = new Solution
            {
                Name = "invertor",
                ProblemPrioritizer = p => solvedProblemNames.Contains(p.Name) ? ProblemPriority.Normal : ProblemPriority.High,
                Solver = () => new InvertorDisassembler(new GreedyPartialSolver(
                                                            problem.SourceMatrix.Voxels,
                                                            new bool[R, R, R],
                                                            new Vec(0, 0, 0),
                                                            new ThrowableHelperFast(problem.SourceMatrix)), problem.SourceMatrix),
                CompatibleProblemTypes = new[] { ProblemType.Disassemble }
            };
            
            var invColDisassembler = new Solution
            {
                Name = "invCol",
                ProblemPrioritizer = p => solvedProblemNames.Contains(p.Name) ? ProblemPriority.Normal : ProblemPriority.High,
                Solver = () => new InvertorDisassembler(new DivideAndConquer(problem.SourceMatrix, true), problem.SourceMatrix),
                CompatibleProblemTypes = new[] { ProblemType.Disassemble }
            };
            
            var invSlice6x6Disassembler = new Solution
            {
                Name = "invSlice6x6",
                ProblemPrioritizer = p => solvedProblemNames.Contains(p.Name) ? ProblemPriority.Normal : ProblemPriority.High,
                Solver = () => new InvertorDisassembler(new HorizontalSlicer(problem.SourceMatrix, 6, 6, true), problem.SourceMatrix),
                CompatibleProblemTypes = new[] { ProblemType.Disassemble }
            };

            var slicer6x6 = new Solution
            {
                Name = "Slicer6x6",
                ProblemPrioritizer = p => solvedProblemNames.Contains(p.Name) ? ProblemPriority.Normal : ProblemPriority.High,
                Solver = () => new HorizontalSlicer(problem.TargetMatrix, 6, 6, true),
            };

            var slicer8x5 = new Solution
            {
                Name = "Slicer8x5",
                ProblemPrioritizer = p => solvedProblemNames.Contains(p.Name) ? ProblemPriority.Normal : ProblemPriority.High,
                Solver = () => new HorizontalSlicer(problem.TargetMatrix, 8, 5, true),
            };

            var blockDeconstructor = new Solution
            {
                Name = "BlockDeconstructor",
                ProblemPrioritizer = p => solvedProblemNames.Contains(p.Name) ? ProblemPriority.Normal : ProblemPriority.High,
                Solver = () => new FastDeconstructor(problem.TargetMatrix),
                CompatibleProblemTypes = new [] {ProblemType.Disassemble},
            };

            var parallelGredy = new Solution
            {
                Name = "ParallelGredy",
                ProblemPrioritizer = p => solvedProblemNames.Contains(p.Name) ? ProblemPriority.Normal : ProblemPriority.High,
                Solver = () =>
                    {
                        var state = new DeluxeState(problem.SourceMatrix, problem.TargetMatrix);
                        var solver = new Solver(state, new ParallelGredyFill(state, state.Bots.First()));
                        return solver;
                    }
            };
        
            (string name, Func<Matrix, IAmSolver> solver)[] solvers = {
                    ("g", CreateGreedy),
//                    ("c", CreateColumns),
                    ("s6x6", CreateSlicer6x6),
                };

            var raSolutions = new List<Solution>();
            var disassemblers = solvers
                .Select(s => (name: s.name, solver: (Func<IAmSolver>)(() => (IAmSolver)new InvertorDisassembler(s.solver(problem.SourceMatrix), problem.SourceMatrix))))
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
                        ProblemPrioritizer = p => solvedProblemNames.Contains(p.Name) ? ProblemPriority.Normal : ProblemPriority.High,
                        Solver = () => new SimpleReassembler(
                                           disassembler.solver(),
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
                    slicer6x6,
                    slicer8x5,
                    blockDeconstructor,
                }.Concat(raSolutions).ToArray();
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
        public string Name { get; set; }
        public Func<IAmSolver> Solver { get; set; }
        public ProblemType[] CompatibleProblemTypes { get; set; } = { ProblemType.Assemble };
        public Func<Problem, ProblemPriority> ProblemPrioritizer { get; set; }
    }

    public class ProblemSolutionPair
    {
        public Problem Problem { get; set; }
        public Solution Solution { get; set; }
    }
}