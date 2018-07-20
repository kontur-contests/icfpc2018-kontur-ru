using JetBrains.Annotations;

using lib.Models;
using lib.Utils;

namespace lib.Commands
{
    public interface ICommand
    {
        byte[] Encode();
        void Apply([NotNull] MutableState mutableState, [NotNull] Bot bot);
        //bool CanApply(ApplyingState state, Bot bot);
        [NotNull]
        Vec[] GetVolatileCells([NotNull] MutableState mutableState, [NotNull] Bot bot);
    }
}