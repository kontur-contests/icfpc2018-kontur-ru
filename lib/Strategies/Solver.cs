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

        public IEnumerable<ICommand> SolvePartially()
        {
            while (true)
            {
                state.StartTick();
                for (var i = activeStrategies.Count - 1; i >= 0; i--)
                {
                    var strategy = activeStrategies[i];
                    RecursiveTick(strategy);
                    if (strategy.Status != StrategyStatus.InProgress)
                        activeStrategies.RemoveAt(i);
                }
                if (state.botCommands.Any())
                {
                    foreach (var command in state.EndTick())
                        yield return command;
                }
                if (activeStrategies.Count == 0)
                    break;
                Log.For(this).Info($"Tick #{state.Tick} done");
            }
        }

        public IEnumerable<ICommand> Solve()
        {
            while (state.Bots.Any())
            {
                state.StartTick();
                for (var i = activeStrategies.Count - 1; i >= 0; i--)
                {
                    var strategy = activeStrategies[i];
                    RecursiveTick(strategy);
                    if (strategy.Status != StrategyStatus.InProgress)
                        activeStrategies.RemoveAt(i);
                }
                if (activeStrategies.Count == 0 && state.Bots.Any())
                    throw new InvalidOperationException("State is not completed, but root strategy is done??? Maybe you forgot to return to ZERO and HALT???");
                foreach (var command in state.EndTick())
                    yield return command;
                Log.For(this).Info($"Tick #{state.Tick} done");
            }
        }

        private void RecursiveTick(IStrategy strategy)
        {
            int attempts;
            const int maxAttempts = 1_000;
            for (attempts = 0; attempts < maxAttempts; attempts++)
            {
                Log.For(this).Info($"{strategy} TICK");
                var children = strategy.Tick();
                Log.For(this).Info($"{strategy} TICK RESULT: {strategy.Status} - {string.Join(", ", children?.Select(c => c.ToString()) ?? new string[0])}");
                if (children == null)
                    break;
                if (strategy.Status != StrategyStatus.InProgress)
                    throw new InvalidOperationException($"Inactive strategy MUST NOT derive child strategies. Strategy: {strategy}; Derived: {string.Join("; ", children.Select(d => d.ToString()))}");

                var start = activeStrategies.Count;
                activeStrategies.AddRange(children);

                foreach (var child in children)
                    RecursiveTick(child);

                var allCompleted = true;
                for (int j = start + children.Length - 1; j >= start; j--)
                {
                    if (activeStrategies[j].Status != StrategyStatus.InProgress)
                        activeStrategies.RemoveAt(j);
                    else
                        allCompleted = false;
                }
                if (!allCompleted)
                    break;
            }
            if (attempts >= maxAttempts)
                Log.For(this).Warn($"Too many tick attempts for one strategy: {strategy}");
        }
    }
}