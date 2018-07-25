using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;

namespace lib.Strategies.Features
{
    public class Assembler : Strategy
    {
        public Assembler(State state)
            : base(state)
        {
        }

        protected override async StrategyTask<bool> Run()
        {
            var split = new Split(state, state.Bots.Single(), 8);
            await split;

            var plan = new GenPlanBuilder(state).CreateGenPlan();
            var sorted = new GenPlanSorter(plan, R).Sort();
            if (!await new PlanAssembler(state, split.Bots, sorted))
                return false;
            return await new ReachFinalState(state);
        }
    }
}