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
    }
}