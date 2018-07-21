using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Commands
{
    public class Voidd : BaseCommand
    {
        public NearDifference Shift { get; }

        public Voidd(NearDifference shift)
        {
            this.Shift = shift;
        }

        public override string ToString()
        {
            return $"Void({Shift})";
        }

        [NotNull]
        public override byte[] Encode()
        {
            return new [] {(byte)((Shift.GetParameter() << 3) | 0b010)};
        }

        public override bool CanApply(MutableState state, Bot bot)
        {
            return state.BuildingMatrix.IsInside(GetPosition(bot));
        }

        protected override void DoApply(MutableState mutableState, Bot bot)
        {
            var pos = GetPosition(bot);
            if (mutableState.BuildingMatrix.IsFilledVoxel(pos))
            {
                mutableState.Energy -= 12;
                mutableState.BuildingMatrix.Void(pos);
            }
            else
            {
                mutableState.Energy += 3;
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
            return bot.Position + Shift;
        }
    }
}