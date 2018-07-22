using System;
using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Utils;

using MoreLinq;

namespace lib.Strategies
{
    public class SimpleReassembler : IAmSolver
    {
        public SimpleReassembler(IAmSolver disassembler, IAmSolver assembler, Matrix source, Matrix target)
        {
            this.disassembler = disassembler;
            this.assembler = assembler;
            this.source = source;
            this.target = target;
        }

        private readonly IAmSolver disassembler;
        private readonly IAmSolver assembler;
        private readonly Matrix source;
        private readonly Matrix target;

        public IEnumerable<ICommand> Solve()
        {
            var state = new DeluxeState(source, target);
            var disassembleCommands = disassembler.Solve().Where(c => !(c is Halt)).ToList();
            new Interpreter(state).RunPartially(disassembleCommands);
            var idMap = GetIdMap(state);
            var assembleCommands = GetAssembleCommands(idMap);
            return disassembleCommands.Concat(assembleCommands);
        }

        private IEnumerable<ICommand> GetAssembleCommands(int[] idMap)
        {
            var ticks = new List<List<(int bid, ICommand command)>>();
            var state = new DeluxeState(null, target);
            var originalTrace = new Queue<ICommand>(assembler.Solve());
            while (originalTrace.Any())
            {
                state.StartTick();
                var tick = new List<(int bid, ICommand command)>();
                foreach (var bot in state.Bots.OrderBy(b => b.Bid))
                {
                    var command = originalTrace.Dequeue();
                    state.SetBotCommand(bot, command);
                    tick.Add((bot.Bid, command));
                }
                ticks.Add(tick.OrderBy(t => idMap[t.bid]).ToList());
                state.EndTick();
            }
            return ticks.SelectMany(t => t.Select(botCommand => botCommand.command));
        }

        private static int[] GetIdMap(DeluxeState state)
        {
            var idMap = new int[41];
            var bot = state.Bots.Single();
            idMap[1] = bot.Bid;
            for (int i = 0; i < bot.Seeds.Count; i++)
                idMap[2 + i] = bot.Seeds[i];
            return idMap;
        }
    }
}