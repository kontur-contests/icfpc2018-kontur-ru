using System;

using JetBrains.Annotations;

using lib.Commands;
using lib.Utils;

namespace lib.Primitives
{
    public abstract class LinearDistance
    {
        public Vec Shift { get; }

        protected LinearDistance([NotNull] Vec shift, int maxLength)
        {
            MaxLength = maxLength;
            if (!shift.IsLinear())
                throw new ArgumentException("should be linear", nameof(shift));
            if (shift.MLen() > MaxLength)
                throw new ArgumentException($"should be not mlonger than {MaxLength}", nameof(shift));
            this.Shift = shift;
        }

        public (int A, int I) GetParameters()
        {
            if (Shift.X != 0)
                return (0b01, Shift.X + MaxLength);
            if (Shift.Y != 0)
                return (0b10, Shift.Y + MaxLength);
            if (Shift.Z != 0)
                return (0b11, Shift.Z + MaxLength);
            throw new Exception($"Invalid {nameof(LongLinearDistance)}");
        }

        public static implicit operator Vec([NotNull] LinearDistance linearDistance)
        {
            return linearDistance.Shift;
        }

        protected int MaxLength { get; }
    }
}