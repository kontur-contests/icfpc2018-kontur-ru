using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace lib.Strategies.Features.Async
{
    public class StrategyAwaiter : INotifyCompletion
    {
        public StrategyAwaiter(IStrategy[] strategies, WaitType waitType)
        {
            WaitType = waitType;
            Strategies = strategies;
            if (strategies != null)
            {
                if (waitType == WaitType.WaitAny && !strategies.Any())
                    throw new InvalidOperationException("Couldn't wait any of empty list of strategies");
                if (waitType == WaitType.WaitAll && !strategies.Any())
                    IsCompleted = true;
            }
        }

        public WaitType WaitType { get; }

        public IStrategy[] Strategies { get; }

        public bool IsCompleted { get; }

        public bool GetResult() => Strategies == null 
                                   || WaitType == WaitType.WaitAll && Strategies.All(x => x.Status == StrategyStatus.Done)
                                   || WaitType == WaitType.WaitAny && Strategies.Any(x => x.Status == StrategyStatus.Done);

        public void OnCompleted(Action continuation)
        {
            continuation();
        }
    }
}