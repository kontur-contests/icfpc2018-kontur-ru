using System.Linq;
using System.Threading.Tasks;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Strategies.Features.Async;

namespace lib.Strategies.Features
{
    public class MergingTwo : SimpleStrategyBase
    {
        private readonly Bot bot1;
        private readonly Bot bot2;

        public MergingTwo(DeluxeState state, Bot bot1, Bot bot2)
            : base(state)
        {
            this.bot1 = bot1;
            this.bot2 = bot2;
        }

        protected override async StrategyTask<bool> Run()
        {
            if (await TryMerge(bot1, bot2))
                return true;

            if (await TryMerge(bot2, bot1))
                return true;

            if (await TryMeetAndMerge(bot1, bot2))
                return true;

            if (await TryMeetAndMerge(bot2, bot1))
                return true;

            return false;
        }

        private async Task<bool> TryMeetAndMerge(Bot src, Bot dst)
        {
            var nears = src.Position.GetNears();

            foreach (var near in nears)
            {
                if (await new MoveSingleBot(state, dst, near))
                {
                    return await TryMerge(src, dst);
                }
            }

            return false;
        }

        private async Task<bool> TryMerge(Bot src, Bot dst)
        {
            if (src.Position.GetNears().Any(n => n == dst.Position))
            {
                state.SetBotCommand(src, new FusionP(new NearDifference(src.Position - dst.Position)));
                state.SetBotCommand(dst, new FusionS(new NearDifference(dst.Position - src.Position)));
                await WhenNextTurn();
                return true;
            }

            return false;
        }
    }
}