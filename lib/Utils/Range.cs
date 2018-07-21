using System;
using System.Collections;
using System.Collections.Generic;

namespace lib.Utils
{
    public class Range : IEnumerable<Vec>
    {
        public Vec Start { get; }
        public Vec End { get; }

        public Range(Vec start, Vec end)
        {
            Start = start;
            End = end;
        }

        public static Range ForShift(Vec start, Vec shift)
        {
            return new Range(start, start + shift);
        }

        public IEnumerator<Vec> GetEnumerator()
        {
            var minX = Math.Min(Start.X, End.X);
            var maxX = Math.Max(Start.X, End.X);
            
            var minY = Math.Min(Start.Y, End.Y);
            var maxY = Math.Max(Start.Y, End.Y);
            
            var minZ = Math.Min(Start.Z, End.Z);
            var maxZ = Math.Max(Start.Z, End.Z);
            
            for (var x = minX; x <= maxX; ++x)
            for (var y = minY; y <= maxY; ++y)
            for (var z = minZ; z <= maxZ; ++z)
            {
                yield return new Vec(x, y, z);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}