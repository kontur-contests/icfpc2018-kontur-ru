using JetBrains.Annotations;

using lib.Models;
using lib.Utils;

namespace lib.Commands
{
    public interface ICommand
    {
        byte[] Encode();
        void Apply([NotNull] DeluxeState state, [NotNull] Bot bot);
        bool AllPositionsAreValid([NotNull] IMatrix matrix, [NotNull] Bot bot);
        [NotNull]
        Vec[] GetVolatileCells([NotNull] Bot bot);

        bool HasVolatileConflicts(Bot bot, DeluxeState state);
    }
}