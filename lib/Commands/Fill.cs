using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Commands
{
    public class Fill : BaseCommand
    {
        private readonly NearDifference shift;

        public Fill(NearDifference shift)
        {
            this.shift = shift;
        }

        public override string ToString()
        {
            return $"Fill({shift})";
        }

        [NotNull]
        public override byte[] Encode()
        {
            return new [] {(byte)((shift.GetParameter() << 3) | 0b011)};
        }

        public override bool CanApply(MutableState state, Bot bot)
        {
            return state.Matrix.IsInside(GetPosition(bot));
        }

        protected override void DoApply(MutableState mutableState, Bot bot)
        {
            var pos = GetPosition(bot);
            if (mutableState.Matrix.IsVoidVoxel(pos))
            {
                mutableState.Energy += 12;
                mutableState.Matrix.Fill(pos);
            }
            else
            {
                mutableState.Energy += 6;
            }
        }

        [NotNull]
        public override Vec[] GetVolatileCells([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            return new[] {bot.Position, GetPosition(bot)};
        }

        [NotNull]
        private Vec GetPosition([NotNull] Bot bot)
        {
            return bot.Position + shift;
        }
    }
}