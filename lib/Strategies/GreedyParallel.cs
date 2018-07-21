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
        public int Tick = 1;
    }

    public class GreedyParallel : IAmSolver
    {
        private readonly Matrix whatToFill;
        private readonly Matrix<int> blockedBefore;
        private readonly Matrix filled;
        private Dictionary<int, BotState> bots;
        private readonly IOracle oracle;
        private readonly int R;
        private int Timer = 1;
        private SortedDictionary<int, List<Vec>> ToFill = new SortedDictionary<int, List<Vec>>();

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
            blockedBefore = new Matrix<int>(R);
        }


        public IEnumerable<ICommand> Solve()
        {
            var candidates = BuildCandidates();

            var commands = new List<ICommand>();

            commands.Add(new LMove(new ShortLinearDifference(new Vec(5, 0, 0)), 
                                   new ShortLinearDifference(new Vec(0, 5, 0))));
            commands.Add(new Fission(new NearDifference(new Vec(0, 0, 1)), 10));
            bots[0].Position = new Vec(5, 5, 0);
            bots.Add(10, new BotState {Position = new Vec(5, 5, 1)});

           

            while (true)
            {
                if (!candidates.Any())
                    break;

                foreach (var bot in bots.Values)
                    blockedBefore[bot.Position] = Timer;

                foreach (var bot in bots)
                {
                    if (!bot.Value.Commands.Any())
                    {
                        var candidatesAndPositions = OrderCandidates(bot.Key, candidates);

                        foreach (var (candidate, nearPosition) in candidatesAndPositions)
                        {
                            if (Move(bot.Key, nearPosition))
                            {
                                Fill(bot.Key, candidate);
                                candidates.Remove(candidate);
                                
                                break;
                            }
                        }
                    }

                    if (!bot.Value.Commands.Any())
                        bot.Value.Commands.Add(new Wait());

                    commands.Add(bot.Value.Commands.First());
                    bot.Value.Commands.RemoveAt(0);
                }

                Timer++;
                while (ToFill.Any() && ToFill.First().Key < Timer)
                {
                    foreach (var candidate in ToFill.First().Value)
                    {
                        foreach (var neighbor in candidate.GetMNeighbours())
                        {
                            if (neighbor.IsInCuboid(R)
                                && whatToFill[neighbor]
                                && !filled[neighbor])
                                candidates.Add(neighbor);
                        }
                    }

                    ToFill.Remove(ToFill.First().Key);
                }
            }

            commands.Add(new Halt());
            commands.Add(new Halt());

            foreach (var command in commands)
            {
                yield return command;
            }
            commands.Clear();
        }

        private bool Move(int index, Vec target)
        {
            var pathFinder = new PathFinder(filled.Voxels, bots[index].Position, target, null, null/*, blockedBefore, Timer*/);
            var path = pathFinder.TryFindPath();
            if (path == null) return false;

            foreach (var command in path)
            {
                AddCommand(bots[index], command);
            }

            return true;
        }

        private void Fill(int index, Vec target)
        {
            var tick = bots[index].Tick;
            if (!ToFill.ContainsKey(tick))
                ToFill[tick] = new List<Vec>();
            ToFill[tick].Add(target);

            AddCommand(bots[index], new Fill(new NearDifference(target - bots[index].Position)));
            filled[target] = true;
            oracle.Fill(target);
        }

        private IEnumerable<(Vec candidate, Vec nearPosition)> OrderCandidates(int index, HashSet<Vec> candidates)
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

        private void AddCommand(BotState bot, ICommand command)
        {
            bot.Tick++;

            if (command is LMove move1)
            {
                var cells = move1.GetCellsOnPath(bot.Position);
                foreach (var cell in cells)
                {
                    blockedBefore[cell] = bot.Tick;
                }

                bot.Position = cells.Last();
            } else if (command is SMove move2)
            {
                var cells = move2.GetCellsOnPath(bot.Position);
                foreach (var cell in cells)
                {
                    blockedBefore[cell] = bot.Tick;
                }

                bot.Position = cells.Last();
            }
            else if (command is Fill move3)
            {
                blockedBefore[bot.Position + move3.Shift] = bot.Tick;
            }
            else
            {
                throw new Exception("Unknown move type");
            }

            bot.Commands.Add(command);
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