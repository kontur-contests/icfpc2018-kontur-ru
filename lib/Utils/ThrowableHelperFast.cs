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

            var result = Check(cell, bot);

            if (!result)
                filled[cell] = false;

            return result;
        }

        private bool Check(Vec cell, Vec bot)
        {
            foreach (var delta in Vec.Zero.GetMNeighbours())
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