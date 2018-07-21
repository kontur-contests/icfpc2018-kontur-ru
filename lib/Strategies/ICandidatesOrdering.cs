using System.Collections.Generic;

using lib.Utils;

namespace lib.Strategies
{
    public interface ICandidatesOrdering
    {
        IEnumerable<Vec> Order(HashSet<Vec> candidates, Vec bot);
    }
}