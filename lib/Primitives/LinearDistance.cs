using System;

using JetBrains.Annotations;

using lib.Commands;
using lib.Utils;

namespace lib.Primitives
{
    public abstract class LinearDistance
    {
        private readonly Vec shift;

        protected LinearDistance([NotNull] Vec shift, int maxLength)
        {
            MaxLength = maxLength;
            if (!shift.IsLinear())
                throw new ArgumentException("should be linear", nameof(shift));
            if (shift.MLen() > MaxLength)
                throw new ArgumentException($"should be not mlonger than {MaxLength}", nameof(shift));
            this.shift = shift;
        }

        public (int A, int I) GetParameters()
        {
            if (shift.X != 0)
                return (0b01, shift.X + MaxLength);
            if (shift.Y != 0)
                return (0b10, shift.Y + MaxLength);
            if (shift.Z != 0)
                return (0b11, shift.Z + MaxLength);
            throw new Exception($"Invalid {nameof(LongLinearDistance)}");
        }

        protected int MaxLength { get; }
    }
}