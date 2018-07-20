using System.Linq;

using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Commands
{
    public class LMove  : BaseCommand
    {
        private readonly ShortLinearDifference firstShift, secondShift;

        public LMove(ShortLinearDifference firstShift, ShortLinearDifference secondShift)
        {
            this.firstShift = firstShift;
            this.secondShift = secondShift;
        }

        public override string ToString()
        {
            return $"LMove({firstShift}, {secondShift})";
        }

        [NotNull]
        public override byte[] Encode()
        {
            var (a1, i1) = firstShift.GetParameters();
            var (a2, i2) = secondShift.GetParameters();

            byte firstByte = (byte)((a2 << 6) | (a1 << 4) | 0b1100);
            byte secondByte = (byte)((i2 << 4) | i1);
            return new[] {firstByte, secondByte};
        }

        public override bool CanApply(MutableState state, Bot bot)
        {
            if (!state.Matrix.IsInside(bot.Position + firstShift))
                return false;
            if (!state.Matrix.IsInside(bot.Position + firstShift + secondShift))
                return false;
            return GetCellsOnPath(bot).All(x => state.Matrix.IsVoidVoxel(x));
        }

        protected override void DoApply([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            bot.Position = bot.Position + firstShift + secondShift;
            mutableState.Energy += 2 * (firstShift.Shift.MLen() + 2 + secondShift.Shift.MLen());
        }

        public override Vec[] GetVolatileCells(MutableState mutableState, Bot bot)
        {
            return GetCellsOnPath(bot);
        }

        [NotNull]
        private Vec[] GetCellsOnPath([NotNull] Bot bot)
        {
            return firstShift.GetTrace(bot.Position).Concat(secondShift.GetTrace(bot.Position + firstShift).Skip(1)).ToArray();
        }
    }
}