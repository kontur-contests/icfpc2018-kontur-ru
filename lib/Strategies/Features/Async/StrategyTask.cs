using System;
using System.Runtime.CompilerServices;

namespace lib.Strategies.Features.Async
{
    public class StrategyTask
    {
        private readonly IStrategy[] strategies;

        public StrategyTask(IStrategy[] strategies)
        {
            this.strategies = strategies;
        }

        public StrategyAwaiter GetAwaiter()
        {
            return new StrategyAwaiter(strategies);
        }
    }

    [AsyncMethodBuilder(typeof(StrategyTaskBuilder<>))]
    public class StrategyTask<T>
    {
        public IStrategy[] Strategies { get; set; }

        public T Result { get; set; }

        public bool IsComplete { get; set; }

        public Action Continue { get; set; }
    }
}