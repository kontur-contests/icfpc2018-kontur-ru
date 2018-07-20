using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;

namespace lib.Commands
{
    public class Fill : BaseCommand
    {
        private readonly NearLinearDistance shift;
        private readonly int m;

        public Fill(NearLinearDistance shift, int m)
        {
            this.shift = shift;
            this.m = m;
        }

        [NotNull]
        public override byte[] Encode()
        {
            return new [] {(byte)((shift.GetParameter() << 3) | 0b011)};
        }

        public override void Apply(MutableState mutableState, Bot bot)
        {
            var pos = bot.Position + shift;
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
    }
}