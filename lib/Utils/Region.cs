using System;
using System.Collections;
using System.Collections.Generic;

namespace lib.Utils
{
    public class Region : IEnumerable<Vec>, IEquatable<Region>
    {
        public bool Equals(Region other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Start, other.Start) && Equals(End, other.End);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Region)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Start != null ? Start.GetHashCode() : 0) * 397) ^ (End != null ? End.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Region left, Region right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Region left, Region right)
        {
            return !Equals(left, right);
        }

        public Vec Start { get; }
        public Vec End { get; }

        public Region(Vec start, Vec end)
        {
            var minX = Math.Min(start.X, end.X);
            var maxX = Math.Max(start.X, end.X);
            var minY = Math.Min(start.Y, end.Y);
            var maxY = Math.Max(start.Y, end.Y);
            var minZ = Math.Min(start.Z, end.Z);
            var maxZ = Math.Max(start.Z, end.Z);

            Start = new Vec(minX, minY, minZ);
            End = new Vec(maxX, maxY, maxZ);
        }

        public static Region ForShift(Vec start, Vec shift)
        {
            return new Region(start, start + shift);
        }

        public IEnumerator<Vec> GetEnumerator()
        {
            for (var x = Start.X; x <= End.X; ++x)
            for (var y = Start.Y; y <= End.Y; ++y)
            for (var z = Start.Z; z <= End.Z; ++z)
            {
                yield return new Vec(x, y, z);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Dim => (Start.X == End.X ? 0 : 1) + (Start.Y == End.Y ? 0 : 1) + (Start.Z == End.Z ? 0 : 1);
    }
}