using System.Collections.Generic;
using System.IO;
using System.Linq;

using lib.Models;

namespace lib.Utils
{
    public class PathFinderNeighbours
    {
        private readonly Matrix state;
        private readonly Vec source;
        private readonly Vec target;
        private readonly int R;

        public PathFinderNeighbours(Matrix state, Vec source, Vec target)
        {
            this.state = state;
            this.source = source;
            this.target = target;
            R = state.R;
        }

        public bool TryFindPath()
        {
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
            var used = new HashSet<Vec>();
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
                    if (!state[next] && !used.Contains(next))
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