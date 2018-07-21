using System;
using System.Collections.Generic;
using lib.Utils;

namespace lib.Primitives
{
    public class Cuboid
    {
        public Cuboid(Vec center, int radius)
        {
            var shift = new Vec(radius, radius, radius);
            PMin = center - shift;
            PMax = center + shift;
        }

        public Cuboid(int size)
        {
            PMin = Vec.Zero;
            PMax = new Vec(size-1, size-1, size-1);
        }

        public static Cuboid FromPoints(Vec a, Vec b)
        {
            return new Cuboid(
                new Vec(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z)),
                new Vec(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z)));
        }

        public Cuboid(Vec pMin, Vec pMax)
        {
            PMin = pMin;
            PMax = pMax;
        }

        public Cuboid Intersect(Cuboid other)
        {
            var min1 = this.PMin;
            var min2 = other.PMin;
            var max1 = this.PMax;
            var max2 = other.PMax;
            return new Cuboid(
                new Vec(Math.Max(min1.X, min2.X), Math.Max(min1.Y, min2.Y), Math.Max(min1.Z, min2.Z)),
                new Vec(Math.Min(max1.X, max2.X), Math.Min(max1.Y, max2.Y), Math.Min(max1.Z, max2.Z))
                );
        }

        public IEnumerable<Vec> AllPoints()
        {
            for (int x = PMin.X; x <= PMax.X; x++)
            for (int y = PMin.Y; y <= PMax.Y; y++)
            for (int z = PMin.Z; z <= PMax.Z; z++)
            {
                yield return new Vec(x,y,z);
            }
        }

        public bool Contains(Vec p)
        {
            return
                p.X >= PMin.X && p.X <= PMax.X &&
                p.Y >= PMin.Y && p.Y <= PMax.Y &&
                p.Z >= PMin.Z && p.Z <= PMax.Z;
        }

        public readonly Vec PMin, PMax;
    }
}