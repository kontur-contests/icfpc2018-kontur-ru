using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class GetOutOfRegion : Strategy
    {
        private readonly Region[] regions;
        private readonly Bot[] bots;

        public GetOutOfRegion(State state, IEnumerable<Bot> bots, params Region[] regions)
            : base(state)
        {
            this.regions = regions;
            this.bots = bots.ToArray();
        }

        protected override async StrategyTask<bool> Run()
        {
            var strategies = bots
                .Where(b => regions.Any(rr => b.Position.IsInRegion(rr)))
                .Select(b => new GetOutOfRegionSingleBot(state, b, regions));

            return await WhenAll(strategies);
        }
    }
}