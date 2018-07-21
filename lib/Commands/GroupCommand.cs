using lib.Primitives;
using lib.Utils;

namespace lib.Commands
{
    public abstract class GroupCommand : BaseCommand
    {
        public NearDifference NearShift { get; }
        public FarDifference FarShift { get; }

        protected GroupCommand(NearDifference nearShift, FarDifference farShift)
        {
            NearShift = nearShift;
            FarShift = farShift;
        }

        public Region GetRegion(Vec origin)
        {
            var nearPos = origin + NearShift;
            return Region.ForShift(nearPos, FarShift);
        }
    }
}