using System;

using lib.Utils;

namespace lib.Strategies.Features
{
    public interface IGeneralPlan
    {
        Region GetNextRegion(Predicate<Region> isAcceptableRegion);

        void GroundRegion(Region region);

        bool IsComplete { get; }
    }
}