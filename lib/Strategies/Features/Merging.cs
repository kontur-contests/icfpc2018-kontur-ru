using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;

namespace lib.Strategies.Features
{
    public class Merging : SimpleStrategyBase
    {
        public Merging(DeluxeState state)
            : base(state)
        {
        }

        protected override async StrategyTask<bool> Run()
        {
            while (state.Bots.Count > 1)
            {
                var bots = state.Bots.ToList();
                var strategies = new List<IStrategy>();
                for (int i = 0; i + 1 < bots.Count; i += 2)
                    strategies.Add(new MergingTwo(state, bots[i], bots[i + 1]));

                await WhenAll(strategies.ToArray());
            }

            return true;
        }
    }
}