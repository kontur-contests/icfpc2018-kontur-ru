using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Commands
{
    public class GFill : GroupCommand
    {
        public GFill(NearDifference nearShift, FarDifference farShift)
            : base(nearShift, farShift)
        {
        }

        public override string ToString()
        {
            return $"GFill({NearShift}, {FarShift})";
        }

        [NotNull]
        public override byte[] Encode()
        {
            return new []
                {
                    (byte)((NearShift.GetParameter() << 3) | 0b001),
                    (byte) (FarShift.GetParameterX() + 30),
                    (byte) (FarShift.GetParameterY() + 30),
                    (byte) (FarShift.GetParameterZ() + 30),
                };
        }

        public override bool AllPositionsAreValid([NotNull] IMatrix matrix, Bot bot)
        {
            var region = GetRegion(bot.Position);
            return matrix.IsInside(region.Start) &&
                   matrix.IsInside(region.End);
            
            // Not checking these conditions:
            // * It is also an error if boti.pos + ndi = botj.pos + ndj (for i ≠ j).
            // * It is also an error if any coordinate boti.pos is a member of region r.
        }

        public override void Apply(State state, Bot bot)
        {
            var range = GetRegion(bot.Position);
            foreach (var pos in range)
            {
                if (state.Matrix.IsVoidVoxel(pos))
                {
                    state.Matrix.Fill(pos);
                    state.Energy += 12;
                }
                else
                {
                    state.Energy += 6;
                }
            }
        }

        [NotNull]
        public override Vec[] GetVolatileCells([NotNull] Bot bot)
        {
            return new []{ bot.Position };
        }
    }
}