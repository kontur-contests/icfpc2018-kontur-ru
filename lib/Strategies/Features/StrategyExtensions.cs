using System.Collections.Generic;

using lib.Commands;
using lib.Models;

namespace lib.Strategies.Features
{
    public static class StrategyExtensions
    {
        public static IEnumerable<ICommand> Run(this IStrategy strategy, DeluxeState startState)
        {
            var finalize = new Finalize(startState);
            return new Solver(startState, finalize).Solve();
        }

    }
}