using lib.Models;

namespace lib.Utils
{
    public class ThrowableHelper : IOracle
    {
        private readonly Matrix toFill;
        private readonly Matrix filled;
        private Matrix used;
        private Vec[] queue;

        private readonly int n;

        public ThrowableHelper(Matrix toFill)
        {
            this.toFill = toFill;
            n = toFill.N;
            filled = new Matrix(n);
        }

        public bool TryFill(Vec cell, Vec bot)
        {
            if (filled[cell] || filled[bot])
                return false;

            filled[cell] = true;

            Bfs(new Vec(0, 0, 0));

            var result = true;
            for (int x = 0; x < n; x++)
            {
                for (int y = 0; y < n; y++)
                {
                    for (int z = 0; z < n; z++)
                    {
                        var v = new Vec(x, y, z);
                        if (used[v])
                            continue;
                        if (!filled[v] && toFill[v] || v == bot)
                            result = false;
                    }
                }
            }

            if (!result)
                filled[cell] = false;

            return result;
        }


        private void Bfs(Vec v)
        {
            used = new Matrix(n);
            queue = new Vec[n*n*n + 10];
            used[v] = true;
            int ql = 0, qr = 0;
            queue[qr++] = v;

            while (ql < qr)
            {
                v = queue[ql++];

                foreach (var u in v.GetNeighbors())
                {
                    if (used.IsInside(u) && !used[u] && !filled[u])
                    {
                        used[u] = true;
                        queue[qr++] = u;
                    }
                }
            }
        }
    }
}