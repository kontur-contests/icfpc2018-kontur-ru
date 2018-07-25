using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public abstract class Strategy : IStrategy
    {
        protected readonly State state;
        private readonly AsyncTicker ticker;

        protected Strategy(State state)
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

        protected StrategyTask WhenAll(params IStrategy[] strategies)
        {
            return new StrategyTask(strategies);
        }

        protected StrategyTask WhenAll(IEnumerable<IStrategy> strategies)
        {
            return new StrategyTask(strategies.ToArray());
        }

        protected StrategyTask WhenNextTurn()
        {
            return new StrategyTask(null);
        }

        protected int R => state.R;

        protected IStrategy Move(Bot bot, Vec target)
        {
            return new Move(state, bot, target);
        }

        protected IStrategy MergeNears(Bot master, Bot slave)
        {
            return new MergeTwoNears(state, master, slave);
        }

        protected IStrategy Finalize()
        {
            return new Finalize(state);
        }

        protected StrategyTask Halt()
        {
            state.SetBotCommand(state.Bots.First(), new Halt());
            return new StrategyTask(null);
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}