using System;
using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class GreedyFill : BotStrategy
    {
        private readonly IOracle oracle;
        private readonly ICandidatesOrdering candidatesOrdering;

        public GreedyFill(State state, Bot bot, IOracle oracle, ICandidatesOrdering candidatesOrdering = null)
            : base(state, bot)
        {
            this.oracle = oracle;
            this.candidatesOrdering = candidatesOrdering ?? new BuildAllStayingStill();
        }

        protected override async StrategyTask<bool> Run()
        {
            var candidates = BuildCandidates();
            while (candidates.Any())
            {
                var candidatesAndPositions = OrderCandidates(candidates);
                var any = false;
                foreach (var (candidate, nearPosition) in candidatesAndPositions)
                {
                    if (!await new FillVoxel(state, Bot, candidate, nearPosition))
                        continue;

                    oracle.Fill(candidate);
                    any = true;

                    candidates.Remove(candidate);
                    foreach (var neighbor in candidate.GetMNeighbours(state.Matrix))
                    {
                        if (state.TargetMatrix[neighbor] && !state.Matrix[neighbor])
                            candidates.Add(neighbor);
                    }
                    break;
                }
                if (!any)
                    throw new Exception("Can't move");
            }
            await Move(Vec.Zero);
            await Halt();
            return true;
        }

        private IEnumerable<(Vec candidate, Vec nearPosition)> OrderCandidates(HashSet<Vec> candidates)
        {
            foreach (var candidate in candidatesOrdering.Order(candidates, Bot.Position))
            {
                var nearPositions = candidate.GetNears().Where(n => n.IsInCuboid(state.Matrix.R));
                foreach (var nearPosition in nearPositions.OrderBy(p => p.MDistTo(Bot.Position)))
                {
                    if (oracle.CanFill(candidate, nearPosition))
                    {
                        yield return (candidate, nearPosition);
                    }
                }
            }
        }

        private HashSet<Vec> BuildCandidates()
        {
            var result = new HashSet<Vec>();
            for (int x = 0; x < state.Matrix.R; x++)
                for (int y = 0; y < state.Matrix.R; y++)
                    for (int z = 0; z < state.Matrix.R; z++)
                    {
                        var vec = new Vec(x, y, z);
                        if (state.TargetMatrix[vec]
                            && !state.Matrix[vec]
                            && (y == 0 || vec.GetMNeighbours(state.Matrix).Any(nvec => state.Matrix[nvec])))
                        {
                            result.Add(vec);
                        }
                    }
            return result;
        }

    }
}