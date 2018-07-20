using lib.Models;

namespace lib.Commands
{
    public interface ICommand
    {
        byte[] Encode();
        //void Apply(ApplyingState mutableState);
        //bool CanApply(ApplyingState state, Bot bot);
        //Vec[] GetVolatileCells(ApplyingState state);
    }
}