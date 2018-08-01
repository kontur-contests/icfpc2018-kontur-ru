using System;
using System.Collections.Generic;

using lib.Commands;
using lib.Models;
using lib.Primitives;

namespace lib.Utils
{
    public class DrillerPathFinder
    {
        private readonly bool[,,] state;
        private readonly Vec source;
        private readonly Vec target;
        private readonly Predicate<Vec> isAllowedPosition;
        private readonly int heuristicEfficiency;
        private readonly int R;

        public int TotalCells { get; private set; }
        public int ClosedCells { get; private set; }

        private static readonly Vec[] neighbors =
            {
                new Vec(1, 0, 0),
                new Vec(0, 1, 0),
                new Vec(0, 0, 1),
                new Vec(-1, 0, 0),
                new Vec(0, -1, 0),
                new Vec(0, 0, -1)
            };

        public DrillerPathFinder(bool[,,] state, Vec source, Vec target, Predicate<Vec> isAllowedPosition, int heuristicEfficiency = 0)
        {
            R = state.GetLength(0);
            this.state = state;
            this.source = source;
            this.target = target;
            this.isAllowedPosition = isAllowedPosition;
            this.heuristicEfficiency = heuristicEfficiency != 0 ? heuristicEfficiency : GetDefaultHeuristicEfficiency(R);
        }

        private static int GetDefaultHeuristicEfficiency(int r)
        {
            // max = 250 * 250 * 250 -> 6
            // min = 20 * 20 * 20 -> 1
            // count = r * r * r -> x
            
            var count = r * r * r;
            const int min = 20*20*20;
            const int max = 250*250*250;

            return 1 + (count - min) / (max - min) * (6 - 1);
        }

        public DrillerPathFinder(State state, Bot bot, Vec target, int heuristicEfficiency = 0)
            : this(state.Matrix.Voxels, bot.Position, target, vec => !state.IsVolatile(bot, vec), heuristicEfficiency)
        {
        }

