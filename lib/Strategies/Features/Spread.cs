using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class Spread : Strategy
    {
        private readonly Bot[] bots;

        public Spread(DeluxeState state, IEnumerable<Bot> bots)
            : base(state)
        {
            this.bots = bots.ToArray();
        }

        protected override async StrategyTask<bool> Run()
        {
            var targets = new List<Vec>();
            var origins = new[]
                {
                    new Vec(0, 0, 0),
                    new Vec(R - 1, 0, 0),
                    new Vec(R - 1, 0, R - 1),
                    new Vec(0, 0, R - 1),
                };
            var shifts = new[]
                {
                    new Vec(1, 0, 0),
                    new Vec(0, 0, 1),
                    new Vec(-1, 0, 0),
                    new Vec(0, 0, -1),
                };
            for (int i = 0; i < bots.Length; i++)
            {
                var g = i % 4;
                var p = i / 4;
                targets.Add(origins[g] + shifts[g] * (R / (bots.Length / 4)) * p);
            }
            return await WhenAll(targets.Select((t, i) => Move(bots[i], t)).ToArray());
        }
    }
}