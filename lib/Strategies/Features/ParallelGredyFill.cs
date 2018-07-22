using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class ParallelGredyFill : SimpleSingleBotStrategyBase
    {
        public ParallelGredyFill(DeluxeState state, Bot bot)
            : base(state, bot)
        {
        }

        protected override async StrategyTask<bool> Run()
        {
            await new Cloning(state, bot);

            var strategies = state.Bots.Select(b => (IStrategy)new GreedyFill(state, b, new ThrowableHelperFast(state.TargetMatrix))).ToArray();
            await WhenAll(strategies);

            return true;
        }
    }
}