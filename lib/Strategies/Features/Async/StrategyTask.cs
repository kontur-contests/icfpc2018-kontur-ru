using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace lib.Strategies.Features.Async
{
    public class StrategyTask
    {
        private readonly IStrategy[] strategies;
        private readonly WaitType waitType;

        public StrategyTask(IStrategy[] strategies, WaitType waitType)
        {
            this.strategies = strategies;
            this.waitType = waitType;
        }

        public StrategyAwaiter GetAwaiter()
        {
            return new StrategyAwaiter(strategies, waitType);
        }
    }

    [AsyncMethodBuilder(typeof(StrategyTaskBuilder<>))]
    public class StrategyTask<T>
    {
        public WaitType WaitType { get; set; }

        public List<IStrategy> Strategies { get; set; }

        public T Result { get; set; }

        public bool IsComplete { get; set; }

        public Action Continue { get; set; }
    }
}