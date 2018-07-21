using System;

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

        public void Apply([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            if (!AllPositionsAreValid(mutableState.BuildingMatrix, bot))
                throw new Exception($"Can't apply command {this}");
            DoApply(mutableState, bot);
        }

        public abstract void Apply(DeluxeState state, Bot bot);

        public abstract bool AllPositionsAreValid([NotNull] IMatrix matrix, [NotNull] Bot bot);

        protected abstract void DoApply([NotNull] MutableState mutableState, [NotNull] Bot bot);
    }
}