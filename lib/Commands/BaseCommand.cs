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
        public abstract Vec[] GetVolatileCells(MutableState mutableState, Bot bot);

        public void Apply([NotNull] MutableState mutableState, [NotNull] Bot bot)
        {
            if (!CanApply(mutableState, bot))
                throw new Exception($"Can't apply command {this}");
            DoApply(mutableState, bot);
        }

        public abstract bool CanApply([NotNull] MutableState state, [NotNull] Bot bot);

        protected abstract void DoApply([NotNull] MutableState mutableState, [NotNull] Bot bot);
    }
}