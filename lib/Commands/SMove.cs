using JetBrains.Annotations;

using lib.Primitives;

namespace lib.Commands
{
    public class SMove : BaseCommand
    {
        private readonly LongLinearDistance shift;

        public SMove(LongLinearDistance shift)
        {
            this.shift = shift;
        }

        [NotNull]
        public override byte[] Encode()
        {
            var (a, i) = shift.GetParameters();
            byte firstByte = (byte)((a << 4) | 0b0100);
            byte secondByte = (byte)i;
            return new[] {firstByte, secondByte};
        }
    }
}