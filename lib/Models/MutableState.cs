using System.Collections.Generic;

namespace lib.Models
{
    public class MutableState
    {
        public long Energy { get; set; }
        public Harmonics Harmonics { get; set; }
        public Model Model { get; set; }
        public List<Bot> Bots { get; set; }
    }
}