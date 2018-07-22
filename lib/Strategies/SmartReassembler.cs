using System;
using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Models;

namespace lib.Strategies
{
    public class SmartReassembler : IAmSolver
    {
        public SmartReassembler(Matrix source, Matrix target, Func<Matrix, Matrix, IAmSolver> disassembler, Func<Matrix, Matrix, IAmSolver> assembler)
        {
            this.source = source;
            this.target = target;
            this.disassembler = disassembler;
            this.assembler = assembler;
        }

        private readonly Matrix source;
        private readonly Matrix target;
        private readonly Func<Matrix, Matrix, IAmSolver> disassembler;
        private readonly Func<Matrix, Matrix, IAmSolver> assembler;

        public IEnumerable<ICommand> Solve()
        {
            // disassembleSource = source
            // disassembleTarget = (source & target)
            // assembleSource = (source & target)
            // assembleTarget = target
            var commonPart = source.Intersect(target);
            commonPart = new ComponentTrackingMatrix(commonPart).GetGroundedVoxels();
            var d = disassembler(source, commonPart);
            var a = assembler(commonPart, target);
            foreach (var command in d.Solve())
            {
                if (command is Halt) continue;
                yield return command;
            }
            foreach (var command in a.Solve())
                yield return command;
        }
    }
}