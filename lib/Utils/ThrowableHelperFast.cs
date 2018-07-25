using System;
using System.Collections.Generic;
using System.Linq;

using lib.Models;

namespace lib.Utils
{
    public class ThrowableHelperFast : IOracle
    {
        private readonly Matrix toFill;
        private readonly Matrix filled;
        private readonly Matrix<int> Closed;
        private readonly int n;

        public ThrowableHelperFast(Matrix toFill)
        {
            n = toFill.R;
            this.toFill = toFill;
            filled = new Matrix(n);
            Closed = new Matrix<int>(n);
        }

        public ThrowableHelperFast(Matrix initial, Matrix toFill) : this(toFill)
        {
            var R = toFill.R;
            for (int x = 0; x < R; x++)
                for (int y = 0; y < R; y++)
                    for (int z = 0; z < R; z++)
                        if (initial[x,y,z])
                            Fill(new Vec(x,y,z));
        }

        public bool CanFill(Vec cell, Vec bot, State state)
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
                    needFill |= state.Bots.Any(b => b.Position == v);

                    if (needFill && !CanFill(v, i))
                        return false;
                }
            }



            return state.Bots.All(b => Closed[b.Position] != 0b11111);
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

            return Closed[bot] != 0b11111;
        }

        private bool CanFill(Vec cell, int from)
        {
            var closed2 = Closed[cell] | (1 << from);
            
            return closed2 != 0b11111;
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

                    Closed[v] |= 1 << i;
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