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

        public override string ToString()
        {
            return "Halt()";
        }

        public override bool AllPositionsAreValid([NotNull] IMatrix matrix, Bot bot)
        {
            if (bot.Position.MLen() != 0)
                return false;
            return true;
        }

        public override void Apply(State state, Bot bot)
        {
            state.Bots.Clear();
        }

        [NotNull]
        public override Vec[] GetVolatileCells([NotNull] Bot bot)
        {
            return new[] {bot.Position};
        }
    }
}