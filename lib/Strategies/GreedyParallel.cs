using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Strategies
{
    public class BotState
    {
        public Vec Position;
        public List<ICommand> Commands = new List<ICommand>();
    }

    public class GreedyParallel : IAmSolver
    {
        private readonly Matrix whatToFill;
        private readonly Matrix filled;
        private Dictionary<int, BotState> bots;
        private readonly IOracle oracle;
        private readonly int R;

        private readonly ICandidatesOrdering candidatesOrdering;

        public GreedyParallel(Matrix whatToFill, Vec pos, IOracle oracle, ICandidatesOrdering candidatesOrdering = null)
        {
            this.whatToFill = whatToFill;
            bots = new Dictionary<int, BotState>();
            bots[0] = new BotState {Position = pos};
            this.oracle = oracle;
            this.candidatesOrdering = candidatesOrdering ?? new BuildAllStayingStill();
            R = whatToFill.R;
            filled = new Matrix(R);
        }

        private List<ICommand> Commands { get; } = new List<ICommand>();

        public IEnumerable<ICommand> Solve()
        {
            var candidates = BuildCandidates();

            Commands.Add(new LMove(new ShortLinearDifference(new Vec(5, 0, 0)), 
                                   new ShortLinearDifference(new Vec(0, 5, 0))));
            Commands.Add(new Fission(new NearDifference(new Vec(0, 0, 1)), 10));

            while (true)
            {
                if (!candidates.Any())
                    break;

                foreach (var bot in bots)
                {
                    var candidatesAndPositions = OrderCandidates(bot.Key, candidates);

                    var any = false;
                    foreach (var (candidate, nearPosition) in candidatesAndPositions)
                    {
                        if (Move(bot.Key, nearPosition))
                        {
                            any = true;
                            Fill(bot.Key, candidate);
                            candidates.Remove(candidate);
                            foreach (var neighbor in candidate.GetMNeighbours())
                            {
                                if (neighbor.IsInCuboid(R)
                                    && whatToFill[neighbor]
                                    && !filled[neighbor])
                                    candidates.Add(neighbor);
                            }
                            break;
                        }
                    }
                }
            }
            
            //while (candidates.Any())
            //{
            //    var candidatesAndPositions = OrderCandidates(candidates);
            //    var any = false;
            //    foreach (var (candidate, nearPosition) in candidatesAndPositions)
            //    {
            //        if (Move(nearPosition))
            //        {
            //            any = true;
            //            Fill(candidate);
            //            candidates.Remove(candidate);
            //            foreach (var neighbor in candidate.GetMNeighbours())
            //            {
            //                if (neighbor.IsInCuboid(R)
            //                    && whatToFill[neighbor]
            //                    && !filled[neighbor])
            //                    candidates.Add(neighbor);
            //            }
            //            break;
            //        }
            //    }
            //    foreach (var command in Commands)
            //    {
            //        yield return command;
            //    }
            //    Commands.Clear();
            //    if (!any)
            //        throw new Exception("Can't move");
            //}
            
            //Move(Vec.Zero);

            Commands.Add(new Halt());
            Commands.Add(new Halt());

            foreach (var command in Commands)
            {
                yield return command;
            }
            Commands.Clear();
        }

        private bool Move(int index, Vec target)
        {
            var pathFinder = new PathFinder(filled.Voxels, bots[index].Position, target);
            var path = pathFinder.TryFindPath();
            if (path == null) return false;
            Commands.Add(path.First());
            bots[index].Position = target;
            return true;
        }

        private void Fill(int index, Vec target)
        {
            Commands.Add(new Fill(new NearDifference(target - bots[index].Position)));
            filled[target] = true;
            oracle.Fill(target);
        }

        private IEnumerable<(Vec candidate, Vec nearPosition)> OrderCandidates(int index, IEnumerable<Vec> candidates)
        {
            var pos = bots[index].Position;
            foreach (var candidate in candidatesOrdering.Order(candidates, pos))
            {
                var nearPositions = candidate.GetNears().Where(n => n.IsInCuboid(R));
                foreach (var nearPosition in nearPositions.OrderBy(p => p.MDistTo(pos)))
                {
                    if (oracle.CanFill(candidate, nearPosition))
                    {
                        yield return (candidate, nearPosition);
                    }
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
                        if (whatToFill[vec]
                            && !filled[vec]
                            && y == 0)
                        {
                            result.Add(vec);
                        }
                    }
            return result;
        }
    }
}