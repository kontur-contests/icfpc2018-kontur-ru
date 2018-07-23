using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using lib.Commands;
using lib.Models;
using lib.Strategies;
using lib.Utils;

using MoreLinq;

using Nest;

using NUnit.Framework;

namespace tests
{
    public class ElasticTestResult
    {
        //public string Id { get; set; }
        public string TestName { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public long Energy { get; set; }
        public DateTime StartTime { get; set; }
        public string AlgoVersion { get; set; }
        public string ScopeName { get; set; }
    }

    [TestFixture]
    public class DivideAndConquerStrategyTest
    {
        [TestCaseSource(nameof(GetModels))]
        [Explicit]
        public void TestDivideAndConquer(string filename)
        {
            DoRealTest(model => new DivideAndConquer(model, true),
                       (solver, commands, _) =>
                           {
                               foreach (var command in solver.Solve())
                                   commands.Add(command);
                               return ((DivideAndConquer)solver).State.Energy;
                           },
                       null,
                       "lowceil-5x4-bbox",
                       filename);
        }

        [TestCaseSource(nameof(GetModels))]
        [Explicit]
        public void TestHorizontalSlicer(string filename)
        {
            DoRealTest(model => new HorizontalSlicer(model, 6, 6, useBoundingBox : true),
                       (solver, commands, model) =>
                           {
                               var state = new DeluxeState(new Matrix(model.R), model);
                               var queue = new Queue<ICommand>();
                               var interpreter = new Interpreter(state);
                               foreach (var command in solver.Solve())
                               {
                                   commands.Add(command);
                                   queue.Enqueue(command);
                                   if (state.Bots.Count <= queue.Count)
                                       interpreter.Tick(queue);
                               }
                               interpreter.EnsureIsFinal();
                               return state.Energy;
                           },
                       "horizontal-slicer",
                       "bbox-slicer-remove-sticks-lmoves",
                       filename);
        }

        [TestCaseSource(nameof(GetModels))]
        [Explicit]
        public void TestHorizontalSlicerByLines(string filename)
        {
            var fast = true;
            var suffix = fast ? "-fast" : "";
            DoRealTest(model => new HorizontalSlicerByLines(model, 20, 1, useBoundingBox : true, fast:fast), 
                       (solver, commands, model) =>
                           {
                               var state = new DeluxeState(new Matrix(model.R), model);
                               var queue = new Queue<ICommand>();
                               var interpreter = new Interpreter(state);
                               foreach (var command in solver.Solve())
                               {
                                   commands.Add(command);
                                   queue.Enqueue(command);
                                   if (state.Bots.Count <= queue.Count)
                                       interpreter.Tick(queue);
                               }
                               interpreter.EnsureIsFinal();
                               return state.Energy;
                           },
                       "horizontal-slicer",
                       "bbox-slicer-remove-sticks-lmoves-lines" + suffix,
                       filename);
        }


        [TestCaseSource(nameof(GetDeconstructionModels))]
        [Explicit]
        public void TestFastDeconstructor(string filename)
        {
            DoRealTest(model => new FastDeconstructor(model), 
                       (solver, commands, model) =>
                           {
                               var state = new DeluxeState(model, new Matrix(model.R));
                               var queue = new Queue<ICommand>();
                               var interpreter = new Interpreter(state);
                               foreach (var command in solver.Solve())
                               {
                                   commands.Add(command);
                                   queue.Enqueue(command);
                                   if (state.Bots.Count <= queue.Count)
                                       interpreter.Tick(queue);
                               }
                               interpreter.EnsureIsFinal();
                               return state.Energy;
                           },
                       "fast-deconstructor",
                       "not-so-super-crew",
                       filename);
        }

