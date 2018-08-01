using System.Collections.Generic;
using System.Linq;
using System.Threading;

using lib.Commands;
using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public abstract class Strategy : IStrategy
    {
        private static readonly AsyncLocal<int> level = new AsyncLocal<int>();
        protected readonly State state;
        private readonly AsyncTicker ticker;

        protected Strategy(State state)
        {
            this.state = state;
            ticker = new AsyncTicker(Run, 1000);
        }

        protected abstract StrategyTask<bool> Run();

        public StrategyStatus Status { get; private set; }

        public void Tick()
        {
            level.Value++;
            Log.For(this).Info($"{new string(' ', level.Value * 2)}{ToString()}");
            Status = ticker.Tick();
            Log.For(this).Info($"{new string(' ', level.Value * 2)}{ToString()} => {Status}");
            level.Value--;
        }

        protected StrategyTask WhenAll(params IStrategy[] strategies)
        {
            return new StrategyTask(strategies, WaitType.WaitAll);
        }

        protected StrategyTask WhenAll(IEnumerable<IStrategy> strategies)
        {
            return new StrategyTask(strategies.ToArray(), WaitType.WaitAll);
        }

        protected StrategyTask WhenAny(params IStrategy[] strategies)
        {
            return new StrategyTask(strategies, WaitType.WaitAny);
        }

        protected StrategyTask WhenAny(IEnumerable<IStrategy> strategies)
        {
            return new StrategyTask(strategies.ToArray(), WaitType.WaitAny);
        }

        protected StrategyTask WhenNextTurn()
        {
            return new StrategyTask(null, default);
        }

        protected IStrategy Do(Bot bot, ICommand command)
        {
            return new Do(state, bot, command);
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

        protected IStrategy Halt()
        {
            return Do(state.Bots.First(), new Halt());
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}