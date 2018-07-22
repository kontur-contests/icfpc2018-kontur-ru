using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Strategies.Features.Async;

namespace lib.Strategies.Features
{
    public class Cloning : SimpleSingleBotStrategyBase
    {
        public List<Bot> Bots = new List<Bot>();

        public Cloning(DeluxeState state, Bot bot)
            : base(state, bot)
        {
        }

        protected override async StrategyTask<bool> Run()
        {
            if (bot.Seeds.Count == 1)
            {
                Bots.Add(bot);
                return true;
            }

            var nearPositions = bot.Position.GetNears()
                                   .Where(n => n.IsInCuboid(state.Matrix.R))
                                   .Where(n => !state.VolatileCells.ContainsKey(n))
                                   .ToList();

            if (nearPositions.Any())
            {
                var to = nearPositions.First();
                await Do(new Fission(new NearDifference(to - bot.Position), bot.Seeds[(bot.Seeds.Count + 1) / 2]));
                return true;
            }

            return false;
        }
    }
}