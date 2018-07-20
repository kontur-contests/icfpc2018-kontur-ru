using JetBrains.Annotations;

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
    }
}