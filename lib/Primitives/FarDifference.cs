using System;

using JetBrains.Annotations;

using lib.Utils;

namespace lib.Primitives
{
    public class FarDifference
    {
        public Vec Shift { get; }

        public FarDifference([NotNull] Vec shift)
        {
            if (shift.CLen() <= 0 || shift.CLen() > 30)
                throw new ArgumentException("should has 0 < clen <= 30", nameof(shift));
            this.Shift = shift;
        }

        public FarDifference(int parameterX, int parameterY, int parameterZ)
            : this(new Vec(parameterX - 30, parameterY - 30, parameterZ - 30))
        {
        }

        public override string ToString()
        {
            return Shift.ToString();
        }

        public int GetParameterX()
        {
            return Shift.X;
        }

        public int GetParameterY()
        {
            return Shift.Y;
        }

        public int GetParameterZ()
        {
            return Shift.Z;
        }

        public static implicit operator Vec([NotNull] FarDifference difference)
        {
            return difference.Shift;
        }
    }
}