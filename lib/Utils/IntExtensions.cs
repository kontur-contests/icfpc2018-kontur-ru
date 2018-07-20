using System;

namespace lib.Utils
{
    public static class IntExtensions
    {
        public static int Sign(this int x)
        {
            return Math.Sign(x);
        }

        public static int Abs(this int x)
        {
            return Math.Abs(x);
        }
    }
}