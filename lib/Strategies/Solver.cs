using System.Collections.Generic;

using lib.Commands;
using lib.Models;

namespace lib.Strategies
{
    public class Solver : IAmSolver
    {
        private readonly DeluxeState state;
        private readonly List<IStrategy> activeStrategies = new List<IStrategy>();

        public Solver(DeluxeState state, IStrategy rootStrategy)
        {
            this.state = state;
            activeStrategies.Add(rootStrategy);
        }

        public IEnumerable<ICommand> Solve()
        {
            while (true)
            {
                state.StartTick();
                for (var i = activeStrategies.Count - 1; i >= 0; i--)
                {
                    var strategy = activeStrategies[i];
                    if (strategy.Tick() == StrategyStatus.Done)
                        activeStrategies.RemoveAt(i);
                }
                foreach (var command in state.EndTick())
                    yield return command;
            }
        }
    }
}