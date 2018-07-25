using System.Linq;

using JetBrains.Annotations;

using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Commands
{
    public class Fission : BaseCommand
    {
        public readonly NearDifference shift;
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

        public override bool AllPositionsAreValid([NotNull] IMatrix matrix, Bot bot)
        {
            if (bot.Seeds.Count < m + 1)
                return false;
            if (!matrix.IsInside(bot.Position + shift))
                return false;
            if (!matrix.IsVoidVoxel(bot.Position + shift))
                return false;
            return true;
        }

        public override void Apply(State state, Bot bot)
        {
            var bids = bot.Seeds.Take(m + 1).ToArray();

            bot.Seeds = bot.Seeds.Skip(m + 1).ToList();
            var newBot = new Bot
                {
                    Bid = bids.First(),
                    Position = bot.Position + shift,
                    Seeds = bids.Skip(1).ToList(),
                };
            state.Bots.Add(newBot);
            state.Energy += 24;
        }

        [NotNull]
        public override Vec[] GetVolatileCells([NotNull] Bot bot)
        {
            return new[] {bot.Position, bot.Position + shift};
        }
    }
}