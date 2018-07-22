using System.Collections.Generic;

using lib.Models;
using lib.Strategies.Features.Async;

namespace lib.Strategies.Features
{
    public abstract class SimpleStrategyBase : IStrategy
    {
        protected readonly DeluxeState state;
        private readonly AsyncTicker ticker;

        protected SimpleStrategyBase(DeluxeState state)
        {
            this.state = state;
            ticker = new AsyncTicker(Run);
        }

        protected abstract StrategyTask<bool> Run();

        public StrategyStatus Status { get; private set; }

        public IStrategy[] Tick()
        {
            var tickerResult = ticker.Tick();
            Status = tickerResult.Status;
            return tickerResult.Strategies;
        }

        public StrategyTask WhenAll(params IStrategy[] strategies)
        {
            return new StrategyTask(strategies);
        }

        public StrategyTask WhenNextTurn()
        {
            return new StrategyTask(null);
        }
    }
}