using System.Collections.Generic;

using lib.Models;

namespace lib.Strategies.Features
{
    public abstract class SimpleStrategyBase : IStrategy
    {
        protected readonly DeluxeState state;
        private readonly Ticker ticker;

        protected SimpleStrategyBase(DeluxeState state)
        {
            this.state = state;
            ticker = new Ticker(Run);
        }

        protected abstract IEnumerable<StrategyResult> Run();

        public StrategyStatus Status { get; private set; }

        public IStrategy[] Tick()
        {
            var tickerResult = ticker.Tick();
            Status = tickerResult.Status;
            return tickerResult.Strategies;
        }

        protected StrategyResult Wait(params IStrategy[] strategies)
        {
            return new StrategyResult(StrategyStatus.Incomplete, strategies);
        }

        protected StrategyResult Failed()
        {
            return new StrategyResult(StrategyStatus.Failed, null);
        }
    }
}