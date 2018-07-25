using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;

namespace lib.Strategies.Features
{
    public class Disassembler : Strategy
    {
        public Disassembler(State state)
            : base(state)
        {
        }

        protected override async StrategyTask<bool> Run()
        {
            var split = new Split(state, state.Bots.Single(), 8);
            await split;

            await new Disassembler8(state, split.Bots);
            return await Finalize();
        }
    }
}