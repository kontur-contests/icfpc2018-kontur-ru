using System.Collections.Generic;
using System.Linq;

using lib.Models;

namespace lib.Utils
{
    public class ThrowableHelper : IOracle
    {
        private readonly Matrix toFill;
        private readonly Matrix filled;
        private readonly MatrixInt used;
        private Vec[] queue;
        private int timer = 1;

        private readonly int n;

        public ThrowableHelper(Matrix toFill)
        {
            this.toFill = toFill;
            n = toFill.N;
            filled = new Matrix(n);
            used = new MatrixInt(n);
            queue = new Vec[n * n * n + 10];
        }

        public bool TryFill(Vec cell, Vec bot)
        {
            if (filled[cell] || filled[bot])
                return false;

            var result = Check(cell, bot);

            if (!result)
                filled[cell] = false;

            return result;
        }

        private bool Check(Vec cell, Vec bot)
        {
            filled[cell] = true;

            var hasFree = bot.GetMNeighbours(toFill).Any(v => !filled[v]);
            if (!hasFree && bot != Vec.Zero)
                return false;

            int comps = 0;
            var toCheck = cell.GetMNeighbours(toFill).Where(c => !filled[c]).ToList();

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

                if (cells.Any(c => !filled[v] && toFill[v] || v == bot))
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
                        if (!filled[v] && toFill[v] || v == bot)
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