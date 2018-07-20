using System;

using JetBrains.Annotations;

using lib.Utils;

namespace lib.Primitives
{
    public class NearDifference
    {
        public Vec Shift { get; }

        public NearDifference([NotNull] Vec shift)
        {
            if (shift.CLen() != 1)
                throw new ArgumentException("should has clen = 1", nameof(shift));
            if (shift.MLen() <= 0 || shift.MLen() > 2)
                throw new ArgumentException("should has 0 < mlen <= 2", nameof(shift));
            this.Shift = shift;
        }

        public NearDifference(int parameter)
            : this(new Vec(parameter / 9 - 1,
                           (parameter / 3) % 3 - 1,
                           parameter % 3 - 1))
        {
        }

        public override string ToString()
        {
            return Shift.ToString();
        }

        public int GetParameter()
        {
            return (Shift.X + 1) * 9 + (Shift.Y + 1) * 3 + Shift.Z + 1;
        }

        public static implicit operator Vec([NotNull] NearDifference difference)
        {
            return difference.Shift;
        }
    }
}