        public List<Step> TryFindPath(bool startingOnDrilled)
        {
            if (source == target)
                return new List<Step>();

            if (!isAllowedPosition(target))
                return null;

            var closed = new HashSet<Vec>();
            var bests = new Dictionary<Vec, int>();
            var open = new PriorityQueue<Path>(Comparer<Path>.Create((a, b) =>
                {
                    var compare = a.Estimation.CompareTo(b.Estimation);
                    if (compare != 0)
                        return compare;
                    return a.Position.MDistTo(target).CompareTo(b.Position.MDistTo(target));
                }));
            open.Enqueue(new Path(null, source, target, startingOnDrilled ? StepType.Drill : StepType.Move, null, heuristicEfficiency));
            bests.Add(source, 0);
            while (open.Count > 0)
            {
                var current = open.Dequeue();

                if (current.Position == target)
                {
                    ClosedCells = closed.Count;
                    TotalCells = R * R * R;
                    var result = new List<Step>();
                    for (var p = current; p.Parent != null; p = p.Parent)
                        result.Add(new Step(p.StepType, p.Position, p.MoveCommand));
                    result.Reverse();
                    return result;
                }

                closed.Add(current.Position);

                switch (current.StepType)
                {
                    case StepType.DrillOut:
                    case StepType.Move:
                        foreach (var cmd in IteratePossibleCommands(current.Position))
                        {
                            if (closed.Contains(cmd.nextPosition))
                                continue;
                            if (!bests.TryGetValue(cmd.nextPosition, out var bestLength) || bestLength > current.Length + 1)
                            {
                                bests[cmd.nextPosition] = current.Length + 1;
                                open.Enqueue(new Path(current, cmd.nextPosition, target, StepType.Move, cmd.command, heuristicEfficiency));
                            }
                        }
                        foreach (var n in neighbors)
                        {
                            var next = current.Position + n;
                            if (closed.Contains(next))
                                continue;
                            if (next.IsInCuboid(R) && state.Get(next))
                            {
                                if (!bests.TryGetValue(next, out var bestLength) || bestLength > current.Length + 1)
                                {
                                    bests[next] = current.Length + 1;
                                    open.Enqueue(new Path(current, next, target, StepType.Drill, new SMove(n), heuristicEfficiency));
                                }
                            }
                        }
                        break;
                    case StepType.Drill:
                        foreach (var n in neighbors)
                        {
                            var next = current.Position + n;
                            if (closed.Contains(next))
                                continue;
                            if (next.IsInCuboid(R))
                            {
                                if (!bests.TryGetValue(next, out var bestLength) || bestLength > current.Length + 1)
                                {
                                    bests[next] = current.Length + 1;
                                    open.Enqueue(state.Get(next)
                                                     ? new Path(current, next, target, StepType.Drill, new SMove(n), heuristicEfficiency)
                                                     : new Path(current, next, target, StepType.DrillOut, new SMove(n), heuristicEfficiency));
                                }
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return null; // мы окружены, нет пути!
        }

        private IEnumerable<(ICommand command, Vec nextPosition)> IteratePossibleCommands(Vec current)
        {
            foreach (var n in neighbors)
            {
                var shift = Vec.Zero;
                for (int len = 1; len <= 15; len++)
                {
                    shift += n;
                    var res = current + shift;
                    if (!res.IsInCuboid(R) || !isAllowedPosition(res) || state.Get(res))
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
                    if (!fres.IsInCuboid(R) || !isAllowedPosition(fres) || state.Get(fres))
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
                                if (!res.IsInCuboid(R) || !isAllowedPosition(res) || state.Get(res))
                                    break;
                                yield return (new LMove(new ShortLinearDifference(fshift), new ShortLinearDifference(sshift)), res);
                            }
                        }
                    }
                }
            }
        }

        private class Path
        {
            public Path Parent { get; }
            public Vec Position { get; }
            public StepType StepType { get; }
            public ICommand MoveCommand { get; }
            public int Length { get; }
            public int Estimation { get; }

            public Path(Path parent, Vec position, Vec target, StepType stepType, ICommand moveCommand, int heuristicEfficiency)
            {
                Parent = parent;
                Position = position;
                StepType = stepType;
                MoveCommand = moveCommand;
                if (parent != null)
                {
                    Length = parent.Length + 1;
                    if (stepType == StepType.Drill)
                        Length++;

                    if (stepType == StepType.DrillOut && parent.StepType != StepType.Drill)
                        throw new InvalidOperationException($"Couldn't drill out while parent path didn't end with {StepType.Drill}, but ended with {parent.StepType}");

                    if (parent.StepType == StepType.Drill)
                    {
                        if (stepType == StepType.Move)
                            throw new InvalidOperationException($"Couldn't move while parent path ended with {StepType.Drill}");
                        Length++;
                    }
                }

                var heuristic = (position.MDistTo(target) + 14) * heuristicEfficiency / 15;

                Estimation = Length + heuristic;
            }

            public override string ToString()
            {
                var path = new List<string>();
                for (var current = this; current.Parent != null; current = current.Parent)
                    path.Add($"{current.MoveCommand}({current.StepType})");
                path.Reverse();
                return $"{nameof(Position)}: {Position}, {nameof(StepType)}: {StepType}, " +
                       $"{nameof(Length)}: {Length}, {nameof(Estimation)}: {Estimation}, " +
                       $"Path: {string.Join("->", path)}";
            }
        }

        public class Step
        {
            public Step(StepType type, Vec target, ICommand moveCommand)
            {
                Type = type;
                Target = target;
                MoveCommand = moveCommand;
            }

            public StepType Type { get; }
            public Vec Target { get; }
            public ICommand MoveCommand { get; }

            public override string ToString()
            {
                return $"{nameof(Type)}: {Type}, {nameof(Target)}: {Target}, {nameof(MoveCommand)}: {MoveCommand}";
            }
        }

        public enum StepType
        {
            Move,
            Drill,
            DrillOut
        }
    }
}