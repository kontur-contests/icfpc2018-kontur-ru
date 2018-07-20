using JetBrains.Annotations;

using lib.Models;
using lib.Utils;

namespace lib.Commands
{
    public class Halt : BaseCommand
    {
        [NotNull]
        public override byte[] Encode()
        {
            return new byte[] {0b11111111};
        }

        public override bool CanApply(MutableState state, Bot bot)
        {
            if (bot.Position.MLen() != 0)
                return false;
            if (state.Bots.Count > 0)
                return false;
            if (state.Harmonics != Harmonics.Low)
                return false;
            return true;
        }

        protected override void DoApply([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            mutableState.Bots.Clear();
        }

        [NotNull]
        public override Vec[] GetVolatileCells([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            return new[] {bot.Position};
        }
    }
}