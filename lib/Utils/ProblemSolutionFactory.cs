using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using lib.Models;
using lib.Strategies;

namespace lib.Utils
{
    public static class ProblemSolutionFactory
    {
        public static ProblemSolutionPair[] GetTasks()
        {
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

        public static Problem CreateProblem(string fullPath)
        {
            var fileName = Path.GetFileName(fullPath) ?? "";
            var type = fileName.StartsWith("FA") ? ProblemType.Assemble : fileName.StartsWith("FD") ? ProblemType.Disassemble : ProblemType.Reassemple;

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

        private static Solution[] GetSolutions(Problem problem)
        {
            var R = problem.TargetMatrix?.R ?? problem.SourceMatrix.R;

            var gFast = new Solution
            {
                Name = "GS + TH Fast",
                Solver = () => new GreedyPartialSolver(
                                          problem.TargetMatrix.Voxels,
                                          new bool[R, R, R],
                                          new Vec(0, 0, 0),
                                          new ThrowableHelperFast(problem.TargetMatrix))
            };

            var gLayers = new Solution
            {
                Name = "GS + Layers",
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
                Solver = () => new DivideAndConquer(problem.TargetMatrix, false),
            };

            var columnsBbx = new Solution
            {
                Name = "ColumnsBbx",
                Solver = () => new DivideAndConquer(problem.TargetMatrix, true),
            };

            var gForLarge = new Solution
            {
                Name = "GreedyForLarge",
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
                Solver = () => new StupidDisassembler(problem.SourceMatrix),
                CompatibleProblemTypes = new[] { ProblemType.Disassemble }
            };
            
            var invertorDisassembler = new Solution
            {
                Name = "invertor",
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
                Solver = () => new InvertorDisassembler(new DivideAndConquer(problem.SourceMatrix, true), problem.SourceMatrix),
                CompatibleProblemTypes = new[] { ProblemType.Disassemble }
            };

            return new[]
                {
                    stupidDisassembler,
                    invertorDisassembler,
                    invColDisassembler,
                    gFast,
                    gLayers,
                    columns,
                    columnsBbx,
                    gForLarge
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
        Reassemple
    }

    public class Solution
    {
        public string Name { get; set; }
        public Func<IAmSolver> Solver { get; set; }
        public ProblemType[] CompatibleProblemTypes { get; set; } = { ProblemType.Assemble };
    }

    public class ProblemSolutionPair
    {
        public Problem Problem { get; set; }
        public Solution Solution { get; set; }
    }
}