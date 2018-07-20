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
    }
}