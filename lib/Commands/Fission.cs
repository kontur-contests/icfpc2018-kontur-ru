using JetBrains.Annotations;

using lib.Primitives;

namespace lib.Commands
{
    public class Fission : BaseCommand
    {
        private readonly NearLinearDistance shift;
        private readonly int m;

        public Fission(NearLinearDistance shift, int m)
        {
            this.shift = shift;
            this.m = m;
        }

        [NotNull]
        public override byte[] Encode()
        {
            return new [] {(byte)((shift.GetParameter() << 3) | 0b101), (byte)m};
        }
    }
}