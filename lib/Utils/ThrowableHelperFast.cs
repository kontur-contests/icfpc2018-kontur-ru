using System;
using System.Collections.Generic;

using lib.Models;

namespace lib.Utils
{
    public class ThrowableHelperFast : IOracle
    {
        private readonly Matrix toFill;
        private readonly Matrix filled;
        private readonly Matrix<HashSet<int>> Closed;
        private readonly int n;

        public ThrowableHelperFast(Matrix toFill)
        {
            n = toFill.N;
            this.toFill = toFill;
            filled = new Matrix(n);

            Closed = new Matrix<HashSet<int>>(n);
            for (int x = 0; x < n; x++)
            for (int y = 0; y < n; y++)
            for (int z = 0; z < n; z++)
            {
                Closed[x, y, z] = new HashSet<int>();
            }
        }

        public bool CanFill(Vec cell, Vec bot)
        {
            if (filled[cell] || filled[bot])
                return false;

            for (int i = 0; i < deltas.Length; i++)
            {
                var delta = deltas[i];

                var v = cell;
                while (true)
                {
                    v = v + delta;
                    if (!toFill.IsInside(v))
                        break;

                    var needFill = toFill[v] && !filled[v];
                    needFill |= v == bot;

                    if (needFill && !CanFill(v, i))
                        return false;
                }
            }
            
            return true;
        }

        private bool CanFill(Vec cell, int from)
        {
            var closed2 = new HashSet<int>(Closed[cell]);
            closed2.Add(from);

            return closed2.Count < 5;
        }

        public void Fill(Vec cell)
        {
            filled[cell] = true;

            for (int i = 0; i < deltas.Length; i++)
            {
                var delta = deltas[i];

                var v = cell;
                while (true)
                {
                    v = v + delta;
                    if (!toFill.IsInside(v))
                        break;

                    Closed[v].Add(i);
                }
            }
        }

        private static readonly Vec[] deltas =
            {
                new Vec(0, 0, 1),
                new Vec(0, 0, -1),
                new Vec(1, 0, 0),
                new Vec(-1, 0, 0),
                //new Vec(0, 1, 0),
                new Vec(0, -1, 0),
            };
    }
}