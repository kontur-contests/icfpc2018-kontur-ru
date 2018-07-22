using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace lib.Strategies.Features.Async
{
    public class StrategyAwaiter : INotifyCompletion
    {
        public StrategyAwaiter(IStrategy[] strategies)
        {
            Strategies = strategies;
        }

        public IStrategy[] Strategies { get; }

        public bool IsCompleted { get; private set; }

        public bool GetResult() => Strategies == null || Strategies.All(x => x.Status == StrategyStatus.Done);

        public void OnCompleted(Action continuation)
        {
            IsCompleted = true;
            continuation();
        }
    }
}