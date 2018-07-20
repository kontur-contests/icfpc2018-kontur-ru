using JetBrains.Annotations;

using lib.Utils;

namespace lib.Primitives
{
    public class ShortLinearDifference : LinearDifference
    {
        public ShortLinearDifference([NotNull] Vec shift)
            : base(shift, 5)
        {
        }

        public ShortLinearDifference(int a, int i)
            : base(a, i, 5)
        {
        }
    }
}