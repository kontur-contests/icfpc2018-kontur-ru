using System;
using System.Linq;

namespace lib.Strategies.Features.Async
{
    public class AsyncTicker
    {
        private readonly Func<StrategyTask<bool>> run;
        private readonly int attempts;
        private StrategyTask<bool> task;

        public AsyncTicker(Func<StrategyTask<bool>> run, int attempts)
        {
            this.run = run;
            this.attempts = attempts;
        }

        public StrategyStatus Tick()
        {
            for (int i = 0; i < attempts; i++)
            {
                if (task?.Strategies != null)
                {
                    foreach (var strategy in task.Strategies)
                        strategy.Tick();
                    switch (task.WaitType)
                    {
                        case WaitType.WaitAll:
                            task.Strategies.RemoveAll(s => s.Status != StrategyStatus.InProgress);
                            if (task.Strategies.Any())
                                return StrategyStatus.InProgress;
                            break;
                        case WaitType.WaitAny:
                            if (task.Strategies.All(s => s.Status == StrategyStatus.InProgress))
                                return StrategyStatus.InProgress;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                if (task == null)
                    task = run();
                else
                    task.Continue();
                if (task.IsComplete)
                    return task.Result ? StrategyStatus.Done : StrategyStatus.Failed;

                if (task.Strategies == null)
                    return StrategyStatus.InProgress;
            }
            return StrategyStatus.InProgress;
        }
    }
}