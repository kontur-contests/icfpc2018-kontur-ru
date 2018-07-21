using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Kontur.Houston.Plugin;

using lib.Commands;
using lib.ElasticDTO;
using lib.Models;
using lib.Utils;

using MoreLinq.Extensions;

using Nest;

namespace houston
{
    public class HoustonRunner : IPlugin<HoustonRunnerProperties>
    {
        public void Run(IPluginContext<HoustonRunnerProperties> context)
        {
            
            const string elasticUrl = "http://efk2-elasticsearch9200.efk2.10.217.14.7.xip.io";
            const string elasticIndex = "testruns";
            
            var replicaNumber = context.Info.Replica.ReplicaNumber;
            var replicaCount = context.Info.Replica.ReplicationFactor;

            if (replicaNumber == 0 || replicaCount == 0)
            {
                replicaNumber = 1;
                replicaCount = 1;
            }
            
            context.Log.Info($"Replica # {replicaNumber} of {replicaCount}: preparing...");

            var client = new ElasticClient(new ConnectionSettings(new Uri(elasticUrl)).DefaultIndex(elasticIndex));

            var tasks = ProblemSolutionFactory.GetTasks();

            var selectedTasks = tasks
                .Where(task => ((uint) task.Problem.Name.GetHashCode()) % replicaCount == replicaNumber - 1)
                .ToArray();

            context.Log.Info($"Replica # {replicaNumber} of {replicaCount}: " +
                             $"running {selectedTasks.Length} of {tasks.Length} tasks");

            while (!context.CancellationToken.IsCancellationRequested)
            {
                selectedTasks.ForEach(task =>
                    {
                        var solution = task.Solution.Invoke();
                        
                        var result = new TaskRunMeta
                            {
                                StartedAt = DateTime.UtcNow,
                                TaskName = task.Problem.Name,
                                SolverName = solution.Name,
                                RunningHostName = Environment.MachineName
                            };

                        context.Log.Info($"Task {result.TaskName} " +
                                         $"with solver {result.SolverName} " +
                                         $"at {result.RunningHostName}: " +
                                         $"starting...");

                        try
                        {
                            var timer = Stopwatch.StartNew();

                            var solver = solution.Solver;
                            var commands = solver.Solve().ToArray();

                            var state = new MutableState(task.Problem.Matrix);
                            var queue = new Queue<ICommand>(commands);
                            while (queue.Any())
                            {
                                state.Tick(queue);
                            }
                            state.EnsureIsFinal();

                            result.SecondsSpent = (int)timer.Elapsed.TotalSeconds;
                            result.EnergySpent = state.Energy;
                            result.EnergyHistory = state.EnergyHistory;
                            result.Solution = CommandSerializer.Save(commands).SerializeSolutionToString();
                            result.IsSuccess = true;
                        }
                        catch (Exception e)
                        {
                            context.Log.Warn($"Unhandled exception in solver for {task.Problem.FileName}");

                            result.IsSuccess = false;
                            result.ExceptionInfo = $"{e.Message}\n{e.StackTrace}";
                        }

                        context.Log.Info($"Task {result.TaskName} " +
                                         $"with solver {result.SolverName} " +
                                         $"at {result.RunningHostName}: " +
                                         $"completed in {result.SecondsSpent}s");

                        client.IndexDocument(result);
                    });

                Thread.Sleep(int.MaxValue);
            }
        }
    }
}