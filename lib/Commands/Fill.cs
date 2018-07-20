using JetBrains.Annotations;

using lib.Primitives;

namespace lib.Commands
{
    public class Fill : BaseCommand
    {
        private readonly NearLinearDistance shift;
        private readonly int m;

        public Fill(NearLinearDistance shift, int m)
        {
            this.shift = shift;
            this.m = m;
        }

        [NotNull]
        public override byte[] Encode()
        {
            return new [] {(byte)((shift.GetParameter() << 3) | 0b011)};
        }
    }
}