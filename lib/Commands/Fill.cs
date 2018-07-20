using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Commands
{
    public class Fill : BaseCommand
    {
        private readonly NearLinearDistance shift;

        public Fill(NearLinearDistance shift)
        {
            this.shift = shift;
        }

        [NotNull]
        public override byte[] Encode()
        {
            return new [] {(byte)((shift.GetParameter() << 3) | 0b011)};
        }

        public override void Apply(MutableState mutableState, Bot bot)
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