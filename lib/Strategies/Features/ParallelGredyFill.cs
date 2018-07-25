using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class ParallelGredyFill : BotStrategy
    {
        public ParallelGredyFill(State state, Bot bot)
            : base(state, bot)
        {
        }

        protected override async StrategyTask<bool> Run()
        {
            var split = new Split(state, bot, 40);
            await split;

            await new Spread(state, split.Bots);


            var helper = new ThrowableHelperFast(state.TargetMatrix);
            var candidates = new HashSet<Vec>(state.GetGroundedCellsToBuild());
            
            await WhenAll(split.Bots.Select(b => new CooperativeGreedyFill(state, b, candidates)));

            return await Finalize();
        }
    }
}