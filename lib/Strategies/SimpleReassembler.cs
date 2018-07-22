using System.Collections.Generic;
using System.Linq;

using lib.Commands;

namespace lib.Strategies
{
    public class SimpleReassembler : IAmSolver
    {
        public SimpleReassembler(IAmSolver disassembler, IAmSolver assembler)
        {
            this.disassembler = disassembler;
            this.assembler = assembler;
        }

        private readonly IAmSolver disassembler;
        private readonly IAmSolver assembler;

        public IEnumerable<ICommand> Solve()
        {
            return disassembler.Solve().Where(c => !(c is Halt)).Concat(assembler.Solve());
        }
    }
}