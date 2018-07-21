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
                .Where(x => string.Equals(Path.GetExtension(x), ".mdl"))
                .Select(x => new Problem
                    {
                        FileName = x,
                        Name = Path.GetFileNameWithoutExtension(x),
                        Matrix = Matrix.Load(File.ReadAllBytes(x))
                    })
                .ToArray();
        }

        private static Func<Solution>[] GetSolutions(Problem problem)
        {
            var R = problem.Matrix.R;

            Func<Solution> s1 = () => new Solution
                {
                    Name = "GS + TH",
                    Solver = new GreedyPartialSolver(
                                          problem.Matrix.Voxels,
                                          new bool[R, R, R],
                                          new Vec(0, 0, 0),
                                          new ThrowableHelper(problem.Matrix))
                };

            Func<Solution> s2 = () => new Solution
                {
                    Name = "GS + TH Fast",
                    Solver = new GreedyPartialSolver(
                                          problem.Matrix.Voxels,
                                          new bool[R, R, R],
                                          new Vec(0, 0, 0),
                                          new ThrowableHelperFast(R))
                };

            Func<Solution> s3 = () => new Solution
                {
                    Name = "GS + TH AStar",
                    Solver = new GreedyPartialSolver(
                                          problem.Matrix.Voxels,
                                          new bool[R, R, R],
                                          new Vec(0, 0, 0),
                                          new ThrowableHelperAStar(R))
                };

            return new[]
                {
                    s1,
                    s2,
                    s3
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
        public GreedyPartialSolver Solver { get; set; }
    }

    public class ProblemSolutionPair
    {
        public Problem Problem { get; set; }
        public Func<Solution> Solution { get; set; }
    }
}