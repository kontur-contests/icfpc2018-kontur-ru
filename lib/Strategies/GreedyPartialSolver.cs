using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static int A = 0, B = 0;

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

        private readonly Comparison<Vec> comparison;

        public GreedyPartialSolver(bool[,,] whatToFill, bool[,,] state, Vec pos, IOracle oracle, Func<Vec, Vec, int> priorityByCandidateAndBot = null)
        {
            priorityByCandidateAndBot = priorityByCandidateAndBot ?? ((candidate, bot) => bot.MDistTo(candidate));
            this.whatToFill = whatToFill;
            this.state = state;
            this.pos = pos;
            this.oracle = oracle;
            R = whatToFill.GetLength(0); //523042236
            comparison = (a, b) => Comparer<int>.Default.Compare(priorityByCandidateAndBot(a, this.pos), priorityByCandidateAndBot(b, this.pos));
        }


        public List<ICommand> Commands { get; } = new List<ICommand>();

        public bool Solve(int timeoutMs = -1)
        {
            // ! красить можно то, что не покрашено, и после покраски станет граундед
            // строим список того, что можно красить, сортированный по расстоянию до бота (candidates)
            // while !empty (candidates)
            //   для каждой:
            //      перебираем near-позиции с которых красить, сортировано по расстоянию до бота
            //         выбираем ту, с которой оракул разрешает красить
            //   перемещаемся в ту точку, красим, обновляем список (добавляем ноды и сортируем заново)
            // в конце возвращаемся в 0 и HALT
            var sw = Stopwatch.StartNew();
            var candidates = BuildCandidates();
            int filledCount = 0;
            while (candidates.Any())
            {
                if (timeoutMs > 0 && sw.Elapsed.TotalMilliseconds > timeoutMs)
                {
                    Console.WriteLine(filledCount);
                    return false;
                }
                var candidatesAndPositions = OrderCandidates(candidates);
                var any = false;
                foreach (var (candidate, nearPosition) in candidatesAndPositions)
                {
                    if (Move(nearPosition))
                    {
                        any = true;
                        Fill(candidate);
                        filledCount++;
                        candidates.Remove(candidate);
                        foreach (var n in neighbors)
                        {
                            var neighbor = candidate + n;
                            if (neighbor.IsInCuboid(R)
                                && whatToFill.Get(neighbor)
                                && !state.Get(neighbor))
                                candidates.Add(neighbor);
                        }
                        break;
                    }
                }

                if (!any)
                    throw new Exception("Can't move");
            }
            Move(Vec.Zero);
            Commands.Add(new Halt());
            return true;
        }

        private bool Move(Vec target)
        {
            var pathFinder = new PathFinder(state, pos, target);
            var path = pathFinder.TryFindPath();
            if (path == null) return false;
            Commands.AddRange(path);
            pos = target;
            return true;
        }

        // old code from the past
        private void DumpNoPath(Vec target)
        {
            var all = whatToFill.Cast<bool>().Count(b => b);
            var done = Commands.Count(c => c is Fill);
            var bytes = CommandSerializer.Save(Commands.ToArray());
            File.WriteAllBytes($@"c:\temp\020_.nbt", bytes);

            var s = "";
            for (int y = 0; y < R; y++)
            {
                for (int z = 0; z < R; z++)
                {
                    for (int x = R - 1; x >= 0; x--)
                        s += state.Get(new Vec(x, y, z)) ? "X" : ".";
                    s += "\r\n";
                }
                s += "===r\n";
            }
            File.WriteAllText(@"c:\1.txt", s);

            throw new InvalidOperationException($"Couldn't find path from {pos} to {target}; all={all}; done={done}; Commands.Count={Commands.Count}; {string.Join("; ", Commands.Take(20))}");
        }

        private void Fill(Vec target)
        {
            Commands.Add(new Fill(new NearDifference(target - pos)));
            state.Set(target, true);
            oracle.Fill(target);
        }

        private IEnumerable<(Vec candidate, Vec nearPosition)> OrderCandidates(IEnumerable<Vec> candidates)
        {
            var list = candidates.ToList();
            list.Sort(comparison);
            var nears = pos.GetNears().ToHashSet();
            var orderedCandidates = list.GroupBy(cand => nears.Contains(cand)).OrderByDescending(g => g.Key).SelectMany(g => g);
            foreach (var candidate in orderedCandidates)
            {
                var nearPositions = candidate.GetNears().Where(n => n.IsInCuboid(R));
                foreach (var nearPosition in nearPositions.OrderBy(p => p.MDistTo(pos)))
                {
                    if (oracle.CanFill(candidate, nearPosition))
                    {
                        A++;
                        yield return (candidate, nearPosition);
                    }
                    B++;
                }
            }
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