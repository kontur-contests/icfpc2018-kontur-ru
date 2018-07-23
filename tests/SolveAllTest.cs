using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using JetBrains.Annotations;

using lib;
using lib.Commands;
using lib.Models;
using lib.Strategies;
using lib.Strategies.Features;
using lib.Utils;

using MoreLinq;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    public class SolveAllTest
    {
        [Test]
        [Explicit]
        public void Temp([Values(3)] int problemId)
        {
            var problem = ProblemSolutionFactory.LoadProblem($"FR{problemId:D3}");
            //var assembler = new GreedyPartialSolver(problem.TargetMatrix, new ThrowableHelperFast(problem.TargetMatrix));
            //var disassembler = new InvertorDisassembler(new GreedyPartialSolver(problem.SourceMatrix, new ThrowableHelperFast(problem.SourceMatrix)), problem.SourceMatrix);
            //var solver = new SimpleReassembler(disassembler, assembler);
            var commonPart = problem.SourceMatrix.Intersect(problem.TargetMatrix);
            commonPart = new ComponentTrackingMatrix(commonPart).GetGroundedVoxels();

            File.WriteAllBytes(Path.Combine(FileHelper.ProblemsDir, "FR666_tgt.mdl"), commonPart.Save());
            File.WriteAllBytes(Path.Combine(FileHelper.ProblemsDir, "FR666_src.mdl"), commonPart.Save());
            var solver = new GreedyPartialSolver(problem.SourceMatrix, commonPart, new ThrowableHelperFast(commonPart, problem.SourceMatrix));
            List<ICommand> commands = new List<ICommand>();
            try
            {
                foreach (var command in solver.Solve())
                {
                    commands.Add(command);
                }
                //commands.AddRange(solver.Solve());
            }
            catch (Exception e)
            {
                Log.For(this).Error($"Unhandled exception in solver for {problem.Name}", e);
                throw;
            }
            finally
            {
                Console.WriteLine(commands.Take(5000).ToDelimitedString("\n"));
                var bytes = CommandSerializer.Save(commands.ToArray());
                File.WriteAllBytes(GetSolutionPath(FileHelper.SolutionsDir, "FR666"), bytes);
            }
        }

        [Test]
        [Explicit]
        public void Reassemble([Values(75)] int problemId)
        {
            var problem = ProblemSolutionFactory.LoadProblem($"FR{problemId:D3}");
            //var assembler = new GreedyPartialSolver(problem.TargetMatrix, new ThrowableHelperFast(problem.TargetMatrix));
            //var disassembler = new InvertorDisassembler(new GreedyPartialSolver(problem.SourceMatrix, new ThrowableHelperFast(problem.SourceMatrix)), problem.SourceMatrix);
            var solver = new SimpleReassembler(
                new InvertorDisassembler(ProblemSolutionFactory.CreateSlicer6x6(problem.SourceMatrix), problem.SourceMatrix),
                ProblemSolutionFactory.CreateSlicer6x6(problem.TargetMatrix),
                problem.SourceMatrix,
                problem.TargetMatrix
                );
            //var solver = new SmartReassembler(
            //    problem.SourceMatrix,
            //    problem.TargetMatrix,
            //    (s, t) => new InvertorDisassembler(new GreedyPartialSolver(s, t, new ThrowableHelperFast(t, s)), s, t),
            //    (s, t) => new GreedyPartialSolver(t, s, new ThrowableHelperFast(s, t))
            //    );
            List<ICommand> commands = new List<ICommand>();
            try
            {
                foreach (var command in solver.Solve())
                {
                    commands.Add(command);
                }
                //commands.AddRange(solver.Solve());
            }
            catch (Exception e)
            {
                Log.For(this).Error($"Unhandled exception in solver for {problem.Name}", e);
                throw;
            }
            finally
            {
                Console.WriteLine(commands.Take(5000).ToDelimitedString("\n"));
                var bytes = CommandSerializer.Save(commands.ToArray());
                File.WriteAllBytes(GetSolutionPath(FileHelper.SolutionsDir, problem.Name), bytes);
            }
            var state = new DeluxeState(problem.SourceMatrix, problem.TargetMatrix);
            new Interpreter(state).Run(commands);
        }

        [Test]
        [Explicit]
        [Timeout(60000)]
        public void AssembleKung()
        {
            var problem = ProblemSolutionFactory.LoadProblem("FA060");
            var state = new DeluxeState(problem.SourceMatrix, problem.TargetMatrix);
            var solver = new Solver(state, new ParallelGredyFill(state, state.Bots.First()));
            List<ICommand> commands = new List<ICommand>();
            try
            {
                commands.AddRange(solver.Solve());
            }
            catch (Exception e)
            {
                Log.For(this).Error($"Unhandled exception in solver for {problem.Name}", e);
                throw;
            }
            finally
            {
                var bytes = CommandSerializer.Save(commands.ToArray());
                File.WriteAllBytes(GetSolutionPath(FileHelper.SolutionsDir, problem.Name), bytes);
            }
            Console.Out.WriteLine(state.Energy);
        }

        [Test]
        public void Test()
        {
            var a = new Region(new Vec(7, 0, 7), new Vec(7, 0, 7));
            var b = new Region(new Vec(7, 1, 7), new Vec(7, 1, 7));
            GenPlanSorter.Connected(a, b).Should().Be(true);
        }

        [Test]
        public void Test2()
        {
            var problem = ProblemSolutionFactory.LoadProblem("FA001");
            var state = new DeluxeState(problem.SourceMatrix, problem.TargetMatrix);
            var plan = new GenPlanBuilder(state).CreateGenPlan();
            var sorted = new GenPlanSorter(plan, state.R).Sort();
            sorted.ToHashSet().SetEquals(plan).Should().BeTrue();
        }

        [Test]
        [Explicit]
        //[Timeout(30000)]
        public void AssembleSpaceorc()
        {
            var problem = ProblemSolutionFactory.LoadProblem("FA001");
            var state = new DeluxeState(problem.SourceMatrix, problem.TargetMatrix);
            var solver = new Solver(state, new Assembler(state));
            List<ICommand> commands = new List<ICommand>();
            try
            {
                commands.AddRange(solver.Solve());
            }
            catch (Exception e)
            {
                Log.For(this).Error($"Unhandled exception in solver for {problem.Name}", e);
                throw;
            }
            finally
            {
                var bytes = CommandSerializer.Save(commands.ToArray());
                File.WriteAllBytes(GetSolutionPath(FileHelper.SolutionsDir, problem.Name), bytes);
            }
            Console.Out.WriteLine(state.Energy);
        }

        [Test]
        [Explicit]
        //[Timeout(30000)]
        public void Assemble()
        {
            var problem = ProblemSolutionFactory.LoadProblem("FA011");
            //var solver = new DivideAndConquer(problem.SourceMatrix, false);
            var state = new DeluxeState(problem.SourceMatrix, problem.TargetMatrix);
            var solver = new Solver(state, new AssembleFA11(state));
            List<ICommand> commands = new List<ICommand>();
            try
            {
                commands.AddRange(solver.Solve());
            }
            catch (Exception e)
            {
                Log.For(this).Error($"Unhandled exception in solver for {problem.Name}", e);
                throw;
            }
            finally
            {
                var bytes = CommandSerializer.Save(commands.ToArray());
                File.WriteAllBytes(GetSolutionPath(FileHelper.SolutionsDir, problem.Name), bytes);
            }
        }
        [Test]
        [Explicit]
        public void Disassemble()
        {
            var problem = ProblemSolutionFactory.LoadProblem("FD120");
            //var solver = new InvertorDisassembler(new DivideAndConquer(problem.SourceMatrix, true), problem.SourceMatrix);
            //var solver = new InvertorDisassembler(new GreedyPartialSolver(problem.SourceMatrix, new Matrix(problem.R), new ThrowableHelperFast(problem.SourceMatrix)), problem.SourceMatrix);
            var solver = new InvertorDisassembler(new HorizontalSlicer(problem.SourceMatrix, 6, 6, true), problem.SourceMatrix);
            //var solver = new HorizontalSlicer(problem.SourceMatrix, 6, 6, true);
            List<ICommand> commands = new List<ICommand>();
            try
            {
                commands.AddRange(solver.Solve());
            }
            catch (Exception e)
            {
                Log.For(this).Error($"Unhandled exception in solver for {problem.Name}", e);
                throw;
            }
            finally
            {
                var bytes = CommandSerializer.Save(commands.ToArray());
                File.WriteAllBytes(GetSolutionPath(FileHelper.SolutionsDir, problem.Name), bytes);
            }
            var state = new DeluxeState(problem.SourceMatrix, problem.TargetMatrix);
            new Interpreter(state).Run(commands);
        }

        [Test]
        [Explicit]
        //[Timeout(30000)]
        public void SolveOne(
            [Values(122)] int problemId
            //[ValueSource(nameof(Problems))] int problemId
            )
        {
            var problemsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/problemsF");
            var resultsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/solutions");
            var problemFile = Path.Combine(problemsDir, $"FD{problemId.ToString().PadLeft(3, '0')}_src.mdl");
            var matrix = Matrix.Load(File.ReadAllBytes(problemFile));
            var R = matrix.R;
            //var solver = new GreedyPartialSolver(matrix.Voxels, new bool[R, R, R], new Vec(0, 0, 0), new ThrowableHelper(matrix), new BottomToTopBuildingAround());
            var solver = new GreedyPartialSolver(matrix.Voxels, new bool[R, R, R], new Vec(0, 0, 0), new ThrowableHelper(matrix), new NearToFarBottomToTopBuildingAround());
            //var solver = new DivideAndConquer(matrix, true);
            List<ICommand> commands = new List<ICommand>();
            try
            {
                var sw = Stopwatch.StartNew();
                commands.AddRange(solver.Solve().TakeWhile(x => sw.Elapsed.TotalSeconds < 20));
                Console.WriteLine(GreedyPartialSolver.candidatesCount.ToDetailedString());
            }
            catch (Exception e)
            {
                Log.For(this).Error($"Unhandled exception in solver for {Path.GetFileName(problemFile)}", e);
                throw;
            }
            finally
            {
                var bytes = CommandSerializer.Save(commands.ToArray());
                File.WriteAllBytes(GetSolutionPath(resultsDir, problemFile), bytes);
            }

            var solutionEnergy = GetSolutionEnergy(matrix, commands.ToArray(), problemFile);
            Console.WriteLine(solutionEnergy);
            Console.WriteLine(ThrowableHelper.opt.ToDetailedString());
        }

        [Test]
        public void Solve()
        {
            var problemsDir = FileHelper.ProblemsDir;
            var resultsDir = FileHelper.SolutionsDir;
            var allProblems = Directory.EnumerateFiles(problemsDir, "*.mdl");
            var problems = allProblems.Select(p =>
                {
                    var matrix = Matrix.Load(File.ReadAllBytes(p));
                    return new { m = matrix, p, weight = matrix.Voxels.Cast<bool>().Count(b => b) };
                }).Take(10).ToList();
            problems.Sort((p1, p2) => p1.weight.CompareTo(p2.weight));
            Log.For(this).Info(string.Join("\r\n", problems.Select(p => p.p)));
            Parallel.ForEach(problems, p =>
                {
                    var R = p.m.R;
                    //var solver = new GreedyPartialSolver(p.m.Voxels, new bool[R, R, R], new Vec(0, 0, 0), new ThrowableHelper(p.m));
                    var solver = new GreedyPartialSolver(p.m.Voxels, new bool[R, R, R], new Vec(0, 0, 0), new ThrowableHelperFast(p.m));
                    //var solver = new DivideAndConquer(p.m);
                    //var solverName = "div-n-conq";
                    var solverName = "greedy-fst";
                    ICommand[] commands = null;
                    try
                    {
                        commands = solver.Solve().ToArray();
                    }
                    catch (Exception e)
                    {
                        Log.For(this).Error($"Unhandled exception in solver for {Path.GetFileName(p.p)}", e);
                        return;
                    }

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
                throw;
            }
        }

        private long Validate([NotNull] Matrix problem, [NotNull] ICommand[] solution)
        {
            var state = new DeluxeState(null, problem);
            new Interpreter(state).Run(solution);
            return state.Energy;
        }
    }
}