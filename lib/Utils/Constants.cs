using System.Collections.Generic;

namespace lib.Utils
{
    public static class Constants
    {
        public static List<Vec> NeighborDeltas = new List<Vec>
            {
                new Vec(0, 0, 1),
                new Vec(0, 0, -1),
                new Vec(0, 1, 0),
                new Vec(0, -1, 0),
                new Vec(1, 0, 0),
                new Vec(-1, 0, 0),
            };
    }
}