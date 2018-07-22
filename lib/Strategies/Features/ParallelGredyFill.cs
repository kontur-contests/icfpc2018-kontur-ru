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

            var helper = new ThrowableHelperFast(state.TargetMatrix);
            var strategies = state.Bots.Select(b => (IStrategy)new CooperativeGreedyFill(state, b, helper)).ToArray();
            await WhenAll(strategies);

            return true;
        }
    }
}