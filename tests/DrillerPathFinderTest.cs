using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

using lib.Models;
using lib.Strategies;
using lib.Strategies.Features;
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

        [Test]
        public void Test()
        {
            var s = @"2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[5 at 9 2 3], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[5 at 9 2 3], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[6 at 0 1 1], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[6 at 0 1 1], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[7 at 0 2 7], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[7 at 0 2 7], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[8 at 0 0 2], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[8 at 0 0 2], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[9 at 1 1 1], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[9 at 1 1 1], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[10 at 4 3 1], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[10 at 4 3 1], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[12 at 1 0 11], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[12 at 1 0 11], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[13 at 1 1 5], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[13 at 1 1 5], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[14 at 1 0 52], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[14 at 1 0 52], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[15 at 1 0 5], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[15 at 1 0 5], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[16 at 1 0 2], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[16 at 1 0 2], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[17 at 10 2 7], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[17 at 10 2 7], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[18 at 1 1 36], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[18 at 1 1 36], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[19 at 1 0 1], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[19 at 1 0 1], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[20 at 0 2 0], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[20 at 0 2 0], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[21 at 2 0 14], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[21 at 2 0 14], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[22 at 4 0 1], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[22 at 4 0 1], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[23 at 1 0 58], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[23 at 1 0 58], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[25 at 1 2 60], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[25 at 1 2 60], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[26 at 2 0 3], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[26 at 2 0 3], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[27 at 0 3 2], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[27 at 0 3 2], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[28 at 5 3 0], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[28 at 5 3 0], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[29 at 0 0 12], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[29 at 0 0 12], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[30 at 13 2 13], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[30 at 13 2 13], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[31 at 4 2 8], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[31 at 4 2 8], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[32 at 0 2 1], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[32 at 0 2 1], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[33 at 0 0 9], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[33 at 0 0 9], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[34 at 7 2 9], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[34 at 7 2 9], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[35 at 0 3 3], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[35 at 0 3 3], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[36 at 5 17 3], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[36 at 5 17 3], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[37 at 9 2 2], target: 0 0 1
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[37 at 9 2 2], target: 0 0 1 => Failed
2018-08-02 20:55:57.546 INFO  [NonParallelWorker]       ReachTarget[38 at 3 0 1], target: 0 0 1
2018-08-02 20:55:57.547 INFO  [NonParallelWorker]       ReachTarget[38 at 3 0 1], target: 0 0 1 => Failed
2018-08-02 20:55:57.547 INFO  [NonParallelWorker]       ReachTarget[39 at 14 2 8], target: 0 0 1
2018-08-02 20:55:57.547 INFO  [NonParallelWorker]       ReachTarget[39 at 14 2 8], target: 0 0 1 => Failed
2018-08-02 20:55:57.547 INFO  [NonParallelWorker]       ReachTarget[40 at 4 0 30], target: 0 0 1
2018-08-02 20:55:57.547 INFO  [NonParallelWorker]       ReachTarget[40 at 4 0 30], target: 0 0 1 => Failed
";
            var problem = ProblemSolutionFactory.LoadProblem("FA097");
            var state = new State(problem.TargetMatrix, problem.TargetMatrix);
            
            state.Bots.Clear();
            state.Bots.Add(new Bot {Bid = 24, Position = Vec.Zero, Seeds = new List<int>()});

            var ms = new Regex(@"ReachTarget\[(?<bid>\d+) at (?<x>\d+) (?<y>\d+) (?<z>\d+)\], target: 0 0 1 => Failed").Matches(s);
            var strategies = new List<ReachTarget>();
            foreach (Match m in ms)
            {
                var bot = new Bot
                    {
                        Bid = int.Parse(m.Groups["bid"].Value),
                        Position = new Vec(int.Parse(m.Groups["x"].Value), int.Parse(m.Groups["y"].Value), int.Parse(m.Groups["z"].Value)),
                        Seeds = new List<int>()
                    };
                state.Bots.Add(bot);
                strategies.Add(new ReachTarget(state, bot, new Vec(0, 0, 1)));
            }

            state.StartTick();
            foreach (var strategy in strategies)
            {
                strategy.Tick();
            }
            state.EndTick();

            if (strategies.All(ss => ss.Status == StrategyStatus.Failed))
            {
                throw new Exception("WTF???");
            }
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