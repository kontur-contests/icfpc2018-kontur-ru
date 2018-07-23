using System.Collections.Generic;

using lib.Commands;
using lib.Models;

namespace lib.Strategies.Features
{
    public static class StrategyExtensions
    {
        public static IEnumerable<ICommand> Run(this IStrategy strategy, DeluxeState startState)
        {
            return new Solver(startState, strategy).SolvePartially();
        }

    }
}