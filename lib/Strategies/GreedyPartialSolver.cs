using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using lib.Commands;
using lib.Primitives;
using lib.Utils;

using MoreLinq;

namespace lib.Strategies
{
    public class GreedyPartialSolver
    {
        private readonly bool[,,] whatToFill;
        private readonly bool[,,] state;
        private Vec pos;
        private readonly IOracle oracle;
        private readonly int R;
        private static readonly Vec[] neighbors =
            {
                new Vec(1, 0, 0),
                new Vec(0, 1, 0),
                new Vec(0, 0, 1),
                new Vec(-1, 0, 0),
                new Vec(0, -1, 0),
                new Vec(0, 0, -1)
            };

        private static readonly Vec[] nears =
            Enumerable.Range(-1, 3)
                      .SelectMany(x => Enumerable.Range(-1, 3).Select(y => new { x, y }))
                      .SelectMany(v => Enumerable.Range(-1, 3).Select(z => new Vec(v.x, v.y, z)))
                      .Where(v => v != Vec.Zero && v.MDistTo(Vec.Zero) <= 2).ToArray();

        private readonly Comparison<Vec> comparison;

        public GreedyPartialSolver(bool[,,] whatToFill, bool[,,] state, Vec pos, IOracle oracle)
        {
            this.whatToFill = whatToFill;
            this.state = state;
            this.pos = pos;
            this.oracle = oracle;
            R = whatToFill.GetLength(0);
            comparison = (a, b) => Comparer<int>.Default.Compare(a.MDistTo(this.pos), b.MDistTo(this.pos));
        }

        public List<ICommand> Commands { get; } = new List<ICommand>();

        public void Solve()
        {
            // ! красить можно то, что не покрашено, и после покраски станет граундед
            // строим список того, что можно красить, сортированный по расстоянию до бота (candidates)
            // while !empty (candidates)
            //   для каждой:
            //      перебираем near-позиции с которых красить, сортировано по расстоянию до бота
            //         выбираем ту, с которой оракул разрешает красить
            //   перемещаемся в ту точку, красим, обновляем список (добавляем ноды и сортируем заново)
            // в конце возвращаемся в 0 и HALT

            var candidates = BuildCandidates();

            while (candidates.Any())
            {
                var (candidate, nearPosition) = Decide(candidates);
                Move(nearPosition);
                Fill(candidate);

                candidates.Remove(candidate);
                foreach (var n in neighbors)
                {
                    var neighbor = candidate + n;
                    if (neighbor.IsInCuboid(R)
                        && whatToFill.Get(neighbor)
                        && !state.Get(neighbor))
                        candidates.Add(neighbor);
                }
            }
            Move(Vec.Zero);
            Commands.Add(new Halt());
        }

        private void Move(Vec target)
        {
            var pathFinder = new PathFinder(state, pos, target);
            var path = pathFinder.TryFindPath();
            if (path == null)
            {
                var all = whatToFill.Cast<bool>().Count(b => b);
                var done = Commands.Count(c => c is Fill);
                //for (int i = 0; i < Commands.Count; i++)
                //{
                //    var bytes = CommandSerializer.Save(Commands.Take(i + 1).ToArray());
                //    File.WriteAllBytes($@"c:\temp\007_{i:0000000}.nbt", bytes);
                //}

                //var s = "";
                //for (int y = 0; y < R; y++)
                //{
                //    for (int z = 0; z < R; z++)
                //    {
                //        for (int x = R- 1; x >= 0; x--)
                //            s += state.Get(new Vec(x, y, z)) ? "X" : ".";
                //        s += "\r\n";
                //    }
                //    s += "===r\n";
                //}
                //File.WriteAllText(@"c:\1.txt", s);

                throw new InvalidOperationException($"Couldn't find path from {pos} to {target}; all={all}; done={done}; Commands.Count={Commands.Count}; {string.Join("; ", Commands)}");
            }
            Commands.AddRange(path);
            pos = target;
        }

        private void Fill(Vec target)
        {
            Commands.Add(new Fill(new NearDifference(target - pos)));
            state.Set(target, true);
        }

        private (Vec candidate, Vec nearPosition) Decide(IEnumerable<Vec> candidates)
        {
            var list = candidates.ToList();
            list.Sort(comparison);
            foreach (var candidate in list)
            {
                var nearPositions = nears.Select(n => n + candidate).Where(n => n.IsInCuboid(R)).ToList();
                nearPositions.Sort(comparison);
                foreach (var nearPosition in nearPositions)
                {
                    if (oracle.TryFill(candidate, nearPosition))
                        return (candidate, nearPosition);
                }
            }
            throw new InvalidOperationException("Couldn't decide what to fill next");
        }

        private HashSet<Vec> BuildCandidates()
        {
            var result = new HashSet<Vec>();
            for (int x = 0; x < R; x++)
                for (int y = 0; y < R; y++)
                    for (int z = 0; z < R; z++)
                    {
                        var vec = new Vec(x, y, z);
                        if (whatToFill.Get(vec)
                            && !state.Get(vec)
                            && (y == 0 || neighbors.Any(n =>
                                {
                                    var nvec = vec + n;
                                    return nvec.IsInCuboid(R) && state.Get(nvec);
                                })))
                        {
                            result.Add(vec);
                        }
                    }
            return result;
        }
    }
}