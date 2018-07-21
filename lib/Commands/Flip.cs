using JetBrains.Annotations;

using lib.Models;
using lib.Utils;

namespace lib.Commands
{
    public class Flip : BaseCommand
    {
        [NotNull]
        public override byte[] Encode()
        {
            return new byte[] {0b11111101};
        }
        public override string ToString()
        {
            return "Flip()";
        }

        public override bool AllPositionsAreValid([NotNull] IMatrix matrix, Bot bot)
        {
            return true;
        }

        public override void Apply(DeluxeState state, Bot bot)
        {
            state.Harmonics = state.Harmonics == Harmonics.High ? Harmonics.Low : Harmonics.High;
        }

        [NotNull]
        public override Vec[] GetVolatileCells([NotNull] Bot bot)
        {
            return new[] {bot.Position};
        }
    }
}