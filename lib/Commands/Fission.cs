using System.Linq;

using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Commands
{
    public class Fission : BaseCommand
    {
        private readonly NearDifference shift;
        private readonly int m;

        public Fission(NearDifference shift, int m)
        {
            this.shift = shift;
            this.m = m;
        }

        public override string ToString()
        {
            return $"Fission({shift}, {m})";
        }

        [NotNull]
        public override byte[] Encode()
        {
            return new [] {(byte)((shift.GetParameter() << 3) | 0b101), (byte)m};
        }

        public override bool CanApply(MutableState state, Bot bot)
        {
            if (bot.Seeds.Count < m + 1)
                return false;
            if (!state.Matrix.IsInside(bot.Position + shift))
                return false;
            if (!state.Matrix.IsVoidVoxel(bot.Position + shift))
                return false;
            return true;
        }

        protected override void DoApply([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            var bids = bot.Seeds.Take(m + 1).ToArray();

            bot.Seeds = bot.Seeds.Skip(m + 1).ToList();
            var newBot = new Bot
                {
                    Bid = bids.First(),
                    Position = bot.Position + shift,
                    Seeds = bids.Skip(1).ToList(),
                };
            mutableState.Bots.Add(newBot);
            mutableState.Energy += 24;
        }

        [NotNull]
        public override Vec[] GetVolatileCells([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            return new[] {bot.Position, bot.Position + shift};
        }
    }
}