using JetBrains.Annotations;

namespace lib.Commands
{
    public class Flip : BaseCommand
    {
        [NotNull]
        public override byte[] Encode()
        {
            return new byte[] {0b11111101};
        }
    }
}