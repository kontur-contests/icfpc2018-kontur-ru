using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class Split : SimpleSingleBotStrategyBase
    {
        public Split(DeluxeState state, Bot bot)
            : base(state, bot)
        {
        }

        protected override async StrategyTask<bool> Run()
        {
            if (bot.Seeds.Count == 1)
            {
                return false;
            }

            var to = NearPosition();

            if (to != null)
            {
                await Do(new Fission(new NearDifference(to - bot.Position), bot.Seeds[(bot.Seeds.Count + 1) / 2]));
                return true;
            }

            return false;
        }

        private Vec NearPosition()
        {
            return bot.Position
                      .GetNears()
                      .Where(n => n.IsInCuboid(state.Matrix.R))
                      .FirstOrDefault(n => !state.VolatileCells.ContainsKey(n));
        }
    }
}