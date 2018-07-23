using System;
using System.Collections.Generic;
using System.Linq;

using lib.Utils;

using MoreLinq;

namespace lib.Strategies.Features
{
    public static class GenPlanSorter
    {
        public static IEnumerable<Region> DefaultSort(this IEnumerable<Region> genPlan)
        {
            var nonGroundedSet = genPlan.ToHashSet();
            var toGroundSet = new SortedSet<Region>(Comparer<Region>.Create((a, b) =>
                {
                    var compare = Comparer<int>.Default.Compare(Eval(a), Eval(b));
                    if (compare != 0)
                        return compare;
                    return Comparer<string>.Default.Compare(a.ToString(), b.ToString());
                }));
            var resultSet = new List<Region>();

            while (nonGroundedSet.Any() || toGroundSet.Any())
            {
                var newToGround = nonGroundedSet.Where(r => CanBeGrounded(resultSet, r)).ToList();
                if (newToGround.Any())
                {
                    nonGroundedSet.ExceptWith(newToGround);
                    toGroundSet.UnionWith(newToGround);
                }
                else
                {
                    var nextItem = toGroundSet.First();
                    resultSet.Add(nextItem);
                    toGroundSet.Remove(nextItem);
                }
            }

            return resultSet;
        }

        private static bool CanBeGrounded(IEnumerable<Region> resultSet, Region region)
        {
            return region.Start.Y == 0 || resultSet.Any(res => Connected(res, region));
        }

        public static bool Connected(Region a, Region b)
        {
            return Connected1(a, b) || Connected1(b, a);
        }

        private static bool Connected1(Region a, Region b)
        {
            if (a.End.X + 1 == b.Start.X)
                return SquareIntersects(
                    a.Start.Y, a.Start.Z, a.End.Y, a.End.Z,
                    b.Start.Y, b.Start.Z, b.End.Y, b.End.Z);

            if (a.End.Y + 1 == b.Start.Y)
                return SquareIntersects(
                    a.Start.X, a.Start.Z, a.End.X, a.End.Z,
                    b.Start.X, b.Start.Z, b.End.X, b.End.Z);

            if (a.End.Z + 1 == b.Start.Z)
                return SquareIntersects(
                    a.Start.X, a.Start.Y, a.End.X, a.End.Y,
                    b.Start.X, b.Start.Y, b.End.X, b.End.Y);

            return false;
        }

        private static bool SquareIntersects(int ax1, int ay1, int ax2, int ay2, int bx1, int by1, int bx2, int by2)
        {
            return SegmentIntersects(ax1, ax2, bx1, bx2) && SegmentIntersects(ay1, ay2, by1, by2);
        }

        private static bool SegmentIntersects(int ax1, int ax2, int bx1, int bx2)
        {
            return Math.Max(ax1, bx1) <= Math.Min(ax2, bx2);
        }

        private static int Eval(Region r)
        {
            return r.Start.X * 1000 + r.Start.Z;
        }
    }
}