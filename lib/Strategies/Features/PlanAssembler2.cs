using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class PlanAssembler2 : Strategy
    {
        private readonly IGeneralPlan plan;

        public PlanAssembler2(State state, IGeneralPlan plan)
            : base(state)
        {
            this.plan = plan;
        }

        protected override async StrategyTask<bool> Run()
        {
            var split = new Split(state, state.Bots.Single(), 8);
            await split;

            var freeBots = new HashSet<Bot>(split.Bots);
            var buildingRegions = new HashSet<Region>();

            while (!plan.IsComplete)
            {
                var strategies = new List<IStrategy>();
                while (true)
                {
                    var nextRegion = plan.GetNextRegion(r =>
                        r.Vertices().Count() <= freeBots.Count
                        && !buildingRegions.Contains(r)
                        && !buildingRegions.Any(br => RegionsAreTooNear(r, br)));
                    if (nextRegion == null)
                        break;
                    var brigade = freeBots.Take(nextRegion.Vertices().Count()).ToList();
                    freeBots.ExceptWith(brigade);
                    strategies.Add(new AssembleRegion(state, nextRegion, brigade));
                    buildingRegions.Add(nextRegion);
                }

                foreach (var freeBot in freeBots)
                {
                    var regionToLeave = buildingRegions.SingleOrDefault(r => freeBot.Position.IsInRegion(r));
                    if (regionToLeave != null)
                        strategies.Add(new GetOutOfRegionSingleBot(state, freeBot, regionToLeave));
                }

                await WhenAny(strategies);

                var finishedStrategies = strategies.Where(s => s.Status != StrategyStatus.InProgress).ToList();

                foreach (var strategy in finishedStrategies.OfType<GetOutOfRegionSingleBot>())
                {
                    freeBots.Add(strategy.Bot);
                    strategies.Remove(strategy);
                }

                foreach (var strategy in finishedStrategies.OfType<AssembleRegion>())
                {
                    freeBots.UnionWith(strategy.Bots);
                    strategies.Remove(strategy);
                    buildingRegions.Remove(strategy.Region);
                    if (strategy.Status == StrategyStatus.Done)
                       plan.GroundRegion(strategy.Region);
                }
            }

            return await new ReachFinalState(state);
        }

        private static bool RegionsAreTooNear(Region a, Region b)
        {
            return a.Expand().Intersects(b.Expand());
        }
    }
}