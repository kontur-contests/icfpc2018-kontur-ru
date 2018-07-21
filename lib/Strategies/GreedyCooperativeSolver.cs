using System;
using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Primitives;
using lib.Utils;

using MoreLinq;

namespace lib.Strategies
{
    public class GreedyCooperativeSolver : IAmSolver
    {
        public static int A = 0, B = 0;

        private readonly bool[,,] whatToFill;
        private readonly bool[,,] state;
        private Vec[] pos;
        private Region[] regions;
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

        public GreedyCooperativeSolver(bool[,,] whatToFill, bool[,,] state, IOracle oracle, ICandidatesOrdering candidatesOrdering = null)
        {
            this.whatToFill = whatToFill;
            this.state = state;
            this.oracle = oracle;
            this.candidatesOrdering = candidatesOrdering ?? new BuildAllStayingStill();
            R = whatToFill.GetLength(0);
        }

        public IEnumerable<ICommand> Solve()
        {
            for (int i = 0; i < N - 1; i++)
            {
                for (int k = 0; k < i; k++)
                    yield return new Wait();
                yield return new Fission(new NearDifference(new Vec(0, 0, 1)), N - i - 2);
            }
            pos = new Vec[N];
            regions = new Region[N];
            for (int i = 0; i < N; i++)
            {
                pos[i] = new Vec(0, 0, i);
                regions[i] = Region.ForShift(new Vec(i % Nx * BlockSizeX, 0, i / Nx * BlockSizeZ), new Vec(BlockSizeX - 1, R - 1, BlockSizeZ - 1));
            }

            for (int i = N - 1; i >= 0; i--)
            {
                var volatiles = pos.ToHashSet();
                var target = regions[i].Start;
                var commands = new PathFinder(state, pos[i], target, volatiles, null).TryFindPath();
                if (commands == null)
                    throw new InvalidOperationException($"Failed to find path from {pos[i]} to {target}");
                foreach (var command in commands)
                {
                    for (int k = 0; k < i; k++)
                        yield return new Wait();
                    yield return command;
                    for (int k = i + 1; k < N; k++)
                        yield return new Wait();
                }
                pos[i] = target;
            }

            var enumerators = new IEnumerator<ICommand>[N];
            while (true)
            {
                for (int bot = 0; bot < N; bot++)
                {
                    if (enumerators[bot] == null)
                        enumerators[bot] = SolveSingle(bot).GetEnumerator();
                }

                var tickCommands = new ICommand[N];
                for (int bot = 0; bot < N; bot++)
                {
                    if (enumerators[bot].MoveNext())
                        tickCommands[bot] = enumerators[bot].Current;
                    else
                    {
                        enumerators[bot].Dispose();
                        enumerators[bot] = null;
                    }
                }

                if (tickCommands.All(c => c == null))
                    break;

                foreach (var tickCommand in tickCommands)
                    yield return tickCommand ?? new Wait();
            }

            for (int bot = N - 1; bot >= 1; bot--)
            {
                var parentPos = pos[bot - 1];
                List<ICommand> commands = null;
                foreach (var near in parentPos.GetNears().OrderBy(n => n.MDistTo(pos[bot])))
                {
                    if (near.IsInCuboid(R) && !state.Get(near))
                    {
                        var pathFinder = new PathFinder(state, pos[bot], near, null, null);
                        commands = pathFinder.TryFindPath();
                        if (commands != null)
                        {
                            pos[bot] = near;
                            break;
                        }
                    }
                }
                if (commands == null)
                    throw new InvalidOperationException($"Couldn't find path for fusion from {pos[bot]} to near of {parentPos}");
                foreach (var command in commands)
                {
                    for (int k = 0; k < bot; k++)
                        yield return new Wait();
                    yield return command;
                }
                for (int k = 0; k < bot - 1; k++)
                    yield return new Wait();
                yield return new FusionP(new NearDifference(pos[bot] - parentPos));
                yield return new FusionS(new NearDifference(parentPos - pos[bot]));
            }

            var cmds = new List<ICommand>();
            Move(0, Vec.Zero, cmds);
            foreach (var command in cmds)
                yield return command;
            yield return new Halt();
        }

        private IEnumerable<ICommand> SolveSingle(int bot)
        {
            var candidates = BuildCandidates(bot);
            
            while (candidates.Any())
            {
                var commands = new List<ICommand>();
                var candidatesAndPositions = OrderCandidates(bot, candidates);
                var any = false;
                foreach (var (candidate, nearPosition) in candidatesAndPositions)
                {
                    if (Move(bot, nearPosition, commands))
                    {
                        any = true;
                        Fill(bot, candidate, commands);
                        candidates.Remove(candidate);
                        foreach (var n in neighbors)
                        {
                            var neighbor = candidate + n;
                            if (neighbor.IsInCuboid(R)
                                && neighbor.IsInRegion(regions[bot])
                                && whatToFill.Get(neighbor)
                                && !state.Get(neighbor))
                                candidates.Add(neighbor);
                        }
                        break;
                    }
                }
                foreach (var command in commands)
                    yield return command;
                if (!any)
                    throw new Exception("Can't move");
            }
        }

        private int Nx { get; } = 4;
        private int Nz { get; } = 5;
        private int N => Nx * Nz;

        private int BlockSizeX => (R + Nx - 1) / Nx;
        private int BlockSizeZ => (R + Nz - 1) / Nz;

        private bool Move(int bot, Vec target, List<ICommand> commands)
        {
            var pathFinder = new PathFinder(state, pos[bot], target, null, regions[bot]);
            var path = pathFinder.TryFindPath();
            if (path == null) return false;
            commands.AddRange(path);
            pos[bot] = target;
            return true;
        }

        private void Fill(int bot, Vec target, List<ICommand> commands)
        {
            commands.Add(new Fill(new NearDifference(target - pos[bot])));
            state.Set(target, true);
            oracle.Fill(target);
        }

        private bool IsInBotRange(Vec vec, int bot)
        {
            return vec.IsInRegion(regions[bot]);
        }

        private IEnumerable<(Vec candidate, Vec nearPosition)> OrderCandidates(int bot, HashSet<Vec> candidates)
        {
            foreach (var candidate in candidatesOrdering.Order(candidates, pos[bot]))
            {
                var nearPositions = candidate.GetNears().Where(n => n.IsInCuboid(R) && IsInBotRange(n, bot));
                foreach (var nearPosition in nearPositions.OrderBy(p => p.MDistTo(pos[bot])))
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

        private HashSet<Vec> BuildCandidates(int bot)
        {
            var result = new HashSet<Vec>();
            for (int x = 0; x < R; x++)
                for (int y = 0; y < R; y++)
                    for (int z = 0; z < R; z++)
                    {
                        var vec = new Vec(x, y, z);
                        if (!IsInBotRange(vec, bot))
                            continue;
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