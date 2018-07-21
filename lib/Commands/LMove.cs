using System.Linq;

using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Commands
{
    public class LMove  : BaseCommand
    {
        public readonly ShortLinearDifference firstShift, secondShift;

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

        public override bool AllPositionsAreValid([NotNull] IMatrix matrix, Bot bot)
        {
            if (!matrix.IsInside(bot.Position + firstShift))
                return false;
            if (!matrix.IsInside(bot.Position + firstShift + secondShift))
                return false;
            return GetCellsOnPath(bot.Position).All(matrix.IsVoidVoxel);
        }

        protected override void DoApply([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            bot.Position = bot.Position + firstShift + secondShift;
            mutableState.Energy += 2 * (firstShift.Shift.MLen() + 2 + secondShift.Shift.MLen());
        }

        public override void Apply(DeluxeState state, Bot bot)
        {
            bot.Position = bot.Position + firstShift + secondShift;
            state.Energy += 2 * (firstShift.Shift.MLen() + 2 + secondShift.Shift.MLen());
        }

        public override Vec[] GetVolatileCells(Bot bot)
        {
            return GetCellsOnPath(bot.Position);
        }

        [NotNull]
        public Vec[] GetCellsOnPath([NotNull] Vec position)
        {
            return firstShift.GetTrace(position).Concat(secondShift.GetTrace(position + firstShift).Skip(1)).ToArray();
        }
    }
}