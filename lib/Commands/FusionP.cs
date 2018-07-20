using System.Linq;

using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Commands
{
    public class FusionP : BaseCommand
    {
        private readonly NearLinearDistance shift;

        public FusionP(NearLinearDistance shift)
        {
            this.shift = shift;
        }

        [NotNull]
        public override byte[] Encode()
        {
            return new[] {(byte)((shift.GetParameter() << 3) | 0b111)};
        }

        public override void Apply(MutableState mutableState, Bot bot)
        {
            var secondaryBot = GetSecondaryBot(mutableState, bot);
            mutableState.Bots.Remove(secondaryBot);
            bot.Seeds.Add(secondaryBot.Bid);
            bot.Seeds.AddRange(secondaryBot.Seeds);
            mutableState.Energy -= 24;
        }

        [NotNull]
        public override Vec[] GetVolatileCells([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            var secondaryBot = GetSecondaryBot(mutableState, bot);
            return new[] {bot.Position, secondaryBot.Position};
        }

        [NotNull]
        private Bot GetSecondaryBot([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            var pos = bot.Position + shift;
            return mutableState.Bots.Single(x => x.Position == pos);
        }
    }
}