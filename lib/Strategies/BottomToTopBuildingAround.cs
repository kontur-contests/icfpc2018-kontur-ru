using System.Collections.Generic;
using System.Linq;

using lib.Utils;

using MoreLinq;

namespace lib.Strategies
{
    public class BottomToTopBuildingAround : ICandidatesOrdering
    {
        public IEnumerable<Vec> Order(HashSet<Vec> candidates, Vec bot)
        {
            var nears = bot.GetNears().ToHashSet();
            return candidates.OrderBy(c => c.Y).ThenByDescending(c => nears.Contains(c)).ThenBy(c => c.MDistTo(bot));
        }
    }
}