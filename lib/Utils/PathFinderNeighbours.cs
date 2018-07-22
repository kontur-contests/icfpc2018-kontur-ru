using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using lib.Models;

namespace lib.Utils
{
    public class PathFinderNeighbours
    {
        private readonly IMatrix state;
        private readonly Vec source;
        private readonly Vec target;
        private readonly Predicate<Vec> isAllowedPosition;
        private readonly int R;

        public PathFinderNeighbours(IMatrix state, Vec source, Vec target, Predicate<Vec> isAllowedPosition = null)
        {
            this.state = state;
            this.source = source;
            this.target = target;
            this.isAllowedPosition = isAllowedPosition ?? (vec => true);
            R = state.R;
        }

        public bool TryFindPath()
        {
            return TryFindPath(out _);
        }

        public bool TryFindPath(out HashSet<Vec> used)
        {
            used = new HashSet<Vec>();
            if (source == target)
                return true;

            var queue = new SortedSet<Vec>(Comparer<Vec>.Create((a, b) =>
                {
                    var compareTo = a.MDistTo(target).CompareTo(b.MDistTo(target));
                    if (compareTo != 0)
                        return compareTo;
                    return Comparer<int>.Default.Compare(a.GetHashCode(), b.GetHashCode());
                }));
            queue.Add(source);
            used.Add(source);
            while (queue.Any())
            {
                var current = queue.First();
                if (current == target)
                {
                    return true;
                }
                queue.Remove(current);
                foreach (var next in current.GetMNeighbours(state))
                {
                    if (!state[next] && !used.Contains(next) && isAllowedPosition(next))
                    {
                        used.Add(next);
                        queue.Add(next);
                    }
                }
            }

            return false;
        }
    }
}