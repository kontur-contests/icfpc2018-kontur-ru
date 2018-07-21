using System;
using System.IO;
using System.Threading;

using Kontur.Houston.Plugin;

using lib;
using lib.ElasticDTO;
using lib.Models;
using lib.Strategies;
using lib.Utils;

using Nest;

namespace houston
{
    public class HoustonRunner : IPlugin<HoustonRunnerProperties>
    {
        public void Run(IPluginContext<HoustonRunnerProperties> context)
        {
            var client = new ElasticClient(new ConnectionSettings(new Uri("http://efk2-elasticsearch9200.efk2.10.217.14.7.xip.io")).DefaultIndex("testruns"));

            while (!context.CancellationToken.IsCancellationRequested)
            {
                var problemId = 1;
                var problemsDir = Path.Combine(Environment.CurrentDirectory, "../data/problemsL");
                var problemFileName = $"LA{problemId.ToString().PadLeft(3, '0')}_tgt.mdl";
                var matrix = Matrix.Load(File.ReadAllBytes(Path.Combine(problemsDir, problemFileName)));
                var R = matrix.R;

                var solver = new GreedyPartialSolver(matrix.Voxels, new bool[R, R, R], new Vec(0, 0, 0), new ThrowableHelper(matrix));
                var testResult = new TaskRunMeta
                    {
                        RunningHostName = Environment.MachineName,
                        SolverName = solver.GetType().Name,
                        StartedAt = DateTime.Now,
                        TaskName = problemFileName,
                    };
                context.Log.Info($"Starting task {testResult.TaskName} at {testResult.RunningHostName} with solver {testResult.SolverName}");

                try
                {
                    solver.Solve();

                    //TODO fill IsSuccess, EnergySpent

                    testResult.Solution = Convert.ToBase64String(CommandSerializer.Save(solver.Commands.ToArray()));
                }
                catch (Exception e)
                {
                    context.Log.Warn($"Unhandled exception in solver for {problemFileName}", e);
                }

                client.IndexDocument(testResult);

                Thread.Sleep(10000);
            }

            context.Log.Info("Bye!");
        }
    }
}