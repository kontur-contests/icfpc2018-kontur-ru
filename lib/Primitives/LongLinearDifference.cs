using JetBrains.Annotations;

using lib.Utils;

namespace lib.Primitives
{
    public class LongLinearDifference : LinearDifference
    {
        public LongLinearDifference([NotNull] Vec shift)
            : base(shift, 15)
        {
        }

        public LongLinearDifference(int a, int i)
            : base(a, i, 15)
        {
        }
    }
}