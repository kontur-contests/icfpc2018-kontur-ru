using System.Linq;

using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;

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
            var pos = bot.Position + shift;
            var secondaryBot = mutableState.Bots.Single(x => x.Position == pos);
            mutableState.Bots.Remove(secondaryBot);
            bot.Seeds.Add(secondaryBot.Bid);
            bot.Seeds.AddRange(secondaryBot.Seeds);
            mutableState.Energy -= 24;
        }
    }
}