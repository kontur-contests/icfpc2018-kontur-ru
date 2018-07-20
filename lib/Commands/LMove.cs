using JetBrains.Annotations;

using lib.Primitives;

namespace lib.Commands
{
    public class LMove  : BaseCommand
    {
        private readonly ShortLinearDistance firstShift, secondShift;

        public LMove(ShortLinearDistance firstShift, ShortLinearDistance secondShift)
        {
            this.firstShift = firstShift;
            this.secondShift = secondShift;
        }

        [NotNull]
        public override byte[] Encode()
        {
            var (a1, i1) = firstShift.GetParameters();
            var (a2, i2) = secondShift.GetParameters();

            byte firstByte = (byte)((a2 << 6) | (a1 << 4) | 0b1100);
            byte secondByte = (byte)((i2 << 4) | i1);
            return new[] {firstByte, secondByte};
        }
    }
}