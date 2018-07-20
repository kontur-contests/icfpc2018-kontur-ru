using JetBrains.Annotations;

using lib.Models;

namespace lib.Commands
{
    public interface ICommand
    {
        byte[] Encode();
        void Apply([NotNull] MutableState mutableState, [NotNull] Bot bot);
        //bool CanApply(ApplyingState state, Bot bot);
        //Vec[] GetVolatileCells(ApplyingState state);
    }
}