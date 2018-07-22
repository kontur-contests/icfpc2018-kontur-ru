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
            while (state.Bots.Any())
            {
                state.StartTick();
                for (var i = activeStrategies.Count - 1; i >= 0; i--)
                {
                    var strategy = activeStrategies[i];
                    var children = strategy.Tick();
                    if (strategy.Status == StrategyStatus.Incomplete)
                    {
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
                    }
                    else
                    {
                        if (children != null)
                            throw new InvalidOperationException($"Inactive strategy MUST NOT derive child strategies. Strategy: {strategy}; Derived: {string.Join("; ", children.Select(d => d.ToString()))}");
                        activeStrategies.RemoveAt(i);
                    }
                }
                if (activeStrategies.Count == 0 && state.Bots.Any())
                    throw new InvalidOperationException("State is not completed, but root strategy is done??? Maybe you forgot to return to ZERO and HALT???");
                foreach (var command in state.EndTick())
                    yield return command;
            }
        }
    }
}