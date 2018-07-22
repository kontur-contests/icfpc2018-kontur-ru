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

        protected abstract IEnumerable<TickerResult> Run();

        public StrategyStatus Status { get; private set; }

        public IStrategy[] Tick()
        {
            var tickerResult = ticker.Tick();
            Status = tickerResult.Status;
            return tickerResult.Strategies;
        }

        protected TickerResult Wait(params IStrategy[] strategies)
        {
            return new TickerResult(StrategyStatus.Incomplete, strategies);
        }

        protected TickerResult Failed()
        {
            return new TickerResult(StrategyStatus.Failed, null);
        }
    }
}