using JetBrains.Annotations;

using lib.Models;

namespace lib.Commands
{
    public class Wait : BaseCommand
    {
        [NotNull]
        public override byte[] Encode()
        {
            return new byte[] {0b11111110};
        }

        public override void Apply([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            
        }
    }
}