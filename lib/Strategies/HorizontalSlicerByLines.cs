using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Strategies.Features;
using lib.Utils;

using MoreLinq;

namespace lib.Strategies
{
    public class HorizontalSlicerByLines : IAmSolver
    {
        private readonly Matrix targetMatrix;
        private int N => targetMatrix.R;
        private Grid grid;
        private int[,,] groundDistance;
        private int minX, minZ, maxX, maxZ;
        private readonly bool useBoundingBox;
        private readonly bool fast;
        private CorrectComponentTrackingMatrix buildingMatrix;
        private int maxY;

        public HorizontalSlicerByLines(Matrix targetMatrix, int gridCountX, int gridCountZ, bool useBoundingBox, bool fast = false)
        {
            if (gridCountZ != 1)
                throw new Exception("Wrong CountZ");
            this.targetMatrix = targetMatrix;
            this.useBoundingBox = useBoundingBox;
            this.fast = fast;
            gridCountX = Math.Min(gridCountX, N / 2); // todo (mpivko, 23.07.2018): 
            CalcBoundingBox(gridCountX, gridCountZ);
            gridCountX = Math.Min(gridCountX, (maxX - minX + 1));
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
                            maxY = Math.Max(maxY, y);
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

        private int GetPartnerId(int id)
        {
            if (id < grid.CountX)
                return id + grid.CountX;
            return id - grid.CountX;
        }

        public IEnumerable<ICommand> Solve()
        {
            buildingMatrix = new CorrectComponentTrackingMatrix(new bool[N, N, N]);
            var (transformedTargetMatrix, stickPositions) = TransformMatrix(targetMatrix);
            var (cloneCommands, initialBots) = 
                fast 
                ? Clone2(new Grid(grid.CountX, 2, minX, minZ, maxX, maxZ))
                : Clone(2 * grid.CountX, new Grid(grid.CountX, 2, minX, minZ, maxX, maxZ));
            foreach (var command in cloneCommands)
                yield return command;

            var botQueues = new List<Queue<ICommand>>();
            for (var i = 0; i < initialBots.Count; i++)
                botQueues.Add(new Queue<ICommand>());

            var botsToGenerateCommands = initialBots.ToList();
            var botCount = botsToGenerateCommands.Count;
            for (var y = 0; y <= maxY; y++)
            {
                for (var botId = 0; botId < botCount / 2; botId++)
                {
                    var nearId = botId;
                    var farId = botId + botCount / 2;
                    foreach (var (nearCommand, farCommand) in FillLayer(transformedTargetMatrix, botsToGenerateCommands[nearId], botsToGenerateCommands[farId]))
                    {
                        var nearBeforeTransform = grid.GetCellId(botsToGenerateCommands[nearId]);
                        var farBeforeTransform = grid.GetCellId(botsToGenerateCommands[farId]);
                        botQueues[nearId].Enqueue(nearCommand);
                        botQueues[farId].Enqueue(farCommand);
                        if (nearCommand is SMove sMove)
                            botsToGenerateCommands[nearId] += sMove.Shift;
                        if (nearCommand is LMove lMove)
                            botsToGenerateCommands[nearId] += lMove.firstShift.Shift + lMove.secondShift.Shift;
                        if (farCommand is SMove sMove1)
                            botsToGenerateCommands[farId] += sMove1.Shift;
                        if (farCommand is LMove lMove1)
                            botsToGenerateCommands[farId] += lMove1.firstShift.Shift + lMove1.secondShift.Shift;
                        if (!nearBeforeTransform.Equals(grid.GetCellId(botsToGenerateCommands[nearId])))
                            throw new Exception("Wrong zone");
                        if (!farBeforeTransform.Equals(grid.GetCellId(botsToGenerateCommands[farId])))
                            throw new Exception("Wrong zone");
                    }
                }
            }
            for (var botId = 0; botId < botCount / 2; botId++)
            {
                foreach (var command in RemoveSticks(stickPositions, botsToGenerateCommands[botId]))
                {
                    botQueues[botId].Enqueue(command);
                    if (command is SMove sMove)
                        botsToGenerateCommands[botId] += sMove.Shift;
                    if (command is LMove lMove)
                        botsToGenerateCommands[botId] += lMove.firstShift.Shift + lMove.secondShift.Shift;
                }
            }

            var botsToEvaluate = initialBots.ToList();
            bool isHighEnergy = false;
            bool firstHigh = false;
            while (botQueues.Any(x => x.Count > 0))
            {
                var commands = new List<ICommand>();
                if (isHighEnergy && !firstHigh && !buildingMatrix.HasNonGroundedVoxels)
                {
                    commands = new ICommand[] {new Flip()}.Concat(Enumerable.Repeat<ICommand>(new Wait(), botsToEvaluate.Count - 1)).ToList();
                    isHighEnergy = false;
                    foreach (var command in commands)
                        yield return command;
                    continue;
                }
                bool[] shouldWait = new bool[botCount];
                firstHigh = false;

                for (var i = 0; i < botQueues.Count; i++)
                {
                    if (botQueues[i].Count == 0)
                    {
                        continue;
                    }
                    if (botQueues[i].Peek() is Fill fillCommand)
                    {
                        var fillPosition = botsToEvaluate[i] + fillCommand.Shift;
                        if (!CanFill(buildingMatrix.Voxels, fillPosition) && !isHighEnergy)
                        {
                            shouldWait[i] = true;
                            shouldWait[GetPartnerId(i)] = true;
                        }
                    }
                    else if (botQueues[i].Peek() is GFill gFillCommand)
                    {
                        var filledCells = new List<Vec>();
                        foreach (var cell in Cuboid.FromPoints(botsToEvaluate[i] + gFillCommand.NearShift, botsToEvaluate[i] + gFillCommand.NearShift + gFillCommand.FarShift).AllPoints())
                        {
                            if (!buildingMatrix[cell])
                                filledCells.Add(cell);
                            buildingMatrix[cell] = true;
                        }
                        if (buildingMatrix.HasNonGroundedVoxels && !isHighEnergy)
                        {
                            foreach (var filledCell in filledCells)
                            {
                                buildingMatrix[filledCell] = false;
                            }
                            shouldWait[i] = true;
                            shouldWait[GetPartnerId(i)] = true;
                        }
                    }
                    else if (botQueues[i].Peek() is Voidd voidCommand)
                    {
                        var voidPosition = botsToEvaluate[i] + voidCommand.Shift;

                        if (!buildingMatrix.CanVoidCell(voidPosition) && !isHighEnergy)
                        {
                            shouldWait[i] = true;
                            shouldWait[GetPartnerId(i)] = true;
                        }
                    }
                }

                for (var i = 0; i < botQueues.Count; i++)
                {
                    if (botQueues[i].Count == 0 || shouldWait[i])
                    {
                        commands.Add(new Wait());
                        continue;
                    }
                    if (botQueues[i].Peek() is Fill fillCommand)
                    {
                        var fillPosition = botsToEvaluate[i] + fillCommand.Shift;
                        buildingMatrix[fillPosition] = true;
                        commands.Add(botQueues[i].Dequeue());
                    }
                    else if (botQueues[i].Peek() is GFill gFillCommand)
                    {
                        foreach (var cell in Cuboid.FromPoints(botsToEvaluate[i] + gFillCommand.NearShift, botsToEvaluate[i] + gFillCommand.NearShift + gFillCommand.FarShift).AllPoints())
                        {
                            buildingMatrix[cell] = true;
                        }
                        commands.Add(botQueues[i].Dequeue());
                    }
                    else if (botQueues[i].Peek() is Voidd voidCommand)
                    {
                        var voidPosition = botsToEvaluate[i] + voidCommand.Shift;
                        buildingMatrix[voidPosition] = false;
                        commands.Add(botQueues[i].Dequeue());
                    }
                    else
                    {
                        commands.Add(botQueues[i].Dequeue());
                    }
                }
                if (commands.All(x => x is Wait))
                {
                    firstHigh = isHighEnergy = true;
                    commands[0] = new Flip();
                }
                for (var i = 0; i < commands.Count; i++)
                {
                    if (commands[i] is SMove sMove)
                        botsToEvaluate[i] += sMove.Shift;
                    if (commands[i] is LMove lMove)
                        botsToEvaluate[i] += lMove.firstShift.Shift + lMove.secondShift.Shift;
                    yield return commands[i];
                }
            }
            foreach (var command in fast ? GoHome2(botsToEvaluate) : GoHome(botsToEvaluate))
                yield return command;
        }

        private bool CanFill(bool[,,] buildingMatrix, [NotNull] Vec vec)
        {
            return vec.Y == 0 || vec.GetMNeighbours().Where(x => x.IsInCuboid(N)).Any(buildingMatrix.Get);
        }

        private List<(Vec Near, Vec Far)> GetLines(List<Vec> cells)
        {
            var result = new List<(int, int)>();
            var zs = cells.Select(x => x.Z).ToList();
            var lastStartIndex = 0;
            for (int i = 1; i < zs.Count; i++)
            {
                if (zs[i] != zs[i - 1] + 1 || i - lastStartIndex >= 30)
                {
                    result.Add((zs[lastStartIndex], zs[i - 1]));
                    lastStartIndex = i;
                }
            }
            if (result.Any(x => x.Item2 - x.Item1 + 1 > 30))
                throw new Exception("Too long line");
            result.Add((zs[lastStartIndex], zs.Last()));
            return result.Select(z => (new Vec(cells[0].X, cells[0].Y, z.Item1), new Vec(cells[0].X, cells[0].Y, z.Item2))).ToList();
        }

        private List<Vec> GetCellsOnLine((Vec form, Vec to) pos)
        {
            return new Cuboid(pos.form, pos.to).AllPoints().ToList();
        }

        private IEnumerable<(ICommand, ICommand)> FillLayer(Matrix matrix, [NotNull] Vec nearBotPosition, [NotNull] Vec farBotPosition)
        {
            var lines = grid.GetCells(new Vec(nearBotPosition.X, nearBotPosition.Y - 1, nearBotPosition.Z))
                            .Where(matrix.IsFilledVoxel)
                            .GroupBy(x => x.X)
                            .SelectMany(x => GetLines(x.ToList()))
                            .OrderBy(position => GetCellsOnLine(position).Min(cell => groundDistance.Get(cell)))
                            .ToList();
            foreach (var line in lines)
            {
                // todo (mpivko, 23.07.2018): order depends on direction
                var nearTarget = line.Near.WithY(nearBotPosition.Y);
                var farTarget = line.Far.WithY(farBotPosition.Y);

                if (line.Far == line.Near)
                {
                    if ((line.Far - nearBotPosition).MLen() > (line.Far - farBotPosition).MLen())
                    {
                        foreach (var command in GoToVerticalFirstXZ(farBotPosition, farTarget))
                        {
                            farBotPosition = farTarget;
                            yield return (new Wait(), command);
                        }
                        yield return (new Wait(), new Fill(new Vec(0, -1, 0)));
                    }
                    else
                    {
                        foreach (var command in GoToVerticalFirstXZ(nearBotPosition, nearTarget))
                        {
                            nearBotPosition = nearTarget;
                            yield return (command, new Wait());
                        }
                        yield return (new Fill(new Vec(0, -1, 0)), new Wait());
                    }
                }
                else
                {
                    foreach (var tuple in LocateCrew(nearBotPosition, farBotPosition, nearTarget, farTarget))
                    {
                        yield return tuple;
                    }
                    nearBotPosition = nearTarget;
                    farBotPosition = farTarget;
                    yield return (new GFill(new NearDifference(new Vec(0, -1, 0)), new FarDifference(line.Far - line.Near)),
                        new GFill(new NearDifference(new Vec(0, -1, 0)), new FarDifference(line.Near - line.Far)));
                }
            }
            if (nearBotPosition.Y != N - 1)
                yield return (new SMove(new LongLinearDifference(new Vec(0, 1, 0))), new SMove(new LongLinearDifference(new Vec(0, 1, 0))));
        }

        private IEnumerable<(ICommand, ICommand)> LocateCrew([NotNull] Vec nearBotPosition, [NotNull] Vec farBotPosition, [NotNull] Vec nearTarget, [NotNull] Vec farTarget)
        {
            var nearDirection = nearTarget - nearBotPosition;
            var farDirection = farTarget - farBotPosition;
            if (nearDirection.Sign().Z != farDirection.Sign().Z)
                return DoLocateCrew(nearBotPosition, farBotPosition, nearTarget, farTarget);
            if (nearDirection.Sign().Z == -1)
                return DoLocateCrew(nearBotPosition, farBotPosition, nearTarget, farTarget);
            return DoLocateCrew(farBotPosition, nearBotPosition, farTarget, nearTarget).Select(x => (x.Item2, x.Item1));
        }

        private List<(ICommand, ICommand)> DoLocateCrew([NotNull] Vec firstBotPosition, [NotNull] Vec secondBotPosition, [NotNull] Vec firstTarget, [NotNull] Vec secondTarget)
        {
            var res = new List<(ICommand, ICommand)>();
            foreach (var command in GoToVerticalFirstZX(firstBotPosition, firstTarget))
            {
                res.Add((command, new Wait()));
            }
            foreach (var command in GoToVerticalFirstZX(secondBotPosition, secondTarget))
            {
                res.Add((new Wait(), command));
            }
            return res;
        }

        private IEnumerable<ICommand> RemoveSticks(List<Vec> stickPositions, Vec botPosition)
        {
            var resultCommands = new List<ICommand>();
            var currentPosition = botPosition;
            foreach (var stickPosition in stickPositions.Where(stick => grid.GetCellId(stick).Equals(grid.GetCellId(botPosition))))
            {
                resultCommands.AddRange(GoToVerticalFirstXZ(currentPosition, stickPosition));
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

        private (List<ICommand> Commands, List<Vec> Bots) Clone(int desiredCount, Grid currGrid)
        {
            if (desiredCount % 2 != 0)
                throw new Exception("desiredCount must be even");
            if (currGrid.CountX * currGrid.CountZ < desiredCount)
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
                var cellStart = currGrid.GetCellStart((i % currGrid.CountX, i / currGrid.CountX));
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
            var first = true;
            Vec firstBot = null;

            foreach (var (currentBot, i) in bots.Select((x, i) => (x, i)).OrderBy(x => (x.x.X, x.x.Z)))
            {
                if (first)
                    firstBot = currentBot;
                var commands = new List<ICommand>();
                //if (currentBot.Y != N - 1)
                //    throw new Exception("Bot should be at the N - 1 y-coord");
                var zCoord = first ? 0 : 1;
                commands.AddRange(GoToVerticalLast(currentBot, new Vec(0, 0, zCoord)));
                foreach (var command in commands)
                {
                    var toApply = Enumerable.Repeat<ICommand>(new Wait(), bots.Count).ToArray();
                    toApply[bots.IndexOf(currentBot)] = command;
                    foreach (var currentTickCommand in toApply)
                        yield return currentTickCommand;
                }
                if (!first)
                {
                    var toApply = Enumerable.Repeat<ICommand>(new Wait(), bots.Count).ToArray();
                    toApply[bots.IndexOf(firstBot)] = new FusionP(new NearDifference(new Vec(0, 0, 1)));
                    toApply[bots.IndexOf(currentBot)] = new FusionS(new NearDifference(new Vec(0, 0, -1)));
                    bots.Remove(currentBot);
                    foreach (var currentTickCommand in toApply)
                        yield return currentTickCommand;
                }
                first = false;
            }
            yield return new Halt();
        }

        [NotNull]
        private List<ICommand> GoToVerticalFirstXZ([NotNull] Vec pos, [NotNull] Vec target)
        {
            var shift = target - pos;
            if (Math.Abs(shift.X) <= 5 && Math.Abs(shift.Z) <= 5 && shift.Y == 0 && shift.X != 0 && shift.Z != 0)
            {
                return new List<ICommand>
                    {
                        new LMove(new ShortLinearDifference(new Vec(shift.X, 0, 0)), new ShortLinearDifference(new Vec(0, 0, shift.Z)))
                    };
            }
            var result = new List<ICommand>();
            result.AddRange(StraightGoTo(pos, new Vec(pos.X, target.Y, pos.Z)));
            pos = new Vec(pos.X, target.Y, pos.Z);
            result.AddRange(StraightGoTo(pos, new Vec(target.X, target.Y, pos.Z)));
            pos = new Vec(target.X, target.Y, pos.Z);
            result.AddRange(StraightGoTo(pos, new Vec(target.X, target.Y, target.Z)));
            return result;
        }

        [NotNull]
        private List<ICommand> GoToVerticalFirstZX([NotNull] Vec pos, [NotNull] Vec target)
        {
            var shift = target - pos;
            if (Math.Abs(shift.X) <= 5 && Math.Abs(shift.Z) <= 5 && shift.Y == 0 && shift.X != 0 && shift.Z != 0)
            {
                return new List<ICommand>
                    {
                        new LMove(new ShortLinearDifference(new Vec(0, 0, shift.Z)), new ShortLinearDifference(new Vec(shift.X, 0, 0)))
                    };
            }
            var result = new List<ICommand>();
            result.AddRange(StraightGoTo(pos, new Vec(pos.X, target.Y, pos.Z)));
            pos = new Vec(pos.X, target.Y, pos.Z);
            result.AddRange(StraightGoTo(pos, new Vec(pos.X, target.Y, target.Z)));
            pos = new Vec(pos.X, target.Y, target.Z);
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

        private DeluxeState CreateState(List<Vec> bots)
        {
            var state = new DeluxeState(targetMatrix, targetMatrix);
            state.Bots.Clear();
            for (var i = 0; i < bots.Count; i++)
            {
                var bot = bots[i];
                state.Bots.Add(new Bot { Bid = i + 1, Position = bot, Seeds = new List<int>() });
            }
            return state;
        }
        private IEnumerable<ICommand> GoHome2([NotNull] List<Vec> bots)
        {
            var state = CreateState(bots);
            return new Finalize(state).Run(state);
        }
        private (List<ICommand> Commands, List<Vec> Bots) Clone2(Grid botsGrid)
        {
            var state = new DeluxeState(null, targetMatrix);
            var botsCount = botsGrid.CountX * botsGrid.CountZ;
            var split = new Split(state, state.Bots.Single(), botsCount);
            var commands = split.Run(state).ToList();
            var targets = botsGrid.AllCellsStarts().Select(xz => new Vec(xz.x, 1, xz.z)).ToArray();
            var bots = split.Bots.OrderBy(b => b.Bid).ToList();
            var spread = new SpreadToPositions(state, bots, targets.ToList());
            commands.AddRange(spread.Run(state));

            var badGroups = bots.GroupBy(b => botsGrid.GetCellId(b.Position)).Where(g => g.Count() > 1).ToList();
            if (badGroups.Any())
                throw new Exception($"Bad group {badGroups.First().Key}");
            return (commands, split.Bots.Select(b => b.Position).ToList());
        }

    }
}