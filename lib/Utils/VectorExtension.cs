using JetBrains.Annotations;

using lib.Utils;

namespace lib.Commands
{
    public static class VectorExtension
    {
        public static bool IsLinear([NotNull] this Vec vec)
        {
            if (vec.X != 0 && vec.Y == 0 && vec.Z == 0)
                return true;
            if (vec.X == 0 && vec.Y != 0 && vec.Z == 0)
                return true;
            if (vec.X == 0 && vec.Y == 0 && vec.Z != 0)
                return true;
            return false;
        }
    }
}