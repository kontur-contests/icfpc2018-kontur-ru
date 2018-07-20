using System;

using lib.Models;

namespace lib.Commands
{
    public abstract class BaseCommand : ICommand
    {
        public abstract byte[] Encode();

        public void Apply(MutableState mutableState)
        {
            if (!CanApply(mutableState))
                throw new Exception("Can't apply command");
            DoApply(mutableState);
        }

        public abstract bool CanApply(MutableState state);

        protected abstract void DoApply(MutableState mutableState);
    }
}