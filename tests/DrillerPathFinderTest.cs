using System;
using System.Diagnostics;
using System.Linq;

using lib.Models;
using lib.Utils;

using NUnit.Framework;

namespace tests
{
    [TestFixture]
    public class DrillerPathFinderTest
    {
        private static string[] GetProblems()
        {
            return new[]
                {
                    "FA001",
                    "FA186",
                };
        }

        [TestCaseSource(nameof(GetProblems))]
        public void Test0(string problemName)
        {
            var problem = ProblemSolutionFactory.LoadProblem(problemName);
            var state = new State(problem.SourceMatrix, problem.TargetMatrix);

            var target = new Vec(state.R - 1, state.R - 1, state.R - 1);
            var pathFinder = new PathFinder(state, state.Bots.Single(), target);
            var sw = Stopwatch.StartNew();
            var steps = pathFinder.TryFindPath();
            sw.Stop();

            Console.Out.WriteLine($"PathLength: {steps.Count}; Elapsed: {sw.ElapsedMilliseconds}");
            foreach (var step in steps)
            {
                Console.Out.WriteLine(step);
            }
        }

        [TestCaseSource(nameof(GetProblems))]
        public void Test1(string problemName)
        {
            var problem = ProblemSolutionFactory.LoadProblem(problemName);
            var state = new State(problem.SourceMatrix, problem.TargetMatrix);

            var target = new Vec(state.R - 1, state.R - 1, state.R - 1);
            var pathFinder = new DrillerPathFinder(state, state.Bots.Single(), target);
            var sw = Stopwatch.StartNew();
            var steps = pathFinder.TryFindPath(false);
            sw.Stop();

            Console.Out.WriteLine($"PathLength: {steps.Count}; Elapsed: {sw.ElapsedMilliseconds}; Total: {pathFinder.TotalCells}; Closed: {pathFinder.ClosedCells} ({pathFinder.ClosedCells * 100 / pathFinder.TotalCells}%)");
            foreach (var step in steps)
            {
                Console.Out.WriteLine(step);
            }
        }

        [TestCaseSource(nameof(GetProblems))]
        public void Test2(string problemName)
        {
            var problem = ProblemSolutionFactory.LoadProblem(problemName);
            var state = new State(problem.SourceMatrix, problem.TargetMatrix);

            var target = new Vec(state.R - 1, state.R - 1, state.R - 1);
            state.Matrix[target] = true;

            var pathFinder = new DrillerPathFinder(state, state.Bots.Single(), target);
            var sw = Stopwatch.StartNew();
            var steps = pathFinder.TryFindPath(true);
            sw.Stop();

            Console.Out.WriteLine($"PathLength: {steps.Count}; Elapsed: {sw.ElapsedMilliseconds}; Total: {pathFinder.TotalCells}; Closed: {pathFinder.ClosedCells} ({pathFinder.ClosedCells * 100 / pathFinder.TotalCells}%)");
            foreach (var step in steps)
            {
                Console.Out.WriteLine(step);
            }
        }

        [TestCaseSource(nameof(GetProblems))]
        public void Test3(string problemName)
        {
            var problem = ProblemSolutionFactory.LoadProblem(problemName);
            var state = new State(problem.SourceMatrix, problem.TargetMatrix);

            var target = new Vec(state.R - 1, state.R - 1, state.R - 1);
            state.Matrix[target] = true;
            for (int x = 0; x < state.R; x++)
            for (int y = 1; y < state.R; y++)
            {
                state.Matrix[x, y, state.R - 2] = true;
            }

            var pathFinder = new DrillerPathFinder(state, state.Bots.Single(), target);
            var sw = Stopwatch.StartNew();
            var steps = pathFinder.TryFindPath(true);
            sw.Stop();

            Console.Out.WriteLine($"PathLength: {steps.Count}; Elapsed: {sw.ElapsedMilliseconds}; Total: {pathFinder.TotalCells}; Closed: {pathFinder.ClosedCells} ({pathFinder.ClosedCells * 100 / pathFinder.TotalCells}%)");
            foreach (var step in steps)
            {
                Console.Out.WriteLine(step);
            }
        }

        [TestCaseSource(nameof(GetProblems))]
        public void Test4(string problemName)
        {
            var problem = ProblemSolutionFactory.LoadProblem(problemName);
            var state = new State(problem.SourceMatrix, problem.TargetMatrix);

            var target = new Vec(state.R - 1, state.R - 1, state.R - 1);
            state.Matrix[target] = true;
            for (int x = 0; x < state.R; x++)
            for (int y = 1; y < state.R; y++)
            {
                state.Matrix[x, y, state.R - 2] = true;
            }

            var pathFinder = new DrillerPathFinder(state, state.Bots.Single(), target, 5);
            var sw = Stopwatch.StartNew();
            var steps = pathFinder.TryFindPath(true);
            sw.Stop();

            Console.Out.WriteLine($"PathLength: {steps.Count}; Elapsed: {sw.ElapsedMilliseconds}; Total: {pathFinder.TotalCells}; Closed: {pathFinder.ClosedCells} ({pathFinder.ClosedCells * 100 / pathFinder.TotalCells}%)");
            foreach (var step in steps)
            {
                Console.Out.WriteLine(step);
            }
        }
    }
}