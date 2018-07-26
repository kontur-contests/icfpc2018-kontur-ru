using System;
using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Models;

namespace lib.Strategies
{
    public class Solver : IAmSolver
    {
        private readonly State state;
        private readonly IStrategy strategy;

        public Solver(State state, IStrategy strategy)
        {
            this.state = state;
            this.strategy = strategy;
        }

        public IEnumerable<ICommand> SolvePartially()
        {
            while (true)
            {
                state.StartTick();
                strategy.Tick();
                if (state.botCommands.Any())
                {
                    foreach (var command in state.EndTick())
                        yield return command;
                }
                if (strategy.Status != StrategyStatus.InProgress)
                    break;
                Log.For(this).Info($"Tick #{state.Tick} done");
            }
        }

        public IEnumerable<ICommand> Solve()
        {
            while (state.Bots.Any())
            {
                state.StartTick();
                strategy.Tick();
                if (strategy.Status != StrategyStatus.InProgress && state.Bots.Any())
                    throw new InvalidOperationException("State is not completed, but root strategy is done??? Maybe you forgot to return to ZERO and HALT???");
                foreach (var command in state.EndTick())
                    yield return command;
                Log.For(this).Info($"Tick #{state.Tick} done");
            }
        }
    }
}