using System;
using System.Collections.Generic;

using lib.Utils;

namespace lib.Models
{
    public class Bot : IEquatable<Bot>
    {
        public int Bid { get; set; }
        public Vec Position { get; set; }
        public List<int> Seeds { get; set; }

        public override string ToString()
        {
            return $"{nameof(Bid)}: {Bid}, {nameof(Position)}: {Position}, {nameof(Seeds)}: [{string.Join(", ", Seeds)}]";
        }

        public bool Equals(Bot other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
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

        public static bool operator ==(Bot left, Bot right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Bot left, Bot right)
        {
            return !Equals(left, right);
        }
    }
}