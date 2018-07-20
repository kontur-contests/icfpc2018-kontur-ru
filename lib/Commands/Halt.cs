using System.Linq;

using JetBrains.Annotations;

using lib.Models;

namespace lib.Commands
{
    public class Halt : BaseCommand
    {
        [NotNull]
        public override byte[] Encode()
        {
            return new byte[] {0b11111111};
        }

        public override void Apply([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            mutableState.Bots.Clear();
        }
    }
}