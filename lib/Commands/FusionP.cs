using System.Linq;

using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Commands
{
    public class FusionP : BaseCommand
    {
        public readonly NearDifference shift;

        public FusionP(NearDifference shift)
        {
            this.shift = shift;
        }

        public override string ToString()
        {
            return $"FusionP({shift})";
        }

        [NotNull]
        public override byte[] Encode()
        {
            return new[] {(byte)((shift.GetParameter() << 3) | 0b111)};
        }

        public override bool AllPositionsAreValid([NotNull] IMatrix matrix, Bot bot)
        {
            var pos = bot.Position + shift;
            if (!matrix.IsInside(bot.Position))
                return false;
            if (!matrix.IsInside(pos))
                return false;
            return true;
        }

        public override void Apply(DeluxeState state, Bot bot)
        {
            var secondaryBot = GetSecondaryBot(state, bot);
            state.Bots.Remove(secondaryBot);
            bot.Seeds.Add(secondaryBot.Bid);
            bot.Seeds.AddRange(secondaryBot.Seeds);
            state.Energy -= 24;
        }

        [NotNull]
        public override Vec[] GetVolatileCells([NotNull] Bot bot)
        {
            return new[] {bot.Position };
        }

        [NotNull]
        private Bot GetSecondaryBot([NotNull] DeluxeState state, [NotNull] Bot bot)
        {
            var pos = bot.Position + shift;
            return state.Bots.Single(x => x.Position == pos);
        }
    }
}