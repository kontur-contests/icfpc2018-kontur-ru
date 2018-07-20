using System.Linq;

using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Commands
{
    public class FusionS : BaseCommand
    {
        private readonly NearLinearDistance shift;

        public FusionS(NearLinearDistance shift)
        {
            this.shift = shift;
        }

        [NotNull]
        public override byte[] Encode()
        {
            return new[] {(byte)((shift.GetParameter() << 3) | 0b110)};
        }

        public override bool CanApply(MutableState state, Bot bot)
        {
            var pos = bot.Position + shift;
            if (!state.Matrix.IsInside(bot.Position))
                return false;
            if (!state.Matrix.IsInside(pos))
                return false;

            var secondaryBot = state.Bots.SingleOrDefault(x => x.Position == pos);
            return secondaryBot != null;
        }

        protected override void DoApply(MutableState mutableState, Bot bot)
        {
            // The whole work is done in FusionP
        }


        public override Vec[] GetVolatileCells(MutableState mutableState, Bot bot)
        {
            // Both volatile cells are in FusionP
            return new Vec[0];
        }
    }
}