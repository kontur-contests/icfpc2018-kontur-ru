using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using lib.Utils;

namespace lib.Primitives
{
    public class LinearDifference
    {
        public Vec Shift { get; }

        public override string ToString()
        {
            return Shift.ToString();
        }

        protected LinearDifference([NotNull] Vec shift, int maxLength)
        {
            MaxLength = maxLength;
            if (!shift.IsLinear())
                throw new ArgumentException("should be linear", nameof(shift));
            if (shift.MLen() > MaxLength)
                throw new ArgumentException($"should be not mlonger than {MaxLength}", nameof(shift));
            this.Shift = shift;
        }

        protected LinearDifference(int a, int i, int maxLength)
        {
            MaxLength = maxLength;
            var shift = i - maxLength;
            if (shift.Abs() > maxLength)
                throw new ArgumentException($"shift parameter must be in range [{-maxLength}, {maxLength}], but was {shift}");
            if (a == 0b01)
                Shift = new Vec(shift, 0, 0);
            else if (a == 0b10)
                Shift = new Vec(0, shift, 0);
            else if (a == 0b11)
                Shift = new Vec(0, 0, shift);
            else
                throw new ArgumentException("invalid 'a' parameter");
        }

        public (int A, int I) GetParameters()
        {
            if (Shift.X != 0)
                return (0b01, Shift.X + MaxLength);
            if (Shift.Y != 0)
                return (0b10, Shift.Y + MaxLength);
            if (Shift.Z != 0)
                return (0b11, Shift.Z + MaxLength);
            throw new Exception($"Invalid {nameof(LongLinearDifference)}");
        }

        public static implicit operator Vec([NotNull] LinearDifference linearDifference)
        {
            return linearDifference.Shift;
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
            return Shift.Sign();
        }

        protected int MaxLength { get; }
    }
}