using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Strategies.Features.Async;

namespace lib.Strategies.Features
{
    public class MergeTwoNears : SimpleStrategyBase
    {
        private readonly Bot src;
        private readonly Bot dst;

        public MergeTwoNears(DeluxeState state, Bot src, Bot dst)
            : base(state)
        {
            this.src = src;
            this.dst = dst;
        }

        protected override async StrategyTask<bool> Run()
        {
            if (src.Position.GetNears().Any(n => n == dst.Position))
            {
                state.SetBotCommand(src, new FusionP(new NearDifference(dst.Position - src.Position)));
                state.SetBotCommand(dst, new FusionS(new NearDifference(src.Position - dst.Position)));
                await WhenNextTurn();
                return true;
            }

            return false;
        }
    }
}