using System.Windows.Input;

namespace lib.Models
{
    public class ApplyingState
    {
        public MutableState State { get; set; }
        public ICommand[] Commands { get; set; }
    }
}