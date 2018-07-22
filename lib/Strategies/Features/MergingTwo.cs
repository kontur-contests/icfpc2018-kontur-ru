using System.Linq;

using lib.Models;
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
            /*if (await new MergeTwoNears(state, bot1, bot2))
                return true;*/

            var nears = bot1.Position.GetNears().Where(p => !state.Matrix[p]);

            foreach (var near in nears)
            {
                if (await new MoveSingleBot(state, bot2, near))
                {
                    return await new MergeTwoNears(state, bot1, bot2);
                }
            }

            return false;
        }
    }
}