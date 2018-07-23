using System;
using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Strategies.Features.Async;
using lib.Utils;

using MoreLinq;

namespace lib.Strategies.Features
{
    public class AssembleRegion : Strategy
    {
        private Region region;
        private Bot[] bots;

        public AssembleRegion(DeluxeState state, Region region, IEnumerable<Bot> bots)
            : base(state)
        {
            this.region = region;
            this.bots = bots.ToArray();
        }

        protected override async StrategyTask<bool> Run()
        {
            var vertices = region.Vertices().ToList();
            var indices = GetBestPermutation();
            var strategies = new List<IStrategy>();
            for (var i = 0; i < indices.Length; i++)
            {
                var bot = bots[indices[i]];
                var vertex = vertices[i];
                strategies.Add(new GotoVertex(state, bot, region, vertex));
            }
            if (!await WhenAll(strategies))
                return false;

            if (bots.Length == 1)
                state.SetBotCommand(bots[0], new Fill(new NearDifference(vertices[0] - bots[0].Position)));
            else
            {
                for (var i = 0; i < indices.Length; i++)
                {
                    var bot = bots[indices[i]];
                    var vertex = vertices[i];
                    state.SetBotCommand(bot, new GFill(new NearDifference(vertex - bot.Position), new FarDifference(region.Opposite(vertex) - vertex)));
                }
            }
            await WhenNextTurn();
            return true;
        }

        private int[] GetBestPermutation()
        {
            return GetAllPermutations().OrderBy(Score).First();
        }

        private int Score(int[] indices)
        {
            var maxDist = int.MinValue;
            var vertices = region.Vertices().ToList();
            for (var i = 0; i < indices.Length; i++)
            {
                var bot = bots[indices[i]];
                var vertex = vertices[i];
                maxDist = Math.Max(maxDist, bot.Position.MDistTo(vertex));
            }
            return maxDist;
        }

        private IEnumerable<int[]> GetAllPermutations()
        {
            var indices = Enumerable.Range(0, bots.Length).ToArray();
            do
            {
                yield return indices.ToArray();
            } while (Permutator.NextPermutation(indices));
        }
    }

    public class PlanAssembler : Strategy
    {
        private readonly Region[] plan;
        private readonly Bot[] bots;

        public PlanAssembler(DeluxeState state, IEnumerable<Bot> bots, IEnumerable<Region> plan)
            : base(state)
        {
            this.plan = plan.ToArray();
            this.bots = bots.ToArray();
        }

        protected override async StrategyTask<bool> Run()
        {
            foreach (var region in plan)
            {
                if (!await new GetOutOfRegion(state, region, bots.Skip(region.Vertices().Count())))
                    return false;
                if (!await new AssembleRegion(state, region, bots.Take(region.Vertices().Count())))
                    return false;
            }
            return true;
        }
    }
}