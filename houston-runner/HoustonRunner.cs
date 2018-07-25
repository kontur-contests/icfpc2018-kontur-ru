using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
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

            var client = new ElasticClient(new ConnectionSettings(new Uri(elasticUrl)).DisableDirectStreaming().DefaultIndex(elasticIndex));
            var tasks = ProblemSolutionFactory.GetTasks()
                                              .Where(t => t.Solution.ProblemPrioritizer(t.Problem) != ProblemPriority.DoNotSolve)
                                              .ToArray();

            var selectedTasks = tasks
                .Where(task => ((uint)(task.Problem.Name + task.Solution.Name).GetHashCode()) % replicaCount == replicaNumber - 1)
                .ToArray();

            context.Log.Info($"Replica # {replicaNumber} of {replicaCount}: " +
                             $"running {selectedTasks.Length} of {tasks.Length} tasks");

            var completeTasksCounter = 0;
            selectedTasks.OrderBy(st => st.Solution.ProblemPrioritizer(st.Problem))
                         .ForEach(task =>
                {
                    var solution = task.Solution;

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

                        var solver = solution.Solver(task.Problem);

                        var commands = new List<ICommand>();
                        var started = new ManualResetEvent(false);
                        ExceptionDispatchInfo exceptionDispatchInfo = null;
                        //task.Problem.Type
                        var total = (task.Problem.TargetMatrix?.Weight + task.Problem.SourceMatrix?.Weight) ?? 0;
                        int done = 0;
                        var timeout = Stopwatch.StartNew();
                        var runThread = new Thread(() =>
                            {
                                try
                                {
                                    var solverCommands = solver.Solve();
                                    foreach (var command in solverCommands)
                                    {
                                        started.Set();
                                        commands.Add(command);
                                        if (command is Fill)
                                            Interlocked.Increment(ref done);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
                                    started.Set();
                                }
                            });
                        runThread.Start();
                        if (!started.WaitOne(context.Properties.SolverStartTimeout))
                        {
                            runThread.Abort();
                            runThread.Join();
                            throw new TimeoutException("Solve start timeout expired");
                        }

                        while (!runThread.Join(context.Properties.SolverTimeoutMeasureInterval))
                        {
                            var localDone = Interlocked.CompareExchange(ref done, 0, 0);
                            if (localDone == 0)
                                throw new TimeoutException("Solver didn't fill any cells");

                            var estimatedTotalTime = TimeSpan.FromTicks(timeout.Elapsed.Ticks * total / localDone);
                            if (estimatedTotalTime > context.Properties.SolverTimeout)
                                throw new TimeoutException($"Solver total time estimation {estimatedTotalTime} exceeds limit {context.Properties.SolverTimeout}");
                        }

                        exceptionDispatchInfo?.Throw();

                        var state = new State(task.Problem.SourceMatrix, task.Problem.TargetMatrix);
                        new Interpreter(state).Run(commands);

                        result.SecondsSpent = (int)timer.Elapsed.TotalSeconds;
                        result.EnergySpent = state.Energy;
                        //result.EnergyHistory = state.EnergyHistory;
                        result.Solution = CommandSerializer.Save(commands.ToArray()).SerializeSolutionToString();
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

                    completeTasksCounter++;
                    context.Log.Info($"Tasks complete: {completeTasksCounter} of {selectedTasks.Length} for this worker");

                    var indexingResult = client.IndexDocument(result);
                    var tryCount = 1;
                    while (!indexingResult.IsValid && tryCount < 10)
                    {
                        context.Log.Warn($"Failed to insert task {result.TaskName} into Elastic on {tryCount} try (success was {result.IsSuccess})");
                        context.Log.Warn(indexingResult.DebugInformation);
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                        indexingResult = client.IndexDocument(result);
                        tryCount++;
                    }
                    if (!indexingResult.IsValid)
                    {
                        context.Log.Error($"TOTALLY FAILED to insert task {result.TaskName} into Elastic on {tryCount} try (success was {result.IsSuccess})");
                        context.Log.Error(indexingResult.DebugInformation);
                    }
                });

            context.Log.Info("Sleeping forever, all tasks done");
            Thread.Sleep(int.MaxValue);
        }
    }
}