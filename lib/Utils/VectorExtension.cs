using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace lib.Utils
{
    public static class VectorExtension
    {
        public static bool IsLinear([NotNull] this Vec vec)
        {
            if (vec.X != 0 && vec.Y == 0 && vec.Z == 0)
                return true;
            if (vec.X == 0 && vec.Y != 0 && vec.Z == 0)
                return true;
            if (vec.X == 0 && vec.Y == 0 && vec.Z != 0)
                return true;
            return false;
        }

        public static List<Vec> GetNeighbors(this Vec vec)
        {
            var deltas = new List<Vec>
            {
                new Vec(0, 0, 1),
                new Vec(0, 0, -1),
                new Vec(0, 1, 0),
                new Vec(0, -1, 0),
                new Vec(1, 0, 0),
                new Vec(-1, 0, 0),
            };

            return deltas.Select(d => d + vec).ToList();
        }
    }
}