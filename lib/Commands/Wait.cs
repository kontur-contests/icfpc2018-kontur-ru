using JetBrains.Annotations;

using lib.Models;
using lib.Utils;

namespace lib.Commands
{
    public class Wait : BaseCommand
    {
        [NotNull]
        public override byte[] Encode()
        {
            return new byte[] {0b11111110};
        }

        public override bool CanApply(MutableState state, Bot bot)
        {
            return true;
        }

        protected override void DoApply([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            // Just wait
        }

        [NotNull]
        public override Vec[] GetVolatileCells([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            return new[] {bot.Position};
        }
    }
}