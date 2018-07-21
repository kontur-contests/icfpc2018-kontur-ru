using System.Collections.Generic;

using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Commands
{
    public class GFill : BaseCommand
    {
        public NearDifference NearShift { get; }
        public FarDifference FarShift { get; }

        public GFill(NearDifference nearShift, FarDifference farShift)
        {
            this.NearShift = nearShift;
            this.FarShift = farShift;
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

        public override bool CanApply(MutableState state, Bot bot)
        {
            return state.BuildingMatrix.IsInside(GetPosition(bot)) &&
                   state.BuildingMatrix.IsInside(GetPosition(bot) + FarShift);
            
            // Not checking these conditions:
            // * It is also an error if boti.pos + ndi = botj.pos + ndj (for i ≠ j).
            // * It is also an error if any coordinate boti.pos is a member of region r.
        }

        protected override void DoApply(MutableState mutableState, Bot bot)
        {
            var nearPos = GetPosition(bot);
            var farPos = nearPos + FarShift;
            
            for (var x = nearPos.X; x < farPos.X; ++x)
            for (var y = nearPos.Y; y < farPos.Y; ++y)
            for (var z = nearPos.Z; z < farPos.Z; ++z)
            {
                var pos = new Vec(x, y, z);
                if (mutableState.BuildingMatrix.IsVoidVoxel(pos))
                {
                    mutableState.BuildingMatrix.Fill(pos);
                    mutableState.Energy += 12;
                }
                else
                {
                    mutableState.Energy += 6;
                }
            }
        }

        [NotNull]
        public override Vec[] GetVolatileCells([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            var volatileCells = new List<Vec>
                {
                    // Adding only this bot's position as a volatile cell.
                    // All bots doing GFill should do the same
                    bot.Position
                };

            var nearPos = GetPosition(bot);
            var farPos = nearPos + FarShift;
            
            for (var x = nearPos.X; x < farPos.X; ++x)
            for (var y = nearPos.Y; y < farPos.Y; ++y)
            for (var z = nearPos.Z; z < farPos.Z; ++z)
            {
                var pos = new Vec(x, y, z);
                volatileCells.Add(pos);
            }
            
            return volatileCells.ToArray();
        }

        [NotNull]
        private Vec GetPosition([NotNull] Bot bot)
        {
            return bot.Position + NearShift;
        }
    }
}