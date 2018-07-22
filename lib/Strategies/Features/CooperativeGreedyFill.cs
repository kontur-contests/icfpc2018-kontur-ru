using System;
using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class CooperativeGreedyFill : SimpleSingleBotStrategyBase
    {
        private readonly ThrowableHelperFast oracle;
        private readonly ICandidatesOrdering candidatesOrdering;
        private readonly HashSet<Vec> candidates;

        public CooperativeGreedyFill(DeluxeState state, Bot bot, ThrowableHelperFast oracle, HashSet<Vec> candidates, ICandidatesOrdering candidatesOrdering = null)
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
                    if (!await new FillVoxel(state, bot, candidate, nearPosition, () => AfterFill(candidate)))
                        continue;

                    AfterFill(candidate);
                    any = true;
                    break;
                }

                if (!any)
                    await new MoveSingleBot(state, bot, new Vec(0, bot.Bid, 0));
            }

            return true;
        }

        private void AfterFill(Vec candidate)
        {
            oracle.Fill(candidate);
            
            candidates.Remove(candidate);
            foreach (var neighbor in candidate.GetMNeighbours(state.Matrix))
            {
                if (state.TargetMatrix[neighbor] && !state.Matrix[neighbor])
                    candidates.Add(neighbor);
            }
        }

        private IEnumerable<(Vec candidate, Vec nearPosition)> OrderCandidates()
        {
            foreach (var candidate in candidatesOrdering.Order(candidates, bot.Position))
            {
                var nearPositions = candidate.GetNears().Where(n => n.IsInCuboid(state.Matrix.R));
                foreach (var nearPosition in nearPositions.OrderBy(p => p.MDistTo(bot.Position)))
                {
                    if (oracle.CanFill(candidate, nearPosition, state))
                    {
                        yield return (candidate, nearPosition);
                    }
                }
            }
        }
    }
}