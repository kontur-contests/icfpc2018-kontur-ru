using System.Linq;

using JetBrains.Annotations;

using lib.Models;

namespace lib.Commands
{
    public class Halt : BaseCommand
    {
        [NotNull]
        public override byte[] Encode()
        {
            return new byte[] {0b11111111};
        }

        public override bool CanApply([NotNull] MutableState state)
        {
            if (state.Bots.Count != 1)
                return false;
            if (state.Bots.Single().Position.MLen() != 0)
                return false;
            if (state.Harmonics != Harmonics.Low)
                return false;
            return true;
        }

        protected override void DoApply([NotNull] MutableState mutableState)
        {
            mutableState.Bots.Clear();
        }
    }
}