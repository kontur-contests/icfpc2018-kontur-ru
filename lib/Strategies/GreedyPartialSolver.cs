using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Strategies
{
    public class GreedyPartialSolver : IAmSolver
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

        private readonly ICandidatesOrdering candidatesOrdering;

        public GreedyPartialSolver(Matrix targetMatrix, IOracle oracle, ICandidatesOrdering candidatesOrdering = null)
            : this(targetMatrix.Voxels, new bool[targetMatrix.R, targetMatrix.R, targetMatrix.R], Vec.Zero, oracle, candidatesOrdering)
        {

        }

        public GreedyPartialSolver(Matrix targetMatrix, Matrix sourceMatrix, IOracle oracle, ICandidatesOrdering candidatesOrdering = null)
            : this(targetMatrix.Voxels, sourceMatrix.Voxels, Vec.Zero, oracle, candidatesOrdering)
        {

        }

        public GreedyPartialSolver(bool[,,] whatToFill, bool[,,] state, Vec pos, IOracle oracle, ICandidatesOrdering candidatesOrdering = null)
        {
            this.whatToFill = whatToFill;
            this.state = state;
            this.pos = pos;
            this.oracle = oracle;
            this.candidatesOrdering = candidatesOrdering ?? new BuildAllStayingStill();
            R = whatToFill.GetLength(0);
        }


        private List<ICommand> Commands { get; } = new List<ICommand>();
        public static StatValue candidatesCount = new StatValue();
        public IEnumerable<ICommand> Solve()
        {
            // ! красить можно то, что не покрашено, и после покраски станет граундед
            // строим список того, что можно красить, сортированный по расстоянию до бота (candidates)
            // while !empty (candidates)
            //   для каждой:
            //      перебираем near-позиции с которых красить, сортировано по расстоянию до бота
            //         выбираем ту, с которой оракул разрешает красить
            //   перемещаемся в ту точку, красим, обновляем список (добавляем ноды и сортируем заново)
            // в конце возвращаемся в 0 и HALT
            Commands.Clear();
            HashSet<Vec> candidates = BuildCandidates();
            while (candidates.Any())
            {
                candidatesCount.Add(candidates.Count);
                var candidatesAndPositions = OrderCandidates(candidates);
                var any = false;
                foreach (var (candidate, nearPosition) in candidatesAndPositions)
                {
                    if (Move(nearPosition))
                    {
                        any = true;
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
                        break;
                    }
                }
                foreach (var command in Commands)
                {
                    yield return command;
                }
                Commands.Clear();
                if (!any)
                    throw new Exception("Can't move");
            }
            Move(Vec.Zero);
            Commands.Add(new Halt());
            foreach (var command in Commands)
            {
                yield return command;
            }
            Commands.Clear();
        }

        private bool Move(Vec target)
        {
            var pathFinder = new PathFinder(state, pos, target, null, null);
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

        private IEnumerable<(Vec candidate, Vec nearPosition)> OrderCandidates(HashSet<Vec> candidates)
        {
            foreach (var candidate in candidatesOrdering.Order(candidates, pos))
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