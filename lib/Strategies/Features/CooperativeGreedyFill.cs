using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

using MoreLinq;

namespace lib.Strategies.Features
{
    public class CooperativeGreedyFill : BotStrategy
    {
        private readonly ICandidatesOrdering candidatesOrdering;
        private readonly HashSet<Vec> candidates;

        public CooperativeGreedyFill(State state, Bot bot, HashSet<Vec> candidates, ICandidatesOrdering candidatesOrdering = null)
            : base(state, bot)
        {
            this.candidates = candidates;
            this.candidatesOrdering = candidatesOrdering ?? new BottomToTopBuildingAround();
        }

        protected override async StrategyTask<bool> Run()
        {
            while (candidates.Any())
            {
                var candidatesAndPositions = OrderCandidates();
                var any = false;
                foreach (var (candidate, nearPosition) in candidatesAndPositions)
                {
                    if (!await new FillVoxel(state, Bot, candidate, nearPosition))
                        continue;
                    
                    candidates.Remove(candidate);
                    foreach (var neighbor in candidate.GetMNeighbours(state.Matrix))
                    {
                        if (state.TargetMatrix[neighbor] && !state.Matrix[neighbor])
                            candidates.Add(neighbor);
                    }

                    any = true;
                    break;
                }

                if (!any)
                {
                    if (!await new Move(state, Bot, new Vec(Bot.Bid / state.R, Bot.Bid % state.R, 0)))
                        await WhenNextTurn();
                }
            }

            return true;
        }

        private IEnumerable<(Vec candidate, Vec nearPosition)> OrderCandidates()
        {
            var lowest = candidates.Min(c => c.Y);
            var filtered = candidates.Where(c => c.Y == lowest).ToHashSet();

            foreach (var candidate in candidatesOrdering.Order(filtered, Bot.Position).Where(c => !state.IsVolatile(Bot, c)))
            {
                var nearPositions = candidate.GetNears().Where(n => n.IsInCuboid(state.Matrix.R) && 
                                                                    (!state.IsVolatile(Bot, n) || n == Bot.Position) &&
                                                                    n.Y >= lowest);
                foreach (var nearPosition in nearPositions.OrderBy(p => p.MDistTo(Bot.Position)))
                {
                    yield return (candidate, nearPosition);
                }
            }
        }
    }
}