using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        public int MinX { get; }
        public int MinZ { get; }
        public int XMatrixSize { get; }
        public int ZMatrixSize { get; }
        public int CountLargeX { get; }
        public int CountLargeZ { get; }

        public Grid(int countX, int countZ, int minX, int minZ, int maxX, int maxZ)
        {
            XMatrixSize = maxX - minX + 1;
            ZMatrixSize = maxZ - minZ + 1;

            SizeX = XMatrixSize / countX;
            CountX = countX;
            SizeZ = ZMatrixSize / countZ;
            CountZ = countZ;
            MinX = minX;
            MinZ = minZ;
            if (CountX > XMatrixSize)
                throw new Exception("CountX > XMatrixSize");
            if (CountZ > ZMatrixSize)
                throw new Exception("CountZ > ZMatrixSize");
            if (SizeX * CountX > XMatrixSize)
                throw new Exception("X range grid overflow");
            if (SizeZ * CountZ > ZMatrixSize)
                throw new Exception("Z range grid overflow");
            CountLargeX = XMatrixSize - SizeX * CountX;
            CountLargeZ = ZMatrixSize - SizeZ * CountZ;
        }

        [NotNull]
        public List<Vec> GetCells([NotNull] Vec botPosition)
        {
            var result = new List<Vec>();
            var (blockX, blockZ) = GetCellId(botPosition);
            var (startX, startZ) = GetCellStart((blockX, blockZ));
            var (currSizeX, currSizeZ) = GetCellSize((blockX, blockZ));
            for (var x = startX; x < Math.Min(MinX + XMatrixSize, currSizeX + startX); x++)
                for (var z = startZ; z < Math.Min(MinZ + ZMatrixSize, currSizeZ + startZ); z++)
                    result.Add(new Vec(x, botPosition.Y, z));
            return result;
        }

        public (int, int) GetCellId(Vec position)
        {
            if (position.X < MinX || position.Z < MinZ || position.X >= MinX + XMatrixSize || position.Z > MinZ + ZMatrixSize)
                throw new Exception("Try to GetCellId for position out of bounding box");
            return (CalculateCoord(position.X - MinX, SizeX, CountLargeX),
                CalculateCoord(position.Z - MinZ, SizeZ, CountLargeZ));
        }

        private int CalculateCoord(int coord, int rangeSize, int countLarge)
        {
            if (coord < (rangeSize + 1) * countLarge)
                return coord / (rangeSize + 1);
            return countLarge + (coord - (rangeSize + 1) * countLarge) / rangeSize;
        }

        private (int, int) GetCellSize((int X, int Z) cellId)
        {
            return (cellId.X < CountLargeX ? SizeX + 1 : SizeX,
                cellId.Z < CountLargeZ ? SizeZ + 1 : SizeZ);
        }

        public (int X, int Z) GetCellStart((int X, int Z) cellId)
        {
            return (cellId.X * SizeX + Math.Min(cellId.X, CountLargeX) + MinX,
                cellId.Z * SizeZ + Math.Min(cellId.Z, CountLargeZ) + MinZ);
        }
    }

    public class HorizontalSlicer : IAmSolver
    {
        private readonly Matrix targetMatrix;
        private int N => targetMatrix.R;
        private Grid grid;
        private int[,,] groundDistance;
        private int minX, minZ, maxX, maxZ;
        private readonly bool useBoundingBox;
        private bool[,,] buildingMatrix;

        public HorizontalSlicer(Matrix targetMatrix, int gridCountX, int gridCountZ, bool useBoundingBox)
        {
            this.targetMatrix = targetMatrix;
            this.useBoundingBox = useBoundingBox;
            CalcBoundingBox(gridCountX, gridCountZ);
            grid = new Grid(gridCountX, gridCountZ, minX, minZ, maxX, maxZ);
        }

        private void CalcBoundingBox(int gridCountX, int gridCountZ)
        {
            minX = minZ = 100000;
            for (int x = 0; x < targetMatrix.R; x++)
            {
                for (int y = 0; y < targetMatrix.R; y++)
                {
                    for (int z = 0; z < targetMatrix.R; z++)
                    {
                        if (targetMatrix[x, y, z])
                        {
                            minX = Math.Min(minX, x);
                            minZ = Math.Min(minZ, z);
                            maxX = Math.Max(maxX, x);
                            maxZ = Math.Max(maxZ, z);
                        }
                    }
                }
            }
            if (!useBoundingBox)
            {
                minX = 0;
                minZ = 0;
                maxX = targetMatrix.R - 1;
                maxZ = targetMatrix.R - 1;
            }

            var xOverflow = gridCountX - (maxX - minX + 1);
            var zOverflow = gridCountZ - (maxZ - minZ + 1);
            if (xOverflow > 0 || zOverflow > 0)
            {
                maxX += Math.Max(0, xOverflow);
                maxZ += Math.Max(0, zOverflow);
                Log.For(this).Info($"Bounding box was enlarged due to overflow");
                if (maxX >= N || maxZ >= N)
                    throw new Exception("Too large bounding box after enlaging");
            }
        }

        public IEnumerable<ICommand> Solve()
        {
            buildingMatrix = new bool[N, N, N];
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
            for (var botId = 0; botId < botsToGenerateCommands.Count; botId++)
            {
                foreach (var command in RemoveSticks(stickPositions, botsToGenerateCommands[botId]))
                {
                    botQueues[botId].Enqueue(command);
                    if (command is SMove sMove)
                        botsToGenerateCommands[botId] += sMove.Shift;
                }
            }

            var botsToEvaluate = initialBots.ToList();
            while (botQueues.Any(x => x.Count > 0))
            {
                var commands = new List<ICommand>();
                for (var i = 0; i < botQueues.Count; i++)
                {
                    if (botQueues[i].Count == 0)
                    {
                        commands.Add(new Wait());
                        continue;
                    }
                    if (botQueues[i].Peek() is Fill fillCommand)
                    {
                        var fillPosition = botsToEvaluate[i] + fillCommand.Shift;
                        if (CanFill(buildingMatrix, fillPosition))
                        {
                            buildingMatrix.Set(fillPosition, true);
                            commands.Add(botQueues[i].Dequeue());
                        }
                        else
                            commands.Add(new Wait());
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
            var gridCells = grid.GetCells(new Vec(botPosition.X, botPosition.Y - 1, botPosition.Z))
                                .Where(matrix.IsFilledVoxel)
                                .OrderBy(position => groundDistance.Get(position))
                                .ToList();
            var current = botPosition;
            foreach (var cell in gridCells)
            {
                var botCell = new Vec(cell.X, cell.Y + 1, cell.Z);
                var commandsToGo = GoToVerticalFirst(current, botCell);
                current = botCell;
                foreach (var command in commandsToGo)
                    yield return command;
                yield return new Fill(new NearDifference(new Vec(0, -1, 0)));
            }
            if (botPosition.Y != N - 1)
                yield return new SMove(new LongLinearDifference(new Vec(0, 1, 0)));
        }

        private IEnumerable<ICommand> RemoveSticks(List<Vec> stickPositions, Vec botPosition)
        {
            var resultCommands = new List<ICommand>();
            var currentPosition = botPosition;
            foreach (var stickPosition in stickPositions.Where(stick => grid.GetCellId(stick).Equals(grid.GetCellId(botPosition))))
            {
                resultCommands.AddRange(GoToVerticalFirst(currentPosition, stickPosition));
                currentPosition = stickPosition;
                for (var y = N - 1; y > 0; y--)
                {
                    resultCommands.Add(new Voidd(new NearDifference(new Vec(0, -1, 0))));
                    if (y > 1)
                        resultCommands.Add(new SMove(new LongLinearDifference(new Vec(0, -1, 0))));
                }
                for (var y = 1; y < N; y++)
                {
                    if (targetMatrix[currentPosition.X, y - 1, currentPosition.Z])
                        resultCommands.Add(new Fill(new NearDifference(new Vec(0, -1, 0))));
                    if (y < N - 1)
                        resultCommands.Add(new SMove(new LongLinearDifference(new Vec(0, 1, 0))));
                }
            }
            return resultCommands;
        }

        private (Matrix Matrix, List<Vec> StickPositions) TransformMatrix([NotNull] Matrix matrix)
        {
            groundDistance = new int[N, N, N];
            var stickPositions = new List<Vec>();
            var transformedMatrix = matrix.Clone();
            for (var y = 1; y < N; y++)
            {
                var used = new bool[N, N];
                var usedGrounded = new bool[N, N];
                for (var x = 0; x < N; x++)
                    for (var z = 0; z < N; z++)
                    {
                        if (transformedMatrix[x, y, z] && !used[x, z])
                        {
                            var visitedCells = new List<Vec>();
                            if (!IsGrounded(x, y, z, used, transformedMatrix, visitedCells))
                            {
                                // todo (sivukhin, 21.07.2018): Добавить палку в место получше
                                stickPositions.Add(new Vec(x, N - 1, z));
                                for (var i = 0; i < N; i++)
                                    transformedMatrix[x, i, z] = true;
                            }
                            var queue = new Queue<Vec>();
                            foreach (var cell in visitedCells.Where(cell => transformedMatrix.IsFilledVoxel(new Vec(cell.X, cell.Y - 1, cell.Z))))
                            {
                                queue.Enqueue(cell);
                                usedGrounded[cell.X, cell.Z] = true;
                            }
                            while (queue.Count > 0)
                            {
                                var position = queue.Dequeue();
                                foreach (var newPosition in position.GetMNeighbours().Where(p => p.Y == position.Y).Where(transformedMatrix.IsInside))
                                    if (transformedMatrix.IsFilledVoxel(newPosition) && !usedGrounded[newPosition.X, newPosition.Z])
                                    {
                                        groundDistance.Set(newPosition, groundDistance.Get(position) + 1);
                                        usedGrounded[newPosition.X, newPosition.Z] = true;
                                        queue.Enqueue(newPosition);
                                    }
                            }
                        }
                    }
            }
            return (transformedMatrix, stickPositions.DistinctBy(x => x).ToList());
        }

        private bool IsGrounded(int x, int y, int z, [NotNull] bool[,] used, [NotNull] Matrix matrix, List<Vec> visitedCells)
        {
            var queue = new Queue<Vec>(new[] {new Vec(x, y, z)});
            used[x, z] = true;
            var isGrounded = false;
            while (queue.Count > 0)
            {
                var position = queue.Dequeue();
                visitedCells.Add(position);
                isGrounded |= matrix[position.X, position.Y - 1, position.Z];
                foreach (var newPosition in position.GetMNeighbours().Where(p => p.Y == position.Y).Where(matrix.IsInside))
                    if (matrix.IsFilledVoxel(newPosition) && !used[newPosition.X, newPosition.Z])
                    {
                        used[newPosition.X, newPosition.Z] = true;
                        queue.Enqueue(newPosition);
                    }
            }
            return isGrounded;
        }

        private (List<ICommand> Commands, List<Vec> Bots) Clone(int desiredCount)
        {
            if (desiredCount % 2 != 0)
                throw new Exception("desiredCount must be even");
            if (grid.CountX * grid.CountZ < desiredCount)
                throw new Exception("too small grid");
            var result = new List<ICommand>();
            var botPositions = new List<Vec>();
            result.Add(new Fission(new NearDifference(new Vec(0, 0, 1)), (desiredCount - 2) / 2 - 1));

            result.Add(new Fission(new NearDifference(new Vec(1, 0, 0)), (desiredCount - 2) / 2));
            result.Add(new Wait());

            result.Add(new Wait());
            result.Add(new Wait());
            result.Add(new Fission(new NearDifference(new Vec(0, 0, 1)), (desiredCount - 2) / 2 - 1));

            int rowCount = desiredCount / 2;
            for (var i = 1; i < rowCount - 1; i++) // todo (sivukhin, 22.07.2018): 
            {
                var currentTickCommands = Enumerable.Repeat<ICommand>(new Wait(), i)
                                                    .Concat(new[] {new Fission(new NearDifference(new Vec(0, 0, 1)), rowCount - i - 2)})
                                                    .ToArray();
                result.AddRange(currentTickCommands);
                result.AddRange(currentTickCommands);
            }

            for (var i = 0; i < desiredCount; i++)
            {
                result.Add(new SMove(new LongLinearDifference(new Vec(0, i % 10 + 4, 0))));
            }

            for (var i = desiredCount - 1; i >= 0; i--)
            {
                // todo (mpivko, 21.07.2018): Be careful with your brain
                int xCoord = i < rowCount ? 0 : 1;
                var cellStart = grid.GetCellStart((i % grid.CountX, i / grid.CountX));
                var botFinalPosition = new Vec(Math.Max(1, cellStart.X), 1, cellStart.Z);
                botPositions.Add(botFinalPosition);
                var commands = GoToVerticalLast(new Vec(xCoord, i % 10 + 4, i % rowCount),
                                                botFinalPosition);
                foreach (var command in commands)
                {
                    var currentTickCommands = Enumerable.Repeat<ICommand>(new Wait(), desiredCount).ToArray();
                    currentTickCommands[i] = command;
                    result.AddRange(currentTickCommands);
                }
            }
            botPositions.Reverse();
            return (result, botPositions);
        }

        private IEnumerable<ICommand> GoHome([NotNull] List<Vec> bots)
        {
            var remainBotsCount = bots.Count;
            for (var i = 0; i < bots.Count; i++)
            {
                var currentBot = bots[i];
                var commands = new List<ICommand>();
                if (currentBot.Y != N - 1)
                    throw new Exception("Bot should be at the N - 1 y-coord");
                var zCoord = i == 0 ? 0 : 1;
                commands.AddRange(GoToVerticalLast(currentBot, new Vec(0, 0, zCoord)));
                foreach (var command in commands)
                {
                    var toApply = Enumerable.Repeat<ICommand>(new Wait(), remainBotsCount).ToArray();
                    toApply[i == 0 ? 0 : 1] = command;
                    foreach (var currentTickCommand in toApply)
                        yield return currentTickCommand;
                }
                if (i != 0)
                {
                    var toApply = new ICommand[] {new FusionP(new NearDifference(new Vec(0, 0, 1))), new FusionS(new NearDifference(new Vec(0, 0, -1)))}
                        .Concat(Enumerable.Repeat<ICommand>(new Wait(), remainBotsCount - 2))
                        .ToArray();
                    foreach (var currentTickCommand in toApply)
                        yield return currentTickCommand;
                    remainBotsCount--;
                }
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