using System;
using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Primitives;

using MoreLinq;

namespace lib.Utils
{
    public class ThrowableHelper : IOracle
    {
        public static StatValue opt = new StatValue();
        private readonly Matrix toFill;
        private readonly Matrix filled;
        private readonly Matrix<int> used;
        private Vec[] queue;
        private int timer = 1;

        private readonly int n;

        public ThrowableHelper(Matrix toFill)
        {
            this.toFill = toFill;
            n = toFill.R;
            filled = new Matrix(n);
            used = new Matrix<int>(n);
            queue = new Vec[n * n * n + 10];
        }

        public bool HasConnectivityChangesInLocalCuboid(Vec cellToFill, int localityRadius)
        {
            var marks = GetMarks(cellToFill, localityRadius);
            filled[cellToFill] = true;
            try
            {
                var newMarks = GetMarks(cellToFill, localityRadius);
                var hasChanges = !marks.SequenceEqual(newMarks);
                if (hasChanges)
                {
                    //Console.WriteLine(marks.ToDelimitedString(" "));
                    //Console.WriteLine(newMarks.ToDelimitedString(" "));
                }
                return hasChanges;
            }
            finally
            {
                filled[cellToFill] = false;
            }
        }

        private List<int> GetMarks(Vec center, int localityRadius)
        {
            var marks = new List<int>();
            var mark = new Dictionary<Vec, int>();
            var nextMark = 1;
            var cuboid = new Cuboid(toFill.R).Intersect(new Cuboid(center, localityRadius));
            foreach (var p in cuboid.AllPoints())
            {
                if (p == center || filled[p]) continue;
                if (!mark.ContainsKey(p))
                {
                    mark[p] = nextMark;
                    var component = MoreEnumerable.TraverseDepthFirst(p, cur => cur.GetMNeighbours().Where(next => cuboid.Contains(next) && !filled[next] && !mark.ContainsKey(next)));
                    foreach (var componentItem in component)
                        mark[componentItem] = nextMark;
                    nextMark++;
                }
                marks.Add(mark[p]);
            }
            return marks;
        }

        public void Fill(Vec cell)
        {
            filled[cell] = true;
        }

        public bool CanFill(Vec cell, Vec bot)
        {
            return CanFill(cell, new List<Vec>() {bot});
        }

        public bool CanFill(Vec cell, List<Vec> bots)
        {
            if (filled[cell] || bots.Any(bot => filled[bot]))
                return false;
            if (!HasConnectivityChangesInLocalCuboid(cell, 1))
            {
                opt.Add(1);
                return true;
            }
            else
            {
                opt.Add(0);
            }

            var result = Check(cell, bots);

            if (!result)
                filled[cell] = false;

            return result;
        }

        private bool Check(Vec cell, List<Vec> bots)
        {
            filled[cell] = true;

            int comps = 0;
            var toCheck = cell.GetMNeighbours(toFill).Where(c => !filled[c]).ToList();

            var botLocations = new HashSet<Vec>(bots);

            timer++;
            foreach (var v in toCheck)
            {
                if (used[v] == timer)
                    continue;

                var (cells, finished) = BfsSmall(v);
                comps++;
                
                if (!finished) //TODO
                    continue;

                if (cells.Any(c => c == Vec.Zero))
                    continue;

                if (cells.Any(c => !filled[v] && toFill[v] || botLocations.Contains(v)))
                    return false;
            }

            if (comps == 1)
                return true;

            Bfs(Vec.Zero);

            var result = true;
            for (int x = 0; x < n; x++)
            {
                for (int y = 0; y < n; y++)
                {
                    for (int z = 0; z < n; z++)
                    {
                        var v = new Vec(x, y, z);
                        if (used[v] == timer)
                            continue;
                        if (!filled[v] && toFill[v] || botLocations.Contains(v))
                            result = false;
                    }
                }
            }
            return result;
        }

        private (List<Vec>, bool) BfsSmall(Vec v)
        {
            var result = new List<Vec>();
            used[v] = timer;
            int ql = 0, qr = 0;
            queue[qr++] = v;

            while (ql < qr && result.Count < 1000)
            {
                v = queue[ql++];
                result.Add(v);

                foreach (var u in v.GetMNeighbours(toFill))
                {
                    if (used[u] != timer && !filled[u])
                    {
                        used[u] = timer;
                        queue[qr++] = u;
                    }
                }
            }

            return (result, ql == qr);
        }

        private void Bfs(Vec v)
        {
            timer++;
            used[v] = timer;
            int ql = 0, qr = 0;
            queue[qr++] = v;

            while (ql < qr)
            {
                v = queue[ql++];

                foreach (var u in v.GetMNeighbours(toFill))
                {
                    if (used[u] != timer && !filled[u])
                    {
                        used[u] = timer;
                        queue[qr++] = u;
                    }
                }
            }
        }
    }
}