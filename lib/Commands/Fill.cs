using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Commands
{
    public class Fill : BaseCommand
    {
        public NearDifference Shift { get; }

        public Fill(NearDifference shift)
        {
            this.Shift = shift;
        }

        public override string ToString()
        {
            return $"Fill({Shift})";
        }

        [NotNull]
        public override byte[] Encode()
        {
            return new [] {(byte)((Shift.GetParameter() << 3) | 0b011)};
        }

        public override bool AllPositionsAreValid([NotNull] IMatrix matrix, Bot bot)
        {
            return GetPosition(bot).IsInCuboid(matrix.R);
        }

        protected override void DoApply(MutableState mutableState, Bot bot)
        {
            var pos = GetPosition(bot);
            if (mutableState.BuildingMatrix.IsVoidVoxel(pos))
            {
                mutableState.Energy += 12;
                mutableState.BuildingMatrix.Fill(pos);
            }
            else
            {
                mutableState.Energy += 6;
            }
        }

        public override void Apply(DeluxeState state, Bot bot)
        {
            var pos = GetPosition(bot);
            if (state.Matrix.IsVoidVoxel(pos))
            {
                state.Energy += 12;
                state.Matrix.Fill(pos);
            }
            else
            {
                state.Energy += 6;
            }
        }

        [NotNull]
        public override Vec[] GetVolatileCells([NotNull] Bot bot)
        {
            return new[] {bot.Position, GetPosition(bot)};
        }

        [NotNull]
        private Vec GetPosition([NotNull] Bot bot)
        {
            return bot.Position + Shift;
        }
    }
}