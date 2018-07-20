using lib.Models;

namespace lib.Utils
{
    public class ThrowableHelper
    {
        private readonly Matrix toFill;
        private readonly Matrix filled;
        private readonly int n;

        public ThrowableHelper(Matrix toFill)
        {
            this.toFill = toFill;
            n = toFill.N;
            filled = new Matrix();
        }

        public bool TryFill(Vec bot, Vec cell)
        {
            if (filled[cell] || filled[bot])
                return false;

            filled[cell] = true;
            var result = Check();
            if (!result)
                filled[cell] = false;

            return result;
        }

        private bool Check()
        {
            
        }
    }
}