        [TestCaseSource(nameof(GetDeconstructionModels))]
        [Explicit]
        public void TestNoWallsDeconstructor(string filename)
        {
            DoRealTest(model => new NoWallsDeconstructor(model), 
                       (solver, commands, model) =>
                           {
                               var state = new DeluxeState(model, new Matrix(model.R));
                               var queue = new Queue<ICommand>();
                               var interpreter = new Interpreter(state);
                               foreach (var command in solver.Solve())
                               {
                                   commands.Add(command);
                                   queue.Enqueue(command);
                                   if (state.Bots.Count <= queue.Count)
                                       interpreter.Tick(queue);
                               }
                               interpreter.EnsureIsFinal();
                               return state.Energy;
                           },
                       "fast-deconstructor",
                       "no-walls",
                       filename);
        }

        public void DoRealTest(Func<Matrix, IAmSolver> solverFactory,
                               Func<IAmSolver, List<ICommand>, Matrix, long> energyFactory,
                               string scopeName,
                               string algoName,
                               [NotNull] string filename)
        {
            var sw = Stopwatch.StartNew();
            var startTime = DateTime.Now;
            Console.WriteLine(filename);
            var content = File.ReadAllBytes(filename);
            var model = Matrix.Load(content);
            var solver = solverFactory(model);

            var shortname = Path.GetFileNameWithoutExtension(filename);
            List<ICommand> commands = new List<ICommand>();
            long energy;
            var targetDirectory = "failed";
            try
            {
                energy = energyFactory(solver, commands, model);
                targetDirectory = "res";
            }
            finally
            {
                File.WriteAllBytes($"C:\\workspace\\icfpc\\{targetDirectory}\\{shortname}-test.nbt", CommandSerializer.Save(commands.ToArray()));
            }
            sw.Stop();

            if (targetDirectory == "failed")
                return;
            var testResult = new ElasticTestResult
                {
                    TestName = shortname,
                    TimeSpent = sw.Elapsed,
                    Energy = energy,
                    StartTime = startTime,
                    AlgoVersion = algoName,
                    ScopeName = scopeName,
                };

            const string elasticUrl = "http://efk2-elasticsearch9200.efk2.10.217.14.7.xip.io";

            var client = new ElasticClient(new ConnectionSettings(new Uri(elasticUrl)).DisableDirectStreaming().DefaultMappingFor<ElasticTestResult>(x => x.IndexName("localrunresults")));

            client.IndexDocument(testResult);

            var searchResponse = client.Search<ElasticTestResult>(s => s.Size(10000).Query(q => q.Bool(b => b.Filter(bs => bs.Term(t => t.Field("testName.keyword").Value(shortname))))));

            var results = (searchResponse?.Documents?.ToList() ?? new List<ElasticTestResult>()).Where(d => d.ScopeName == scopeName).ToList();

            var minEnergyRes = results.OrderBy(x => x.Energy).FirstOrDefault() ?? testResult;
            var a = results.Where(x => x.AlgoVersion != testResult.AlgoVersion).OrderBy(x => x.Energy).FirstOrDefault();
            if (minEnergyRes.Energy < energy)
            {
                Assert.Warn($"Not the best energy ({minEnergyRes.Energy} < {energy} in {minEnergyRes.AlgoVersion})");
            }
            else if (a?.Energy > energy)
            {
                Assert.Pass($"New best energy ({a.Energy * 1.0 / energy:##.00} : {a.Energy} > {energy} - prev in {a.AlgoVersion})");
            }

            Console.WriteLine($"Energy: {energy}");
        }

        private static IEnumerable<TestCaseData> GetModels()
        {
            var problemsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/problemsF");
            foreach (string file in Directory.EnumerateFiles(problemsDir, "FA*.mdl"))
            {
                yield return new TestCaseData(file).SetName(Path.GetFileNameWithoutExtension(file));
            }
        }

        private static IEnumerable<TestCaseData> GetDeconstructionModels()
        {
            var problemsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/problemsF");
            foreach (string file in Directory.EnumerateFiles(problemsDir, "FD*.mdl"))
            {
                yield return new TestCaseData(file).SetName(Path.GetFileNameWithoutExtension(file));
            }
        }
        
    }
}