using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using lib.ElasticDTO;
using lib.Utils;

using log4net;

using Nest;

namespace lib.Models
{
    public class Evaluator
    {
        public TaskRunMeta Run(Problem problem, Solution solution)
        {
            var result = new TaskRunMeta
                {
                    StartedAt = DateTime.UtcNow,
                    TaskName = problem.Name,
                    SolverName = solution.Name,
                };
            var timer = Stopwatch.StartNew();

            var solver = solution.Solver();

            var commands = solver.Solve().ToArray();
            var state = new DeluxeState(problem.SourceMatrix, problem.TargetMatrix);
            new Interpreter(state).Run(commands);

            result.SecondsSpent = (int)timer.Elapsed.TotalSeconds;
            result.EnergySpent = state.Energy;
            result.Solution = CommandSerializer.Save(commands).SerializeSolutionToString();
            result.IsSuccess = true;
            return result;
        }
    }

    public class ResultsPoster
    {
        const string elasticUrl = "http://efk2-elasticsearch9200.efk2.10.217.14.7.xip.io";
        const string elasticIndex = "testruns";
        private readonly ElasticClient client = new ElasticClient(new ConnectionSettings(new Uri(elasticUrl)).DisableDirectStreaming().DefaultIndex(elasticIndex));
        private static readonly ILog log = lib.Log.For("ResultsPoster");

        public void PostResult(TaskRunMeta result)
        {
            var indexingResult = client.IndexDocument(result);
            var tryCount = 1;
            while (!indexingResult.IsValid && tryCount < 5)
            {
                log.Error($"Failed to insert task {result.TaskName} into Elastic on {tryCount} try (success was {result.IsSuccess})");
                log.Error(indexingResult.DebugInformation);
                Thread.Sleep(TimeSpan.FromSeconds(10));
                indexingResult = client.IndexDocument(result);
                tryCount++;
            }
        }


    }
}