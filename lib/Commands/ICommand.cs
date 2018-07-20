using lib.Models;

namespace lib.Commands
{
    public interface ICommand
    {
        byte[] Encode();
        void Apply(MutableState mutableState);
        bool CanApply(MutableState state);
    }
}