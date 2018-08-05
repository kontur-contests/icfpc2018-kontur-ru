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
        private readonly List<Region> erase;
        private readonly HashSet<int> used = new HashSet<int>();
        private readonly int n;
        private readonly SortedSet<Region> toGroundSet;
        private readonly SortedSet<Region> toEraseSet;
        private readonly HashSet<Region> notErased;

        public GenPlanSorter(List<Region> regions0, int n, State state)
        {
            regions = regions0.Where(r => r.ToGround).ToList();
            erase = regions0.Where(r => !r.ToGround).ToList();
            regions = regions.OrderBy(r => r.Start.MDistTo(r.End)).ToList();
            
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
            toEraseSet = new SortedSet<Region>(Comparer<Region>.Create((a, b) =>
                {
                    var compare = Comparer<long>.Default.Compare(-Eval(a), -Eval(b));
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
            for (int x = 0; x < n; x++)
                for (int y = 0; y < n; y++)
                    for (int z = 0; z < n; z++)
                        if (state.SourceMatrix[x, y, z])
                            GroundRegion(new Region(new Vec(x, y, z), new Vec(x, y, z)));

            foreach (var r in erase)
            {
                toEraseSet.Add(r);
            }
            notErased = new HashSet<Region>(erase);
        }

        public IEnumerable<Region> Sort()
        {
            while (!IsComplete)
            {
                var nextRegion = GetNextRegion(region => true);
                GroundRegion(nextRegion);
                yield return nextRegion;
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

            if (!toEraseSet.Any())
                return null;

            var top = notErased.OrderByDescending(e => e.End.Y).First().End.Y;

            foreach (var region in toEraseSet)
            {
                if (isAcceptableRegion(region)/* && top == region.End.Y*/)
                {
                    toEraseSet.Remove(region);
                    return region;
                }
            }

            return null;
        }

        public void ReturnRegion(Region region)
        {
            if (region.ToGround)
                toGroundSet.Add(region);
            else
                toEraseSet.Add(region);
        }

        public bool IsComplete => !toGroundSet.Any() && !toEraseSet.Any();

        public void GroundRegion(Region region)
        {
            if (!region.ToGround)
            {
                notErased.Remove(region);
                return;
            }

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
            return r.Start.Y * (30 * 30 * 30) * 100 + (30 * 30 * 30 - r.Volume);
            // return ((30*30*30) - r.Volume) * 1000 + r.Start.Y;
            // return r.Start.X * 1000 + r.Start.Z;
        }
    }
}