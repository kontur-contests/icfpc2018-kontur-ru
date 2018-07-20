using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Primitives;

namespace lib.Utils
{
    public class PathFinder
    {
        private readonly bool[,,] state;
        private readonly Vec source;
        private readonly Vec target;
        private readonly int R;

        public PathFinder(bool[,,] state, Vec source, Vec target)
        {
            this.state = state;
            this.source = source;
            this.target = target;
            R = state.GetLength(0);
        }

        public List<ICommand> TryFindPath()
        {
            if (source == target)
                return new List<ICommand>();

            var queue = new SortedSet<Vec>(Comparer<Vec>.Create((a, b) => a.MDistTo(target).CompareTo(b.MDistTo(target))));
            queue.Add(source);
            var used = new Dictionary<Vec, (Vec prev, ICommand command)>();
            used.Add(source, (null, null));
            while (queue.Any())
            {
                var current = queue.First();
                if (current == target)
                {
                    var result = new List<ICommand>();
                    for (var v = target; v != null; v = used[v].prev)
                    {
                        if (used[v].command != null)
                            result.Add(used[v].command);
                    }
                    result.Reverse();
                    return result;
                }
                queue.Remove(current);
                foreach (var (command, next) in IteratePossibleCommands(current))
                {
                    if (!used.ContainsKey(next))
                    {
                        used.Add(next, (current, command));
                        queue.Add(next);
                    }
                }
            }
            return null;
        }

        private static readonly Vec[] neighbors =
            {
                new Vec(1, 0, 0),
                new Vec(0, 1, 0),
                new Vec(0, 0, 1),
                new Vec(-1, 0, 0),
                new Vec(0, -1, 0),
                new Vec(0, 0, -1)
            };

        private IEnumerable<(ICommand, Vec)> IteratePossibleCommands(Vec current)
        {
            foreach (var n in neighbors)
            {
                var shift = Vec.Zero;
                for (int len = 1; len <= 15; len++)
                {
                    shift += n;
                    var res = current + shift;
                    if (!res.IsInCuboid(R) || state.Get(res))
                        break;
                    yield return (new SMove(new LongLinearDifference(shift)), res);
                }
            }

            foreach (var fn in neighbors)
            {
                var fshift = Vec.Zero;
                for (int flen = 1; flen <= 5; flen++)
                {
                    fshift += fn;
                    var fres = current + fshift;
                    if (!fres.IsInCuboid(R) || state.Get(fres))
                        break;
                    foreach (var sn in neighbors)
                    {
                        if (fn * sn == 0)
                        {
                            var sshift = Vec.Zero;
                            for (int slen = 1; slen <= 5; slen++)
                            {
                                sshift += sn;
                                var res = fres + sshift;
                                if (!res.IsInCuboid(R) || state.Get(res))
                                    break;
                                yield return (new LMove(new ShortLinearDifference(fshift), new ShortLinearDifference(sshift)), res);
                            }
                        }
                    }
                }
            }
        }
    }
}