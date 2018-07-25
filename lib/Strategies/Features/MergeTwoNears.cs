using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Strategies.Features.Async;

namespace lib.Strategies.Features
{
    public class MergeTwoNears : Strategy
    {
        private readonly Bot master;
        private readonly Bot slave;

        public MergeTwoNears(State state, Bot master, Bot slave)
            : base(state)
        {
            this.master = master;
            this.slave = slave;
        }

        protected override async StrategyTask<bool> Run()
        {
            if (master.Position.GetNears().All(n => n != slave.Position))
                return false;

            state.SetBotCommand(master, new FusionP(new NearDifference(slave.Position - master.Position)));
            state.SetBotCommand(slave, new FusionS(new NearDifference(master.Position - slave.Position)));
            await WhenNextTurn();
            return true;
        }
    }
}