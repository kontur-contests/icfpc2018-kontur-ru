using System;
using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class CooperativeGreedyFill : SimpleSingleBotStrategyBase
    {
        private readonly ThrowableHelper oracle;
        private readonly ICandidatesOrdering candidatesOrdering;
        private readonly HashSet<Vec> candidates;

        public CooperativeGreedyFill(DeluxeState state, Bot bot, ThrowableHelper oracle, HashSet<Vec> candidates, ICandidatesOrdering candidatesOrdering = null)
            : base(state, bot)
        {
            this.oracle = oracle;
            this.candidates = candidates;
            this.candidatesOrdering = candidatesOrdering ?? new BuildAllStayingStill();
        }

        protected override async StrategyTask<bool> Run()
        {
            while (candidates.Any())
            {
                var candidatesAndPositions = OrderCandidates();
                var any = false;
                foreach (var (candidate, nearPosition) in candidatesAndPositions)
                {
                    if (!await new FillVoxel(state, bot, candidate, nearPosition, () => oracle.CanFill(candidate, GetBots(nearPosition))))
                        continue;

                    oracle.Fill(candidate);

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
                    if (!await new MoveSingleBot(state, bot, new Vec(0, bot.Bid, 0)))
                        await WhenNextTurn();
                }
            }

            return true;
        }

        private IEnumerable<(Vec candidate, Vec nearPosition)> OrderCandidates()
        {
            foreach (var candidate in candidatesOrdering.Order(candidates, bot.Position).Where(c => !state.VolatileCells.ContainsKey(c)))
            {
                var nearPositions = candidate.GetNears().Where(n => n.IsInCuboid(state.Matrix.R) && 
                                                                    (!state.VolatileCells.ContainsKey(n) || n == bot.Position));
                foreach (var nearPosition in nearPositions.OrderBy(p => p.MDistTo(bot.Position)))
                {
                    if (oracle.CanFill(candidate, GetBots(nearPosition)))
                        yield return (candidate, nearPosition);
                }
            }
        }

        private List<Vec> GetBots(Vec moveTo)
        {
            var others = state.Bots.Where(b => b != bot).Select(b => b.Position).ToList();
            others.Add(moveTo);
            return others;
        }
    }
}