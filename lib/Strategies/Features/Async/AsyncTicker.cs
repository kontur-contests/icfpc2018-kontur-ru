using System;
using System.Linq;

namespace lib.Strategies.Features.Async
{
    public class AsyncTicker
    {
        private readonly Func<StrategyTask<bool>> run;
        private StrategyTask<bool> task;

        public AsyncTicker(Func<StrategyTask<bool>> run)
        {
            this.run = run;
        }

        public StrategyResult Tick()
        {
            if (task?.Strategies != null && task.Strategies.Any(s => s.Status == StrategyStatus.InProgress))
                return new StrategyResult(StrategyStatus.InProgress, null);

            if (task == null)
                task = run();
            else
                task.Continue();

            if (task.IsComplete)
                return task.Result ? new StrategyResult(StrategyStatus.Done, null) : new StrategyResult(StrategyStatus.Failed, null);
            return new StrategyResult(StrategyStatus.InProgress, task.Strategies);
        }
    }
}