using System;

using JetBrains.Annotations;

using lib.Utils;

namespace lib.Primitives
{
    public class NearLinearDistance
    {
        private readonly Vec shift;

        protected NearLinearDistance([NotNull] Vec shift)
        {
            if (shift.CLen() != 1)
                throw new ArgumentException("should has clen = 1", nameof(shift));
            if (shift.MLen() <= 0 || shift.MLen() > 2)
                throw new ArgumentException("should has 0 < mlen <= 2", nameof(shift));
            this.shift = shift;
        }

        public int GetParameter()
        {
            return (shift.X + 1) * 9 + (shift.Y + 1) * 3 + shift.Z + 1;
        }
    }
}