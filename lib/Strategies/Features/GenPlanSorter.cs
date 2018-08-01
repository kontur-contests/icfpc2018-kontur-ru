using System;
using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class GenPlanSorter :IGeneralPlan
    {
        private readonly Matrix<int> regionIndex;
        private readonly List<Region> regions;
        private readonly HashSet<int> used = new HashSet<int>();
        private readonly int n;
        private readonly SortedSet<Region> toGroundSet;

        public GenPlanSorter(List<Region> regions, int n)
        {
            regions = regions.OrderBy(r => r.Start.MDistTo(r.End)).ToList();

            this.regions = regions;
            this.n = n;
            regionIndex = new Matrix<int>(n);

            for (int x = 0; x < n; x++)
                for (int y = 0; y < n; y++)
                    for (int z = 0; z < n; z++)
                        regionIndex[x, y, z] = -1;

            toGroundSet = new SortedSet<Region>(Comparer<Region>.Create((a, b) =>
                {
                    var compare = Comparer<long>.Default.Compare(Eval(a), Eval(b));
                    if (compare != 0)
                        return compare;
                    return Comparer<string>.Default.Compare(a.ToString(), b.ToString());
                }));
            
            for (int i = 0; i < regions.Count; i++)
                foreach (var vec in regions[i])
                {
                    //if (regionIndex[vec] != -1)
                    //    throw new Exception("duplicate!");
                    regionIndex[vec] = i;
                }

            GroundRegion(new Region(new Vec(-1, -1, -1), new Vec(n, -1, n)));
        }

        public IEnumerable<Region> Sort()
        {
            while (!IsComplete)
            {
                yield return GetNextRegion(region => true);
            }
        }

        public Region GetNextRegion(Predicate<Region> isAcceptableRegion)
        {
            foreach (var region in toGroundSet)
            {
                if (isAcceptableRegion(region))
                {
                    toGroundSet.Remove(region);
                    return region;
                }
            }

            return null;
        }

        public bool IsComplete => !toGroundSet.Any();

        public void GroundRegion(Region region)
        {
            var nears = region.SelectMany(v => v.GetMNeighbours())
                              .Where(v => v.IsInCuboid(n))
                              .Distinct();

            foreach (var near in nears)
            {
                int reg = regionIndex[near];
                if (reg == -1)
                    continue;

                if (!used.Contains(reg))
                {
                    used.Add(reg);
                    toGroundSet.Add(regions[reg]);
                }
            }
        }

        private static long Eval(Region r)
        {
            return ((30*30*30) - r.Volume) * 1000 + r.Start.Y; // r.Start.X * 1000 + r.Start.Z;
        }
    }
}