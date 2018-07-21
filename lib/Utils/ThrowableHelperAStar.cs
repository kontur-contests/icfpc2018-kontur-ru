using System.Linq;

using lib.Models;

namespace lib.Utils
{
    public class ThrowableHelperAStar : IOracle
    {
        private readonly Matrix filled;
        
        public ThrowableHelperAStar(int n)
        {
            filled = new Matrix(n);
        }

        public bool CanFill(Vec cell, Vec bot)
        {
            if (filled[cell] || filled[bot])
                return false;

            var result = Check(cell, bot);

            filled[cell] = false;

            return result;
        }

        public void Fill(Vec cell)
        {
            filled[cell] = true;
        }

        private bool Check(Vec cell, Vec bot)
        {
            filled[cell] = true;

            var toCheck = cell.GetMNeighbours(filled).Where(c => !filled[c]).ToList();
            return toCheck.All(c => new PathFinderNeighbours(filled, c, Vec.Zero).TryFindPath());
        }
    }
}