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
        public abstract void Apply([NotNull] MutableState mutableState, [NotNull] Bot bot);
        [NotNull]
        public abstract Vec[] GetVolatileCells(MutableState mutableState, Bot bot);

        /*
        public void Apply(ApplyingState mutableState)
        {
            if (!CanApply(mutableState))
                throw new Exception("Can't apply command");
            DoApply(mutableState);
        }

        public abstract bool CanApply(ApplyingState state);

        protected abstract void DoApply(ApplyingState mutableState);*/
    }
}