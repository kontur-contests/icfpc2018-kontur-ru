using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class SpreadToPositions : Strategy
    {
        private readonly List<Vec> targets;
        private readonly List<Bot> bots;

        public SpreadToPositions(State state, List<Bot> bots, List<Vec> targets)
            : base(state)
        {
            this.targets = targets;
            this.bots = bots;
        }

        protected override async StrategyTask<bool> Run()
        {
            while (true)
            {
                var strategies = targets.Select((t, i) => Move(bots[i], t)).ToArray();
                await WhenAll(strategies);
                if (strategies.All(s => s.Status == StrategyStatus.Done)) break;
            }
            return true;
        }
    }
}