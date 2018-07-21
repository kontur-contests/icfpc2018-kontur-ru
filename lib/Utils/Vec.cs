using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

using lib.Models;

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
        public static readonly Vec Zero = new Vec(0, 0, 0);
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
                return ((x * 256) ^ y) * 256 ^ z;
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

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(Vec a, Vec b)
        {
            return (a.x * b.x) + (a.y * b.y) + (a.z * b.z);
        }

        public int MDistTo(Vec other)
        {
            return (this - other).MLen();
        }

        public int CDistTo(Vec other)
        {
            return (this - other).CLen();
        }

        public IEnumerable<Vec> GetMNeighbours()
        {
            yield return new Vec(x, y, z - 1);
            yield return new Vec(x, y, z + 1);
            yield return new Vec(x - 1, y, z);
            yield return new Vec(x + 1, y, z);
            yield return new Vec(x, y - 1, z);
            yield return new Vec(x, y + 1, z);
        }

        private static readonly Vec[] nears =
            Enumerable.Range(-1, 3)
                      .SelectMany(x => Enumerable.Range(-1, 3).Select(y => new { x, y }))
                      .SelectMany(v => Enumerable.Range(-1, 3).Select(z => new Vec(v.x, v.y, z)))
                      .Where(v => v != Vec.Zero && v.MDistTo(Vec.Zero) <= 2).ToArray();

        public IEnumerable<Vec> GetNears()
        {
            return nears.Select(d => d + this);
        }

        public IEnumerable<Vec> GetMNeighbours(Matrix matrix)
        {
            return GetMNeighbours().Where(matrix.IsInside);
        }

        public bool IsInCuboid(int r)
        {
            return x >= 0 && x < r
                   && y >= 0 && y < r
                   && z >= 0 && z < r;
        }
    }
}