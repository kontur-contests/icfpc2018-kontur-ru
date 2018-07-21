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
    }

    [TestFixture]
    public class DivideAndConquerStrategyTest
    {
        [TestCaseSource(nameof(GetModels))]
        [Explicit]
        public void LoadSample([NotNull] string filename)
        {
            var sw = Stopwatch.StartNew();
            var startTime = DateTime.Now;
            Console.WriteLine(filename);
            var content = File.ReadAllBytes(filename);
            var model = Matrix.Load(content);
            var solver = new DivideAndConquer(model);
            var shortname = Path.GetFileNameWithoutExtension(filename);
            Exception exceptionInfo = null;
            var thread = new Thread(() =>
                {
                    exceptionInfo = DoTest(solver, shortname);
                });
            thread.Start();
            if (!thread.Join(TimeSpan.FromSeconds(10)))
            {
                thread.Abort();
                Assert.Fail("Test aborted due to timeout");
            }
            if (exceptionInfo != null)
                throw exceptionInfo;

            sw.Stop();
            var energy = solver.State.Energy;

            var testResult = new ElasticTestResult
                {
                    TestName = shortname,
                    TimeSpent = sw.Elapsed,
                    Energy = energy,
                    StartTime = startTime,
                    AlgoVersion = "lowceil-5x4",
                };

            const string elasticUrl = "http://efk2-elasticsearch9200.efk2.10.217.14.7.xip.io";
        
            var client = new ElasticClient(new ConnectionSettings(new Uri(elasticUrl)).DefaultMappingFor<ElasticTestResult>(x => x.IndexName("localrunresults")));

            client.IndexDocument(testResult);

            var searchResponse = client.Search<ElasticTestResult>(s => s.Query(q => q.Match(m => m.Field(f => f.TestName).Query(shortname))));

            var results = searchResponse?.Documents?.ToList() ?? new List<ElasticTestResult>();

            var minEnergyRes = results.OrderBy(x => x.Energy).FirstOrDefault() ?? testResult;
            var a = results.Where(x => x.AlgoVersion != testResult.AlgoVersion).OrderBy(x => x.Energy).FirstOrDefault();
            if (minEnergyRes.Energy < energy)
            {
                Assert.Fail($"Not the best energy ({minEnergyRes.Energy} < {energy} in {minEnergyRes.AlgoVersion})");
            }
            else if (a?.Energy > energy)
            {
                Assert.Pass($"New best energy ({a.Energy} > {energy} prev in {a.AlgoVersion})");
            }
            
            Console.WriteLine($"Energy: {solver.State.Energy}");
        }

        private Exception DoTest(DivideAndConquer solver, string shortname)
        {
            List<ICommand> commands = new List<ICommand>();
            try
            {
                foreach (var command in solver.Solve())
                {
                    commands.Add(command);
                }
                File.WriteAllBytes($"C:\\workspace\\icfpc\\res\\{shortname}-test.nbt", CommandSerializer.Save(commands.ToArray()));
                return null;
            }
            catch (Exception e)
            {
                File.WriteAllBytes($"C:\\workspace\\icfpc\\failed\\{shortname}-test.nbt", CommandSerializer.Save(commands.ToArray()));
                return e;
            }
        }

        private static IEnumerable<TestCaseData> GetModels()
        {
            var problemsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../data/problemsL");
            foreach (string file in Directory.EnumerateFiles(problemsDir, "*.mdl"))
            {
                yield return new TestCaseData(file).SetName(Path.GetFileNameWithoutExtension(file));
            }
        }
    }
}