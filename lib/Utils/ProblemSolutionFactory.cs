using System;
using System.IO;
using System.Linq;

using lib.Models;
using lib.Strategies;

using Nest;

namespace lib.Utils
{
    public static class ProblemSolutionFactory
    {
        public static ProblemSolutionPair[] GetTasks()
        {
            return GetProblems()
                .Select(x => Tuple.Create(x, GetSolutions(x)))
                .SelectMany(x => x.Item2.Select(y => new ProblemSolutionPair
                {
                    Problem = x.Item1,
                    Solution = y
                }))
                .ToArray();
        }

        private static Problem[] GetProblems()
        {
            return Directory
                .EnumerateFiles(FileHelper.ProblemsDir)
                .ToArray()
                .Where(x => Path.GetFileName(x).StartsWith("FA")) // TODO learn to disassemble/reassemble
                .Where(x => string.Equals(Path.GetExtension(x), ".mdl"))
                .Select(x => new Problem
                {
                    FileName = x,
                    Name = Path.GetFileNameWithoutExtension(x),
                    Matrix = Matrix.Load(File.ReadAllBytes(x))
                })
                .ToArray();
        }

        private static Solution[] GetSolutions(Problem problem)
        {
            var R = problem.Matrix.R;

            var s1 = new Solution
            {
                Name = "GS + TH",
                Solver = () => new GreedyPartialSolver(
                                          problem.Matrix.Voxels,
                                          new bool[R, R, R],
                                          new Vec(0, 0, 0),
                                          new ThrowableHelper(problem.Matrix))
            };

            var s2 = new Solution
            {
                Name = "GS + TH Fast",
                Solver = () => new GreedyPartialSolver(
                                          problem.Matrix.Voxels,
                                          new bool[R, R, R],
                                          new Vec(0, 0, 0),
                                          new ThrowableHelperFast(problem.Matrix))
            };

            var s3 = new Solution
            {
                Name = "GS + TH AStar",
                Solver = () => new GreedyPartialSolver(
                                          problem.Matrix.Voxels,
                                          new bool[R, R, R],
                                          new Vec(0, 0, 0),
                                          new ThrowableHelperAStar(R))
            };
            var s4 = new Solution
            {
                Name = "GS + Layers",
                Solver = () => new GreedyPartialSolver(
                                          problem.Matrix.Voxels,
                                          new bool[R, R, R],
                                          new Vec(0, 0, 0),
                                          new ThrowableHelper(problem.Matrix),
                                          new BottomToTopBuildingAround())
            };

            var s5 = new Solution
            {
                Name = "Columns",
                Solver = () => new DivideAndConquer(problem.Matrix, false),
            };

            var s6 = new Solution
            {
                Name = "ColumnsBbx",
                Solver = () => new DivideAndConquer(problem.Matrix, true),
            };

            var greedyForLargeModels = new Solution
            {
                Name = "GreedyForLarge",
                Solver = () => new GreedyPartialSolver(
                                   problem.Matrix.Voxels,
                                   new bool[R, R, R],
                                   new Vec(0, 0, 0),
                                   new ThrowableHelper(problem.Matrix),
                                   new NearToFarBottomToTopBuildingAround())
            };

            return new[]
                {
                    //s1,
                    s2,
                    //s3,
                    s4,
                    s5,
                    s6,
                    greedyForLargeModels
                };
        }
    }

    public class Problem
    {
        public string FileName { get; set; }
        public string Name { get; set; }
        public Matrix Matrix { get; set; }
    }

    public class Solution
    {
        public string Name { get; set; }
        public Func<IAmSolver> Solver { get; set; }
    }

    public class ProblemSolutionPair
    {
        public Problem Problem { get; set; }
        public Solution Solution { get; set; }
    }
}