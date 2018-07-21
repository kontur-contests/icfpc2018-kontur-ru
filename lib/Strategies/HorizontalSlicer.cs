using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Utils;

using MoreLinq;

namespace lib.Strategies
{
    public class Grid
    {
        public int SizeX { get; }
        public int CountX { get; }
        public int SizeZ { get; }
        public int CountZ { get; }

        public Grid(int sizeX, int countX, int sizeZ, int countZ)
        {
            SizeX = sizeX;
            CountX = countX;
            SizeZ = sizeZ;
            CountZ = countZ;
        }

        [NotNull]
        public List<Vec> GetCells([NotNull] Vec botPosition, int n)
        {
            var result = new List<Vec>();
            var (blockX, blockZ) = GetCellId(botPosition);
            for (var x = blockX * SizeX; x < Math.Min(n, (blockX + 1) * SizeX); x++)
                for (var z = blockZ * SizeZ; z < Math.Min(n, (blockZ + 1) * SizeZ); z++)
                    result.Add(new Vec(x, botPosition.Y, z));
            return result;
        }

        public (int, int) GetCellId(Vec position)
        {
            return (position.X / SizeX, position.Z / SizeZ);
        }
    }

    public class HorizontalSlicer : IAmSolver
    {
        private readonly Matrix targetMatrix;
        private int N => targetMatrix.R;
        private Grid grid;
        private int[,,] groundDistance;

        public HorizontalSlicer(Matrix targetMatrix)
        {
            this.targetMatrix = targetMatrix;
            grid = new Grid((N + 5) / 6, 6, (N + 5) / 6, 6);
        }

        public IEnumerable<ICommand> Solve()
        {
            var buildingMatrix = new bool[N, N, N];
            var (transformedTargetMatrix, stickPositions) = TransformMatrix(targetMatrix);
            var (cloneCommands, initialBots) = Clone(grid.CountX * grid.CountZ); // todo (sivukhin, 22.07.2018): Constant
            foreach (var command in cloneCommands)
                yield return command;

            var botQueues = new List<Queue<ICommand>>();
            for (var i = 0; i < initialBots.Count; i++)
                botQueues.Add(new Queue<ICommand>());

            var botsToGenerateCommands = initialBots.ToList();
            for (var y = 0; y < N - 1; y++)
            {
                for (var botId = 0; botId < botsToGenerateCommands.Count; botId++)
                {
                    foreach (var command in FillLayer(transformedTargetMatrix, botsToGenerateCommands[botId]))
                    {
                        var beforeTransform = grid.GetCellId(botsToGenerateCommands[botId]);
                        botQueues[botId].Enqueue(command);
                        if (command is SMove sMove)
                            botsToGenerateCommands[botId] += sMove.Shift;
                        if (!beforeTransform.Equals(grid.GetCellId(botsToGenerateCommands[botId])))
                            throw new Exception("Wrong zone");
                    }
                }
            }

            var botsToEvaluate = initialBots.ToList();
            while (botQueues.Any(x => x.Count > 0))
            {
                var commands = new List<ICommand>();
                for (int i = 0; i < botQueues.Count; i++)
                {
                    if (botQueues[i].Count == 0)
                    {
                        commands.Add(new Wait());
                        continue;
                    }
                    if (botQueues[i].Peek() is Fill fillCommand)
                    {
                        if (CanFill(buildingMatrix, botsToEvaluate[i] + fillCommand.Shift))
                        {
                            commands.Add(botQueues[i].Dequeue());
                        }
                        else
                        {
                            commands.Add(new Wait());
                        }
                    }
                    else
                    {
                        commands.Add(botQueues[i].Dequeue());
                    }
                }
                if (commands.All(x => x is Wait))
                    throw new Exception("commands.All(x => x is Wait) == true");
                for (var i = 0; i < commands.Count; i++)
                {
                    if (commands[i] is SMove sMove)
                        botsToEvaluate[i] += sMove.Shift;
                    yield return commands[i];
                }
            }
            foreach (var command in RemoveSticks(stickPositions))
                yield return command;
            foreach (var command in GoHome(botsToEvaluate))
                yield return command;
            yield return new Halt();
        }

        private bool CanFill(bool[,,] buildingMatrix, [NotNull] Vec vec)
        {
            return vec.Y == 0 || vec.GetMNeighbours().Where(x => x.IsInCuboid(N)).Any(buildingMatrix.Get);
        }

        private IEnumerable<ICommand> FillLayer(Matrix matrix, Vec botPosition)
        {
            var gridCells = grid.GetCells(botPosition, N)
                                .Where(matrix.IsFilledVoxel)
                                .OrderBy(position => groundDistance.Get(position))
                                .ToList();
            var current = botPosition;
            foreach (var cell in gridCells)
            {
                var commandsToGo = GoToVerticalFirst(current, cell);
                current = cell;
                foreach (var command in commandsToGo)
                    yield return command;
                yield return new Fill(new NearDifference(new Vec(0, -1, 0)));
            }
            yield return new SMove(new LongLinearDifference(new Vec(0, 1, 0)));
        }

        private IEnumerable<ICommand> RemoveSticks(List<Vec> stickPositions)
        {
            return new ICommand[0];
        }

