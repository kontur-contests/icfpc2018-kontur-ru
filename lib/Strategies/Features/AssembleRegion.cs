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
        public AssembleRegion(State state, Region region, IEnumerable<Bot> bots)
            : base(state)
        {
            Region = region;
            Bots = bots.ToArray();
        }

        public Region Region { get; }
        public Bot[] Bots { get; }

        protected override async StrategyTask<bool> Run()
        {
            var vertices = Region.Vertices().ToList();
            var indices = GetBestPermutation();
            var strategies = new List<IStrategy>();
            for (var i = 0; i < indices.Length; i++)
            {
                var bot = Bots[indices[i]];
                var vertex = vertices[i];
                strategies.Add(new GotoVertex(state, bot, Region, vertex));
            }
            if (!await WhenAll(strategies))
                return false;

            if (Bots.Length == 1)
            {
                if (state.IsVolatile(bots[0], vertices[0]))
                    return false;
                return await Do(bots[0], new Fill(new NearDifference(vertices[0] - bots[0].Position)));
            }
            if (region.Any(v => state.IsVolatile(bots[0], v)))
                return false;
            return await WhenAll(indices.Select((index, i) =>
                {
                    var bot = Bots[index];
                    var vertex = vertices[i];
                    return Do(bot, new GFill(new NearDifference(vertex - bot.Position), new FarDifference(Region.Opposite(vertex) - vertex)));
                }));
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