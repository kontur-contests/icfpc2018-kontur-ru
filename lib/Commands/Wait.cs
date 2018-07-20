using JetBrains.Annotations;

namespace lib.Commands
{
    public class Wait : BaseCommand
    {
        [NotNull]
        public override byte[] Encode()
        {
            return new byte[] {0b11111110};
        }
    }
}