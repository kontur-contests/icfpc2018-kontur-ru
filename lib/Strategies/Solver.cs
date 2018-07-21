using System;
using System.Collections.Generic;
using System.Linq;

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
                    var children = strategy.Tick();
                    if (children != null)
                    {
                        while (children.Any())
                        {
                            var newChildren = new List<IStrategy>();
                            foreach (var child in children)
                            {
                                var descendants = child.Tick();
                                if (child.Status == StrategyStatus.Incomplete)
                                {
                                    activeStrategies.Add(child);
                                    if (descendants != null)
                                        newChildren.AddRange(descendants);
                                }
                                else
                                {
                                    if (descendants != null)
                                        throw new InvalidOperationException($"Inactive strategy MUST NOT derive child strategies. Strategy: {child}; Derived: {string.Join("; ", descendants.Select(d => d.ToString()))}");
                                }
                            }
                            children = newChildren.ToArray();
                        }
                    }
                    if (strategy.Status != StrategyStatus.Incomplete)
                        activeStrategies.RemoveAt(i);
                }
                foreach (var command in state.EndTick())
                    yield return command;
            }
        }
    }
}