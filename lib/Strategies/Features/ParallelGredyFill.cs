using System.Collections.Generic;
using System.Linq;

using lib.Commands;
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
            await new Split(state, bot);

            var helper = new ThrowableHelperFast(state.TargetMatrix);
            var candidates = new HashSet<Vec>(state.GetGroundedCellsToBuild());

            var strategies = state.Bots.Select(b => (IStrategy)new CooperativeGreedyFill(state, b, helper, candidates)).ToArray();
            await WhenAll(strategies);

            await new Merging(state);
            await new MoveSingleBot(state, state.Bots.Single(), Vec.Zero);
            await Do(new Halt());
            return true;
        }
    }
}