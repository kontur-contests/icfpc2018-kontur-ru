using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class PlanAssembler2 : Strategy
    {
        private readonly IGeneralPlan plan;
        private readonly int maxFreeBots;

        public PlanAssembler2(State state, IGeneralPlan plan, int maxFreeBots)
            : base(state)
        {
            this.plan = plan;
            this.maxFreeBots = maxFreeBots;
        }

        protected override async StrategyTask<bool> Run()
        {
            await Do(state.Bots.Single(), new Flip());
            
            var split = new Split(state, state.Bots.Single(), maxFreeBots);
            await split;

            var freeBots = new HashSet<Bot>(split.Bots);
            var buildingRegions = new HashSet<Region>();

            var strategies = new List<IStrategy>();
            while (!plan.IsComplete)
            {
                while (true)
                {
                    var nextRegion = plan.GetNextRegion(r =>
                        !buildingRegions.Contains(r)
                        && !buildingRegions.Any(br => RegionsAreTooNear(r, br))
                        && state.Matrix.CanFillRegion(r));
                    if (nextRegion == null || freeBots.Count < nextRegion.Vertices().Count())
                    {
                        if (nextRegion != null)
                            plan.ReturnRegion(nextRegion);
                        break;
                    }
                    var brigade = freeBots
                                  .OrderBy(b => nextRegion.Vertices().Min(v => v.MDistTo(b.Position)))
                                  .Take(nextRegion.Vertices().Count())
                                  .ToList();
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
            await WhenAll(strategies);
            return await new ReachFinalState(state);
        }

        private static bool RegionsAreTooNear(Region a, Region b)
        {
            return a.Expand().Intersects(b.Expand());
        }
    }
}