using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace lib.Utils
{
    public interface IVec
    {
        int X { get; }
        int Y { get; }
        int Z { get; }
    }

    public class Vec : IVec, IEquatable<Vec>, IFormattable
    {
        private readonly int x, y, z;
        public int X => x;
        public int Y => y;
        public int Z => z;

        public Vec(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public int this[int dimension] => dimension == 0 ? x : dimension == 1 ? y : z;

        [Pure]
        public bool Equals(Vec other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return x == other.x && y == other.y && z == other.z;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return $"{x.ToString(format, formatProvider)} {y.ToString(format, formatProvider)} {z.ToString(format, formatProvider)}";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Vec)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((x * 397) ^ y) * 397 ^ z;
            }
        }

        public override string ToString()
        {
            return $"{x} {y} {z}";
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int MLen()
        {
            return Math.Abs(x) + Math.Abs(y) + Math.Abs(z);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CLen()
        {
            return Math.Max(Math.Max(Math.Abs(x), Math.Abs(y)), Math.Abs(z));
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator -(Vec a, IVec b)
        {
            return new Vec(a.x - b.X, a.y - b.Y, a.z - b.Z);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator -(Vec a)
        {
            return new Vec(-a.x, -a.y, -a.z);
        }

        public static bool operator ==(Vec left, Vec right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Vec left, Vec right)
        {
            return !Equals(left, right);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator +(Vec v, Vec b)
        {
            return new Vec(v.x + b.x, v.y + b.y, v.z + b.z);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator *(Vec a, int k)
        {
            return new Vec(a.x * k, a.y * k, a.z * k);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator /(Vec a, int k)
        {
            return new Vec(a.x / k, a.y / k, a.z / k);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator *(int k, Vec a)
        {
            return new Vec(a.x * k, a.y * k, a.z * k);
        }

        public int MDistTo(Vec other)
        {
            return (this - other).MLen();
        }

        public int CDistTo(Vec other)
        {
            return (this - other).CLen();
        }
    }
}