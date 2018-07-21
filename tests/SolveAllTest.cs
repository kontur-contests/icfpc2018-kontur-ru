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
        public static int[] Problems = Enumerable.Range(1, 100).ToArray();
        [Test]
        [Explicit]
        //[Timeout(30000)]
        public void SolveOne(
            [Values(40)] int problemId
            //[ValueSource(nameof(Problems))] int problemId
            )
        {
            var problemsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/problemsL");
            var resultsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/solutions");
            var problemFile = Path.Combine(problemsDir, $"LA{problemId.ToString().PadLeft(3, '0')}_tgt.mdl");
            var matrix = Matrix.Load(File.ReadAllBytes(problemFile));
            var R = matrix.R;
            var solver = new GreedyPartialSolver(matrix.Voxels, new bool[R, R, R], new Vec(0, 0, 0), new ThrowableHelper(matrix));
            try
            {
                solver.Solve(30000);
                Console.WriteLine($"{GreedyPartialSolver.A} {GreedyPartialSolver.B}");
            }
            catch (Exception e)
            {
                Log.For(this).Error($"Unhandled exception in solver for {Path.GetFileName(problemFile)}", e);
                throw;
            }
            var commands = solver.Commands.ToArray();

            var solutionEnergy = GetSolutionEnergy(matrix, commands, problemFile);
            Console.WriteLine(solutionEnergy);

            var bytes = CommandSerializer.Save(commands);
            File.WriteAllBytes(GetSolutionPath(resultsDir, problemFile), bytes);
            Console.WriteLine(ThrowableHelper.opt.ToDetailedString());
        }

        private int Estimate(Vec pos, Vec bot)
        {
            return 30 * pos.MDistTo(bot) + 4*pos.Y + pos.Z + pos.X;
        }

        [Test]
        public void Solve()
        {
            var problemsDir =  FileHelper.ProblemsDir;
            var resultsDir = FileHelper.SolutionsDir;
            var allProblems = Directory.EnumerateFiles(problemsDir, "*.mdl");
            var problems = allProblems.Select(p =>
                {
                    var matrix = Matrix.Load(File.ReadAllBytes(p));
                    return new {m = matrix, p, weight = matrix.Voxels.Cast<bool>().Count(b => b)};
                }).Skip(18).Take(3).ToList();
            problems.Sort((p1, p2) => p1.weight.CompareTo(p2.weight));
            Log.For(this).Info(string.Join("\r\n", problems.Select(p => p.p)));
            Parallel.ForEach(problems, p =>
                {
                    var R = p.m.N;
                    //var solver = new GreedyPartialSolver(p.m.Voxels, new bool[R, R, R], new Vec(0, 0, 0), new ThrowableHelper(p.m));
                    var solver = new DivideAndConquer(p.m);
                    var solverName = "div-n-conq";
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

                    var solutionEnergy = GetSolutionEnergy(p.m, commands, p.p);

                    var bytes = CommandSerializer.Save(commands);
                    File.WriteAllBytes(GetSolutionPath(resultsDir, p.p, solverName, solutionEnergy), bytes);
                });
        }

        private static string GetSolutionPath(string resultsDir, string p)
        {
            return Path.Combine(resultsDir, GetSolutionFilename(p));
        }

        private static string GetSolutionPath(string resultsDir, string p, string solverName, long solutionEnergy)
        {
            return Path.Combine(resultsDir, GetSolutionFilename(p, solverName, solutionEnergy));
        }

        private static string GetSolutionFilename(string problemName, string solverName, long solutionEnergy)
        {
            return $"{Path.GetFileNameWithoutExtension(problemName).Split('_')[0]}-{solverName}-{solutionEnergy}.nbt";
        }

        private static string GetSolutionFilename(string p)
        {
            return $"{Path.GetFileNameWithoutExtension(p).Split('_')[0]}.nbt";
        }

        private long GetSolutionEnergy(Matrix problem, ICommand[] commands, string p)
        {
            try
            {
                return Validate(problem, commands);
            }
            catch (Exception e)
            {
                Log.For(this).Error($"Invalid solution for {Path.GetFileName(p)}", e);
                return 0;
            }
        }

        private long Validate([NotNull] Matrix problem, [NotNull] ICommand[] solution)
        {
            var state = new MutableState(problem);
            var queue = new Queue<ICommand>(solution);
            while (queue.Any())
                state.Tick(queue);
            state.EnsureIsFinal();
            return state.Energy;
        }
    }
}