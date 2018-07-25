using System;
using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class AssembleRegion : Strategy
    {
        private Region region;
        private Bot[] bots;

        public AssembleRegion(State state, Region region, IEnumerable<Bot> bots)
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
            {
                if (state.IsVolatile(bots[0], vertices[0]))
                    return false;
                state.SetBotCommand(bots[0], new Fill(new NearDifference(vertices[0] - bots[0].Position)));
            }
            else
            {
                if (region.Any(v => state.IsVolatile(bots[0], v)))
                    return false;
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
}