using JetBrains.Annotations;

using lib.Utils;

namespace lib.Primitives
{
    public class LongLinearDistance : LinearDistance
    {
        public LongLinearDistance([NotNull] Vec shift)
            : base(shift, 15)
        {
        }

        public LongLinearDistance(int a, int i)
            : base(a, i, 15)
        {
        }
    }
}