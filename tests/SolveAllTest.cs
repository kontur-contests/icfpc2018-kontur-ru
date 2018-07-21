using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using JetBrains.Annotations;

using lib;
using lib.Commands;
using lib.Models;
using lib.Strategies;
using lib.Utils;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    public class SolveAllTest
    {
        [Test]
        [Explicit]
        public void SolveOne([Values(1)] int problemId)
        {
            var problemsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/problemsL");
            var resultsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/solutions");
            var problemFile = Path.Combine(problemsDir, $"LA{problemId.ToString().PadLeft(3, '0')}_tgt.mdl");
            var matrix = Matrix.Load(File.ReadAllBytes(problemFile));
            var R = matrix.R;
            var solver = new GreedyPartialSolver(matrix.Voxels, new bool[R, R, R], new Vec(0, 0, 0), new ThrowableHelperFast(matrix.N));
            try
            {
                solver.Solve();
                Console.WriteLine($"{GreedyPartialSolver.A} {GreedyPartialSolver.B}");
            }
            catch (Exception e)
            {
                Log.For(this).Error($"Unhandled exception in solver for {Path.GetFileName(problemFile)}", e);
                throw;
            }
            var commands = solver.Commands.ToArray();

            TryValidate(matrix, commands, problemFile);

            var bytes = CommandSerializer.Save(commands);
            File.WriteAllBytes(GetSolutionPath(resultsDir, problemFile), bytes);
        }

        [Test]
        public void Solve()
        {
            var problemsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/problemsL");
            var resultsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/solutions");
            var allProblems = Directory.EnumerateFiles(problemsDir, "*.mdl");
            var problems = allProblems.Where(p => !File.Exists(GetSolutionPath(resultsDir, p))).Select(p =>
                {
                    var matrix = Matrix.Load(File.ReadAllBytes(p));
                    return new {m = matrix, p, weight = matrix.Voxels.Cast<bool>().Count(b => b)};
                }).ToList();
            problems.Sort((p1, p2) => p1.weight.CompareTo(p2.weight));
            Log.For(this).Info(string.Join("\r\n", problems.Select(p => p.p)));
            Parallel.ForEach(problems, p =>
                {
                    var problem = p.m.Clone();
                    var R = p.m.N;
                    var solver = new GreedyPartialSolver(p.m.Voxels, new bool[R, R, R], new Vec(0, 0, 0), new ThrowableHelper(p.m));
                    try
                    {
                        solver.Solve();
                    }
                    catch (Exception e)
                    {
                        Log.For(this).Error($"Unhandled exception in solver for {Path.GetFileName(p.p)}", e);
                        return;
                    }
                    var commands = solver.Commands.ToArray();

                    TryValidate(problem, commands, p.p);

                    var bytes = CommandSerializer.Save(commands);
                    File.WriteAllBytes(GetSolutionPath(resultsDir, p.p), bytes);
                });
        }

        private static string GetSolutionPath(string resultsDir, string p)
        {
            return Path.Combine(resultsDir, GetSolutionFilename(p));
        }

        private static string GetSolutionFilename(string p)
        {
            return $"{Path.GetFileNameWithoutExtension(p).Split('_')[0]}.nbt";
        }

        private void TryValidate(Matrix problem, ICommand[] commands, string p)
        {
            try
            {
                Validate(problem, commands);
            }
            catch (Exception e)
            {
                Log.For(this).Error($"Invalid solution for {Path.GetFileName(p)}", e);
            }
        }

        private void Validate([NotNull] Matrix problem, [NotNull] ICommand[] solution)
        {
            var state = new MutableState(problem);
            var queue = new Queue<ICommand>(solution);
            while (queue.Any())
                state.Tick(queue);
            state.EnsureIsFinal();
        }
    }
}