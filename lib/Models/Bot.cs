using System.Collections.Generic;

using JetBrains.Annotations;

using lib.Utils;

namespace lib.Models
{
    public class Bot
    {
        public int Bid { get; set; }
        public Vec Position { get; set; }
        public List<int> Seeds { get; set; }

        protected bool Equals([NotNull] Bot other)
        {
            return Bid == other.Bid;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Bot)obj);
        }

        public override int GetHashCode()
        {
            return Bid;
        }
    }
}