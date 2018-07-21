using lib.Models;

namespace lib.Utils
{
    public class ThrowableHelperFast : IOracle
    {
        private readonly Matrix filled;

        public ThrowableHelperFast(int n)
        {
            filled = new Matrix(n);
        }

        public bool TryFill(Vec cell, Vec bot)
        {
            if (filled[cell] || filled[bot])
                return false;

            var result = Check(cell);
            filled[cell] = true;

            foreach (var neighbour in cell.GetMNeighbours(filled))
            {
                if (filled[neighbour])
                    continue;

                result &= Check(neighbour);
            }

            result &= Check(bot);

            if (!result)
                filled[cell] = false;

            return result;
        }

        private static readonly Vec[] deltas =
            {
                new Vec(0, 0, 1),
                //new Vec(0, 0, -1) нельзя в пол
                new Vec(1, 0, 0),
                new Vec(-1, 0, 0),
                new Vec(0, 1, 0),
                new Vec(0, -1, 0),
            };

        private bool Check(Vec cell)
        {
            foreach (var delta in deltas)
            {
                var v = cell;
                while (true)
                {
                    v = v + delta;
                    if (!filled.IsInside(v))
                        return true;

                    if (filled[v])
                        break;
                }
            }

            return false;
        }
    }
}