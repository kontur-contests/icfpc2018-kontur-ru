using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class GetOutOfRegion : Strategy
    {
        private readonly Region region;
        private readonly Bot[] bots;

        public GetOutOfRegion(DeluxeState state, Region region, IEnumerable<Bot> bots)
            : base(state)
        {
            this.region = region;
            this.bots = bots.ToArray();
        }

        protected override async StrategyTask<bool> Run()
        {
            var strategies = bots
                .Where(b => b.Position.IsInRegion(region))
                .Select(b => new GetOutOfRegionSingleBot(state, b, region));

            return await WhenAll(strategies);
        }
    }
}