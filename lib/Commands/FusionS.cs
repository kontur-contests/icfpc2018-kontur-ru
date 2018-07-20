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

        public override void Apply(MutableState mutableState, Bot bot)
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