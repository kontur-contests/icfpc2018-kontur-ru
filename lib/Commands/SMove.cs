using System.Linq;

using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;
using lib.Utils;

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

        public override bool CanApply(MutableState state, Bot bot)
        {
            if (!state.Matrix.IsInside(bot.Position + shift))
                return false;
            return GetCellsOnPath(bot).All(x => state.Matrix.IsVoidVoxel(x));
        }

        protected override void DoApply(MutableState mutableState, Bot bot)
        {
            bot.Position = bot.Position + shift;
            mutableState.Energy += 2 * shift.Shift.MLen();
        }

        [NotNull]
        public override Vec[] GetVolatileCells([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            return GetCellsOnPath(bot);
        }

        [NotNull]
        private Vec[] GetCellsOnPath([NotNull] Bot bot)
        {
            return shift.GetTrace(bot.Position);
        }
    }
}