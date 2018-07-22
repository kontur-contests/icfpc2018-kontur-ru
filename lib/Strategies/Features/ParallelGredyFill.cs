using System.Collections.Generic;
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
            var split = new Split(state, bot, 40);
            await split;
            
            var helper = new ThrowableHelperFast(state.TargetMatrix);
            var candidates = new HashSet<Vec>(state.GetGroundedCellsToBuild());

            var strategies = split.Bots.Select(b => (IStrategy)new CooperativeGreedyFill(state, b, candidates)).ToArray();
            await WhenAll(strategies);

            return await Finalize();
        }
    }
}