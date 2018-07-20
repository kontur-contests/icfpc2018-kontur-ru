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
    }
}