        private (Matrix Matrix, List<Vec> StickPositions) TransformMatrix([NotNull] Matrix matrix)
        {
            groundDistance = new int[N, N, N];
            var stickPositions = new List<Vec>();
            var transformedMatrix = matrix.Clone();
            for (var y = 1; y < N; y++)
            {
                var used = new bool[N, N];
                for (var x = 0; x < N; x++)
                    for (var z = 0; z < N; z++)
                    {
                        if (transformedMatrix[x, y, z] && !used[x, z])
                        {
                            if (!IsGrounded(x, y, z, used, transformedMatrix))
                            {
                                // todo (sivukhin, 21.07.2018): Добавить палку в место получше
                                stickPositions.Add(new Vec(x, N - 1, z));
                                for (var i = 0; i < N; i++)
                                    transformedMatrix[x, i, z] = true;
                            }
                        }
                    }
            }
            return (transformedMatrix, stickPositions.DistinctBy(x => x).ToList());
        }

        private bool IsGrounded(int x, int y, int z, [NotNull] bool[,] used, [NotNull] Matrix matrix)
        {
            var queue = new Queue<Vec>(new[] {new Vec(x, y, z)});
            used[x, z] = true;
            var isGrounded = false;
            while (queue.Count > 0)
            {
                var position = queue.Dequeue();
                isGrounded |= matrix[position.X, position.Y - 1, position.Z];
                foreach (var newPosition in position.GetMNeighbours().Where(p => p.Y == position.Y).Where(matrix.IsInside))
                {
                    if (matrix.IsFilledVoxel(newPosition) && !used[newPosition.X, newPosition.Z])
                    {
                        groundDistance.Set(newPosition, groundDistance.Get(position) + 1);
                        used[newPosition.X, newPosition.Z] = true;
                        queue.Enqueue(newPosition);
                    }
                }
            }
            return isGrounded;
        }

        private (List<ICommand> Commands, List<Vec> Bots) Clone(int count)
        {
            var result = new List<ICommand>();
            var botPositions = new List<Vec>();
            for (int i = 0; i < count - 1; i++)
            {
                var currentTickCommands = Enumerable.Repeat<ICommand>(new Wait(), i)
                                                    .Concat(new[] {new Fission(new NearDifference(new Vec(0, 0, 1)), count - i - 2)})
                                                    .ToArray();
                result.AddRange(currentTickCommands);
            }
            for (int i = count - 1; i >= 0; i--)
            {
                // todo (mpivko, 21.07.2018): Be careful with your brain
                var botFinalPosition = new Vec(i % grid.CountX * grid.SizeX, 0, i / grid.CountX * grid.SizeZ);
                botPositions.Add(botFinalPosition);
                var commands = GoToVerticalFirst(new Vec(0, 0, i), botFinalPosition);
                foreach (var command in commands)
                {
                    var currentTickCommands = Enumerable.Repeat<ICommand>(new Wait(), count).ToArray();
                    currentTickCommands[i] = command;
                    result.AddRange(currentTickCommands);
                }
            }
            return (result, botPositions);
        }

        private IEnumerable<ICommand> GoHome([NotNull] List<Vec> bots)
        {
            for (int i = 0; i < bots.Count; i++)
            {
                var currentBot = bots[i];
                var commands = new List<ICommand>();
                if (currentBot.Y != N - 1)
                    throw new Exception("Bot should be at the N - 1 y-coord");
                commands.AddRange(GoToVerticalLast(currentBot, new Vec(0, 0, i)));
                foreach (var command in commands)
                {
                    var toApply = Enumerable.Repeat<ICommand>(new Wait(), N).ToArray();
                    toApply[i] = command;
                    foreach (var currentTickCommand in toApply)
                        yield return currentTickCommand;
                }
            }
            for (int i = bots.Count - 1; i > 0; i--)
            {
                var toApply = Enumerable.Repeat<ICommand>(new Wait(), i - 1)
                                        .Concat(new ICommand[] {new FusionP(new NearDifference(new Vec(0, 0, 1))), new FusionS(new NearDifference(new Vec(0, 0, -1)))})
                                        .ToArray();
                foreach (var currentTickCommand in toApply)
                    yield return currentTickCommand;
            }
        }

        [NotNull]
        private List<ICommand> GoToVerticalFirst([NotNull] Vec pos, [NotNull] Vec target)
        {
            var result = new List<ICommand>();
            result.AddRange(StraightGoTo(pos, new Vec(pos.X, target.Y, pos.Z)));
            pos = new Vec(pos.X, target.Y, pos.Z);
            result.AddRange(StraightGoTo(pos, new Vec(target.X, target.Y, pos.Z)));
            pos = new Vec(target.X, target.Y, pos.Z);
            result.AddRange(StraightGoTo(pos, new Vec(target.X, target.Y, target.Z)));
            return result;
        }

        [NotNull]
        private List<ICommand> GoToVerticalLast([NotNull] Vec pos, [NotNull] Vec target)
        {
            var result = new List<ICommand>();
            result.AddRange(StraightGoTo(pos, new Vec(target.X, pos.Y, pos.Z)));
            pos = new Vec(target.X, pos.Y, pos.Z);
            result.AddRange(StraightGoTo(pos, new Vec(target.X, pos.Y, target.Z)));
            pos = new Vec(target.X, pos.Y, target.Z);
            result.AddRange(StraightGoTo(pos, new Vec(target.X, target.Y, target.Z)));
            return result;
        }

        [NotNull]
        private List<ICommand> StraightGoTo(Vec botPosition, Vec target)
        {
            var res = new List<ICommand>();
            var step = (target - botPosition).Sign();
            var pos = botPosition;
            while (pos != target)
            {
                var delta = step * Math.Min(15, (target - pos).MLen());
                pos += delta;
                res.Add(new SMove(new LongLinearDifference(delta)));
            }
            return res;
        }
    }
}