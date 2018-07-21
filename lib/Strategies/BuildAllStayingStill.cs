using System;
using System.Collections.Generic;
using System.Linq;

using lib.Utils;

using MoreLinq;

namespace lib.Strategies
{
    public class BuildAllStayingStill : ICandidatesOrdering
    {
        private readonly Func<Vec, Vec, double> keySelector;

        public BuildAllStayingStill(Func<Vec, Vec, double> keySelector = null)
        {
            this.keySelector = keySelector ?? ((p, b) => p.MDistTo(b));
        }

        public IEnumerable<Vec> Order(HashSet<Vec> candidates, Vec bot)
        {
            var nears = bot.GetNears().ToHashSet();
            return candidates
                .GroupBy(cand => nears.Contains(cand))
                .OrderByDescending(g => g.Key)
                .SelectMany(g => g.OrderBy(p => keySelector(p, bot)));
        }
    }
}