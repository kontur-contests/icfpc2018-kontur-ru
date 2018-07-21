using System.Collections.Generic;
using System.Linq;

using lib.Primitives;
using lib.Utils;

using MoreLinq;

namespace lib.Strategies
{
    public class NearToFarBottomToTopBuildingAround : ICandidatesOrdering
    {
        public IEnumerable<Vec> Order(HashSet<Vec> candidates, Vec bot)
        {
            return GetNearCandidates(bot).Where(candidates.Contains).OrderBy(c => c.Y).Concat(GetFarCandidates(candidates, bot));
        }

        private static IEnumerable<Vec> GetNearCandidates(Vec bot)
        {
            return new Cuboid(bot, 2).AllPoints();
        }

        private IEnumerable<Vec> GetFarCandidates(HashSet<Vec> candidates, Vec bot)
        {
            var nears = GetNearCandidates(bot).ToHashSet();
            return candidates.Where(c => !nears.Contains(c)).OrderBy(c => c.Y).ThenBy(c => c.MDistTo(bot));
        }
    }
}