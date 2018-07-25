using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class DeriveOne : BotStrategy
    {
        public DeriveOne(State state, Bot bot)
            : base(state, bot)
        {
        }

        public Bot DerivedBot { get; private set; }

        protected override async StrategyTask<bool> Run()
        {
            if (!bot.Seeds.Any())
                return false;

            var to = NearPosition();
            if (to == null)
                return false;

            var newBid = bot.Seeds[0];
            await Do(new Fission(new NearDifference(to - bot.Position), bot.Seeds.Count - 1));

            DerivedBot = state.Bots.Single(b => b.Bid == newBid);
            return true;
        }

        private Vec NearPosition()
        {
            return bot.Position
                      .GetNears()
                      .Where(n => n.IsInCuboid(state.Matrix.R))
                      .FirstOrDefault(n => !state.IsVolatile(bot, n));
        }
    }
}