using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;

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
    }
}