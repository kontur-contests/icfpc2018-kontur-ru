using System.Linq;

using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Commands
{
    public class SMove : BaseCommand
    {
        public LongLinearDifference Shift { get; }

        public SMove(LongLinearDifference shift)
        {
            this.Shift = shift;
        }

        public override string ToString()
        {
            return $"SMove({Shift})";
        }

        [NotNull]
        public override byte[] Encode()
        {
            var (a, i) = Shift.GetParameters();
            byte firstByte = (byte)((a << 4) | 0b0100);
            byte secondByte = (byte)i;
            return new[] {firstByte, secondByte};
        }

        public override bool AllPositionsAreValid([NotNull] IMatrix matrix, Bot bot)
        {
            if (!matrix.IsInside(bot.Position + Shift))
                return false;
            var obstacle = GetCellsOnPath(bot.Position).FirstOrDefault(v => !matrix.IsVoidVoxel(v));
            return obstacle == null;
        }

        public override void Apply(State state, Bot bot)
        {
            bot.Position = bot.Position + Shift;
            state.Energy += 2 * Shift.Shift.MLen();
        }

        [NotNull]
        public override Vec[] GetVolatileCells([NotNull] Bot bot)
        {
            return GetCellsOnPath(bot.Position);
        }

        [NotNull]
        public Vec[] GetCellsOnPath([NotNull] Vec position)
        {
            return Shift.GetTrace(position);
        }
    }
}