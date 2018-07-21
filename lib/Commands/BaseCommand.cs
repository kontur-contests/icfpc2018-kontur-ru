using JetBrains.Annotations;

using lib.Models;
using lib.Utils;

namespace lib.Commands
{
    public abstract class BaseCommand : ICommand
    {
        [NotNull]
        public abstract byte[] Encode();

        [NotNull]
        public abstract Vec[] GetVolatileCells(Bot bot);

        public abstract void Apply(DeluxeState state, Bot bot);

        public abstract bool AllPositionsAreValid([NotNull] IMatrix matrix, [NotNull] Bot bot);
    }
}