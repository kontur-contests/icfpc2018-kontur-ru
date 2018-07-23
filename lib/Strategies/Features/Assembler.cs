using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;

namespace lib.Strategies.Features
{
    public class Assembler : Strategy
    {
        public Assembler(DeluxeState state)
            : base(state)
        {
        }

        protected override async StrategyTask<bool> Run()
        {
            var split = new Split(state, state.Bots.Single(), 8);
            await split;

            if (!await new PlanAssembler(state, split.Bots, new GenPlanBuilder(state).CreateGenPlan().DefaultSort()))
                return false;
            return await Finalize();
        }
    }
}