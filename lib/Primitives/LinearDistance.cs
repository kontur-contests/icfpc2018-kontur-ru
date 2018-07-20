using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using lib.Commands;
using lib.Utils;

namespace lib.Primitives
{
    public class LinearDistance
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

        [NotNull]
        private static LinearDistance ParseFromParameters(int a, int i, int maxLength)
        {
            var shift = i - maxLength;
            if (shift.Abs() <= maxLength)

            if (a == 0b01)
                return new LinearDistance(new Vec(shift, 0, 0), maxLength);
            if (a == 0b10)
                return new LinearDistance(new Vec(0, shift, 0), maxLength);
            if (a == 0b11)
                return new LinearDistance(new Vec(0, 0, shift), maxLength);
            throw new ArgumentException("invalid 'a' parameter");
        }

        public static implicit operator Vec([NotNull] LinearDistance linearDistance)
        {
            return linearDistance.Shift;
        }

        [NotNull]
        public Vec[] GetTrace([NotNull] Vec from)
        {
            var dir = GetDirection();
            var curr = from;
            var trace = new List<Vec> {curr};
            for (int i = 0; i < Shift.MLen(); i++)
            {
                curr += dir;
                trace.Add(curr);
            }
            return trace.ToArray();
        }

        [NotNull]
        private Vec GetDirection()
        {
            return new Vec(Shift.X.Sign(), Shift.Y.Sign(), Shift.Z.Sign());
        }

        protected int MaxLength { get; }
    }
}