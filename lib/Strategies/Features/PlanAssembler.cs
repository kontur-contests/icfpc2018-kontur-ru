using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class PlanAssembler : Strategy
    {
        private readonly Region[] plan;
        private readonly Bot[] bots;

        public PlanAssembler(State state, IEnumerable<Bot> bots, IEnumerable<Region> plan)
            : base(state)
        {
            this.plan = plan.ToArray();
            this.bots = bots.ToArray();
        }

        protected override async StrategyTask<bool> Run()
        {
            foreach (var region in plan)
            {
                if (!await new GetOutOfRegion(state, bots.Skip(region.Vertices().Count()), region))
                    return false;
                if (!await new AssembleRegion(state, region, bots.Take(region.Vertices().Count())))
                    return false;
            }
            return true;
        }
    }
}