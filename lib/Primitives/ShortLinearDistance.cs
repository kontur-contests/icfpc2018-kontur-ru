using JetBrains.Annotations;

using lib.Utils;

namespace lib.Primitives
{
    public class ShortLinearDistance : LinearDistance
    {
        public ShortLinearDistance([NotNull] Vec shift)
            : base(shift, 5)
        {
        }

        public ShortLinearDistance(int a, int i)
            : base(a, i, 5)
        {
        }
    }
}