using System.Collections.Generic;

using lib.Commands;

namespace lib.Strategies
{
    public interface IAmSolver
    {
        IEnumerable<ICommand> Solve();
    }
}