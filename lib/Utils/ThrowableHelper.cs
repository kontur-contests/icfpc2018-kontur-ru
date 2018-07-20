using lib.Models;

namespace lib.Utils
{
    public class ThrowableHelper
    {
        private readonly Matrix matrix;
        private readonly Matrix filled;
        private readonly int n;

        public ThrowableHelper(Matrix matrix)
        {
            this.matrix = matrix;
            n = matrix.N;
            filled = new Matrix();
        }

        public bool TryFill(Vec bot, Vec cell)
        {
            if (filled[cell] || filled[bot])
                return false;

            return true;
        }
    }
}