using System;

using lib.Models;

namespace lib.Commands
{
    public abstract class BaseCommand : ICommand
    {
        public abstract byte[] Encode();
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