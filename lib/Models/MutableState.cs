using System.Collections.Generic;
using System.Windows.Input;

using JetBrains.Annotations;

namespace lib.Models
{
    public class MutableState
    {
        public long Energy { get; set; }
        public Harmonics Harmonics { get; set; }
        public Matrix Matrix { get; set; }
        public List<Bot> Bots { get; set; }

        public void Tick(Queue<ICommand> trace)
        {

        }

        [NotNull]
        public ApplyingState ToAppying(ICommand[] commands)
        {
            return new ApplyingState
                {
                    State = this,
                    Commands = commands,
                };
        }
    }
}