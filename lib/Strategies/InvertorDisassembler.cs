using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Primitives;

using MoreLinq;

namespace lib.Strategies
{
    public class InvertorDisassembler : IAmSolver
    {
        private readonly IAmSolver assembler;

        public InvertorDisassembler(IAmSolver assembler)
        {
            this.assembler = assembler;
        }

        public IEnumerable<ICommand> Solve()
        {
            var commands = assembler.Solve().ToList();
            commands.Reverse();
            return commands.Select(Reverse).Where(c => c != null).Concat(new []{new Halt()});
        }

        private ICommand Reverse(ICommand command)
        {
            if (command is SMove smove) return new SMove(new LongLinearDifference(-smove.shift.Shift));
            if (command is Fill fill) return new Voidd(fill.Shift);
            if (command is Wait wait) return wait;
            if (command is LMove move) return new LMove(new ShortLinearDifference(-move.secondShift.Shift), new ShortLinearDifference(-move.firstShift.Shift));
            return null;
        }
    }
}