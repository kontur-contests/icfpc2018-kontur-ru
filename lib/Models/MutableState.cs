using System.Collections.Generic;
using System.Windows.Input;

using JetBrains.Annotations;

namespace lib.Models
{
    public class MutableState
    {
        public long Energy { get; set; }
        public Harmonics Harmonics { get; set; }
        public Model Model { get; set; }
        public List<Bot> Bots { get; set; }

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