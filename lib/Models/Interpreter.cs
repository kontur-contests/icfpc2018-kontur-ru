using System;
using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Utils;

using MoreLinq;

namespace lib.Models
{
    public class Interpreter
    {
        private readonly DeluxeState state;

        public Interpreter(DeluxeState state)
        {
            this.state = state;
        }

        public void Run(IEnumerable<ICommand> trace)
        {
            var traceQueue = new Queue<ICommand>(trace);
            while (traceQueue.Any())
                Tick(traceQueue);
            EnsureIsFinal();
        }

        public void Tick(Queue<ICommand> trace)
        {
            state.StartTick();
            var botCommands = state.Bots.OrderBy(b => b.Bid).Select(bot => new { bot, command = trace.Dequeue() }).ToList();
            foreach (var bc in botCommands)
                state.SetBotCommand(bc.bot, bc.command);
            state.EndTick();
            LastChangedCells = state.VolatileCells.Keys.ToHashSet();
        }

        public HashSet<Vec> LastChangedCells { get; private set; }

        public void EnsureIsFinal()
        {
            if (state.Bots.Any())
                throw new InvalidOperationException("State is not final");
            for (int i = 0; i < state.TargetMatrix.R; i++)
                for (int j = 0; j < state.TargetMatrix.R; j++)
                    for (int k = 0; k < state.TargetMatrix.R; k++)
                        if (state.TargetMatrix[i, j, k] != state.Matrix[i, j, k])
                            throw new InvalidOperationException("BuildingMatrix differs from targetMatrix");
        }
    }
}