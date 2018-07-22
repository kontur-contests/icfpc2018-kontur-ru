using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Strategies
{
    public class FastDeconstructor : IAmSolver
    {
        private readonly Matrix targetMatrix;
        private int N => targetMatrix.R;
        private Grid grid;
        private CorrectComponentTrackingMatrix buildingMatrix;
        private int crewXCount;
        private int crewZCount;
        private int crewHeight;
        private int halfCrewSize => crewZCount * crewXCount;
        private int crewDistance;
        private int maxSpeed = 15;

        public FastDeconstructor(Matrix targetMatrix)
        {
            this.targetMatrix = targetMatrix;
            crewDistance = Math.Min(29, N - 3);
            crewXCount = Math.Max(2, Math.Min(5, (N - 1) / crewDistance + 1));
            crewZCount = Math.Max(2, Math.Min(4, (N - 1) / crewDistance + 1));
            crewDistance = Math.Min(29, Math.Min((N - 3) / (crewXCount - 1), (N - 3) / (crewZCount - 1)));
            crewHeight = Math.Min(29, N - 1);
        }

        public IEnumerable<ICommand> Solve()
        {
            buildingMatrix = new CorrectComponentTrackingMatrix(new bool[N, N, N]);
            foreach (var command in GoToVerticalLast(new Vec(0, 0, 0), new Vec(0, N - 1, 0), maxSpeed))
                yield return command;
            var (firstCrewFormationCommands, firstBotPositions) = FormHalfCrew(0);
            foreach (var command in firstCrewFormationCommands)
                yield return command;
            foreach (var command in ApplyForHalfCrew(-1, new SMove(new LongLinearDifference(new Vec(1, 0, 0))),
                                                     new SMove(new LongLinearDifference(new Vec(0, 0, 1)))))
                yield return command;
            foreach (var command in ApplyForBot(halfCrewSize, 0, new Fission(new NearDifference(new Vec(-1, 0, -1)), halfCrewSize - 1)))
                yield return command;
            var (secondCrewFormationCommands, secondBotPositions) = FormHalfCrew(halfCrewSize);
            foreach (var command in secondCrewFormationCommands)
                yield return command;

            var crewWidth = crewDistance * (crewXCount - 1);
            var crewDepth = crewDistance * (crewZCount - 1);
            var topY = N - 1;
            var rightX = 1 + crewWidth;
            var farZ = 1 + crewDepth;
            while (true)
            {
                for (var i = 0; i < N - 1; i++)
                    foreach (var command in GoDownWithVoid(0))
                        yield return command;
                foreach (var command in GoToVerticalFirstForHalfCrew(1, new Vec(1, 0, 1), maxSpeed))
                    yield return command;
                foreach (var command in GoToVerticalFirstForHalfCrew(1, new Vec(0, -(N - crewHeight), 0), maxSpeed))
                    yield return command;
                topY -= N - crewHeight;

                foreach (var command in BuildWalls((a, b) => new GFill(a, b)))
                    yield return command;
                while (topY != N - 2)
                {
                    var jumpDistance = Math.Min(crewHeight - 2, N - 2 - topY);
                    foreach (var command in GoToVerticalFirstForFullCrew(new Vec(0, jumpDistance, 0), maxSpeed))
                        yield return command;
                    foreach (var command in BuildWalls((a, b) => new GFill(a, b)))
                        yield return command;
                    topY += jumpDistance;
                }
                while (true)
                {
                    var jumpDistance = Math.Min(crewHeight - 2, topY - crewHeight + 1);
                    foreach (var command in VoidCube())
                        yield return command;
                    if (topY == crewHeight - 1)
                        break;
                    foreach (var command in GoToVerticalFirstForFullCrew(new Vec(0, -jumpDistance, 0), maxSpeed))
                        yield return command;
                    topY -= jumpDistance;
                }

                foreach (var command in GoToVerticalFirstForFullCrew(new Vec(0, N - 1 - topY, 0), crewDistance - 2))
                    yield return command;
                topY = N - 1;

                foreach (var command in GoToVerticalFirstForHalfCrew(1, new Vec(-1, 0, -1), maxSpeed))
                    yield return command;
                foreach (var command in GoToVerticalFirstForHalfCrew(0, new Vec(0, crewHeight - 1, 0), maxSpeed))
                    yield return command;

                var shiftX = Math.Min(N - 2 - rightX, crewWidth);
                if (shiftX == 0)
                {
                    var shiftZ = Math.Min(N - 2 - farZ, crewDepth);

                    if (shiftZ == 0)
                        break;

                    foreach (var command in GoToVerticalFirstForFullCrew(new Vec(-(rightX - crewWidth - 1), 0, shiftZ), crewDistance - 2))
                        yield return command;
                    rightX = 1 + crewWidth;
                    farZ += shiftZ;
                }
                else
                {
                    foreach (var command in GoToVerticalFirstForFullCrew(new Vec(shiftX, 0, 0), crewDistance - 2))
                        yield return command;
                    rightX += shiftX;
                }
            }

            foreach (var command in GoToVerticalFirstForFullCrew(new Vec(-(rightX - crewWidth - 1), 0, -(farZ - crewDepth - 1)), crewDistance - 2))
                yield return command;

            topY = N - 1;
            rightX = 1 + crewWidth;
            farZ = 1 + crewDepth;
            while (true)
            {
                for (var i = 0; i < N - 1; i++)
                    foreach (var command in GoDownWithVoid(0))
                        yield return command;
                foreach (var command in GoToVerticalFirstForHalfCrew(0, new Vec(0, N - crewHeight, 0), maxSpeed))
                    yield return command;

                foreach (var command in GoToVerticalFirstForHalfCrew(1, new Vec(1, 0, 1), maxSpeed))
                    yield return command;

                while (true)
                {
                    var jumpDistance = Math.Min(crewHeight - 2, topY - crewHeight + 1);
                    foreach (var command in BuildWalls((a, b) => new GVoid(a, b)))
                        yield return command;
                    if (topY == crewHeight - 1)
                        break;
                    foreach (var command in GoToVerticalFirstForFullCrew(new Vec(0, -jumpDistance, 0), maxSpeed))
                        yield return command;
                    topY -= jumpDistance;
                }

                foreach (var command in GoToVerticalFirstForFullCrew(new Vec(0, N - 1 - topY, 0), crewDistance - 2))
                    yield return command;
                topY = N - 1;

                foreach (var command in GoToVerticalFirstForHalfCrew(1, new Vec(-1, 0, -1), maxSpeed))
                    yield return command;
                foreach (var command in GoToVerticalFirstForHalfCrew(0, new Vec(0, crewHeight - 1, 0), maxSpeed))
                    yield return command;

                var shiftX = Math.Min(N - 2 - rightX, crewWidth);
                if (shiftX == 0)
                {
                    var shiftZ = Math.Min(N - 2 - farZ, crewDepth);

                    if (shiftZ == 0)
                        break;

                    foreach (var command in GoToVerticalFirstForFullCrew(new Vec(-(rightX - crewWidth - 1), 0, shiftZ), crewDistance - 2))
                        yield return command;
                    rightX = 1 + crewWidth;
                    farZ += shiftZ;
                }
                else
                {
                    foreach (var command in GoToVerticalFirstForFullCrew(new Vec(shiftX, 0, 0), crewDistance - 2))
                        yield return command;
                    rightX += shiftX;
                }
            }

            foreach (var command in GoToVerticalFirstForFullCrew(new Vec(-(rightX - crewWidth - 1), 0, -(farZ - crewDepth - 1)), crewDistance - 2))
                yield return command;

            foreach (var command in GoHome(firstBotPositions.Concat(secondBotPositions.Select(v => v + new Vec(-1, 0, -1))).ToList()))
                yield return command;
            foreach (var command in GoToVerticalFirst(Vec.Zero, new Vec(-1, 0, -1), maxSpeed))
                yield return command;
            yield return new Halt();
        }

        private int GetBotId(int x, int z, int crewId)
        {
            return (crewId == 0 ? 0 : halfCrewSize) + z * crewXCount + x;
        }

        private IEnumerable<ICommand> VoidCube()
        {
            var result = new List<ICommand>();
            var cx = crewDistance - 2;
            var cy = crewHeight - 1;
            var cz = crewDistance - 2;
            for (var x = 0; x < crewXCount - 1; x++)
                for (var z = 0; z < crewZCount - 1; z++)
                {
                    var toApply = Enumerable.Repeat<ICommand>(new Wait(), halfCrewSize * 2).ToArray();
                    toApply[GetBotId(x + 0, z + 0, 0)] = new GVoid(new NearDifference(new Vec(1, 0, 1)), new FarDifference(new Vec(cx, cy, cz)));
                    toApply[GetBotId(x + 1, z + 0, 0)] = new GVoid(new NearDifference(new Vec(-1, 0, 1)), new FarDifference(new Vec(-cx, cy, cz)));
                    toApply[GetBotId(x + 0, z + 0, 1)] = new GVoid(new NearDifference(new Vec(1, 0, 1)), new FarDifference(new Vec(cx, -cy, cz)));
                    toApply[GetBotId(x + 1, z + 0, 1)] = new GVoid(new NearDifference(new Vec(-1, 0, 1)), new FarDifference(new Vec(-cx, -cy, cz)));

                    toApply[GetBotId(x + 0, z + 1, 0)] = new GVoid(new NearDifference(new Vec(1, 0, -1)), new FarDifference(new Vec(cx, cy, -cz)));
                    toApply[GetBotId(x + 1, z + 1, 0)] = new GVoid(new NearDifference(new Vec(-1, 0, -1)), new FarDifference(new Vec(-cx, cy, -cz)));
                    toApply[GetBotId(x + 0, z + 1, 1)] = new GVoid(new NearDifference(new Vec(1, 0, -1)), new FarDifference(new Vec(cx, -cy, -cz)));
                    toApply[GetBotId(x + 1, z + 1, 1)] = new GVoid(new NearDifference(new Vec(-1, 0, -1)), new FarDifference(new Vec(-cx, -cy, -cz)));

                    result.AddRange(toApply);
                }
            return result;
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
                commands.AddRange(GoToVerticalLast(currentBot, new Vec(0, 0, zCoord), maxSpeed));
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

        private IEnumerable<ICommand> BuildWalls(Func<NearDifference, FarDifference, ICommand> commandCreator)
        {
            var result = new List<ICommand>();
            var cx = crewDistance - 2;
            var cy = crewHeight - 1;
            for (var x = 0; x < crewXCount - 1; x++)
                for (var z = 0; z < crewZCount - 1; z++)
                {
                    var toApply = Enumerable.Repeat<ICommand>(new Wait(), halfCrewSize * 2).ToArray();
                    toApply[GetBotId(x, z, 0)] = commandCreator(new NearDifference(new Vec(1, 0, 0)), new FarDifference(new Vec(cx, cy, 0)));
                    toApply[GetBotId(x + 1, z, 0)] = commandCreator(new NearDifference(new Vec(-1, 0, 0)), new FarDifference(new Vec(-cx, cy, 0)));
                    toApply[GetBotId(x, z, 1)] = commandCreator(new NearDifference(new Vec(1, 0, 0)), new FarDifference(new Vec(cx, -cy, 0)));
                    toApply[GetBotId(x + 1, z, 1)] = commandCreator(new NearDifference(new Vec(-1, 0, 0)), new FarDifference(new Vec(-cx, -cy, 0)));

                    toApply[GetBotId(x, z + 1, 0)] = commandCreator(new NearDifference(new Vec(1, 0, 0)), new FarDifference(new Vec(cx, cy, 0)));
                    toApply[GetBotId(x + 1, z + 1, 0)] = commandCreator(new NearDifference(new Vec(-1, 0, 0)), new FarDifference(new Vec(-cx, cy, 0)));
                    toApply[GetBotId(x, z + 1, 1)] = commandCreator(new NearDifference(new Vec(1, 0, 0)), new FarDifference(new Vec(cx, -cy, 0)));
                    toApply[GetBotId(x + 1, z + 1, 1)] = commandCreator(new NearDifference(new Vec(-1, 0, 0)), new FarDifference(new Vec(-cx, -cy, 0)));
                    result.AddRange(toApply);

                    toApply[GetBotId(x, z, 0)] = commandCreator(new NearDifference(new Vec(0, 0, 1)), new FarDifference(new Vec(0, cy, cx)));
                    toApply[GetBotId(x, z + 1, 0)] = commandCreator(new NearDifference(new Vec(0, 0, -1)), new FarDifference(new Vec(0, cy, -cx)));
                    toApply[GetBotId(x, z, 1)] = commandCreator(new NearDifference(new Vec(0, 0, 1)), new FarDifference(new Vec(0, -cy, cx)));
                    toApply[GetBotId(x, z + 1, 1)] = commandCreator(new NearDifference(new Vec(0, 0, -1)), new FarDifference(new Vec(0, -cy, -cx)));

                    toApply[GetBotId(x + 1, z, 0)] = commandCreator(new NearDifference(new Vec(0, 0, 1)), new FarDifference(new Vec(0, cy, cx)));
                    toApply[GetBotId(x + 1, z + 1, 0)] = commandCreator(new NearDifference(new Vec(0, 0, -1)), new FarDifference(new Vec(0, cy, -cx)));
                    toApply[GetBotId(x + 1, z, 1)] = commandCreator(new NearDifference(new Vec(0, 0, 1)), new FarDifference(new Vec(0, -cy, cx)));
                    toApply[GetBotId(x + 1, z + 1, 1)] = commandCreator(new NearDifference(new Vec(0, 0, -1)), new FarDifference(new Vec(0, -cy, -cx)));

                    result.AddRange(toApply);
                }

            return result;
        }

        private IEnumerable<ICommand> GoDownWithVoid(int crewId)
        {
            foreach (var command in ApplyForHalfCrew(crewId,
                                                     new Voidd(new NearDifference(new Vec(0, -1, 0))),
                                                     new SMove(new LongLinearDifference(new Vec(0, -1, 0)))))
                yield return command;
        }

        private IEnumerable<ICommand> GoToVerticalFirstForFullCrew(Vec direction, int stepLength)
        {
            foreach (var command in ApplyForFullCrew(GoToVerticalFirst(Vec.Zero, direction, stepLength).ToArray()))
                yield return command;
        }

        private IEnumerable<ICommand> GoToVerticalFirstForHalfCrew(int crewId, Vec direction, int stepLength)
        {
            foreach (var command in ApplyForHalfCrew(crewId, GoToVerticalFirst(Vec.Zero, direction, stepLength).ToArray()))
                yield return command;
        }

        private IEnumerable<ICommand> ApplyForFullCrew([NotNull] params ICommand[] commands)
        {
            foreach (var command in commands)
            {
                var actionCommands = Enumerable.Repeat(command, 2 * halfCrewSize).ToArray();
                foreach (var resultCommand in actionCommands)
                    yield return resultCommand;
            }
        }

        private IEnumerable<ICommand> ApplyForHalfCrew(int crewId, [NotNull] params ICommand[] commands)
        {
            foreach (var command in commands)
            {
                var waitCommands = Enumerable.Repeat(new Wait(), crewId == -1 ? 0 : halfCrewSize).ToArray();
                var actionCommands = Enumerable.Repeat(command, halfCrewSize).ToArray();
                foreach (var resultCommand in crewId == 0 ? actionCommands.Concat(waitCommands) : waitCommands.Concat(actionCommands))
                    yield return resultCommand;
            }
        }

        [NotNull]
        private IEnumerable<ICommand> ApplyForBot(int botCount, int botId, [NotNull] params ICommand[] commands)
        {
            var result = new List<ICommand>();
            foreach (var command in commands)
            {
                var toApply = Enumerable.Repeat<ICommand>(new Wait(), botCount).ToArray();
                toApply[botId] = command;
                result.AddRange(toApply);
            }
            return result;
        }

        private (List<ICommand> Commands, List<Vec> Bots) FormHalfCrew(int otherCrewSize)
        {
            var result = new List<ICommand>();

            IEnumerable<ICommand> LocalApplyForBot(int botCount, int botId, params ICommand[] commands) =>
                ApplyForBot(botCount + otherCrewSize, botId + otherCrewSize, commands);

            result.AddRange(LocalApplyForBot(1, 0, new Fission(new NearDifference(new Vec(1, 0, 0)), crewXCount - 2)));
            for (var z = 1; z < crewZCount; z++)
            {
                result.AddRange(LocalApplyForBot(2 * z, 2 * (z - 1), new Fission(new NearDifference(new Vec(0, 0, 1)), crewXCount * (crewZCount - z) - 1)));
                result.AddRange(LocalApplyForBot(2 * z + 1, 2 * z, GoToVerticalFirst(new Vec(0, N - 1, (z - 1) * crewDistance + 1), new Vec(0, N - 1, z * crewDistance), maxSpeed).ToArray()));
                result.AddRange(LocalApplyForBot(2 * z + 1, 2 * z, new Fission(new NearDifference(new Vec(1, 0, 0)), crewXCount - 2)));
            }

            for (var x = 1; x < crewXCount; x++)
            {
                var tailCommands = x == crewXCount - 1 ? new ICommand[0] : new[] {new Fission(new NearDifference(new Vec(1, 0, 0)), crewXCount - x - 2)};
                foreach (var moveCommand in GoToVerticalFirst(new Vec(0, 0, 0), new Vec(crewDistance - 1, 0, 0), maxSpeed).Concat(tailCommands))
                {
                    var toApply = Enumerable.Repeat<ICommand>(new Wait(), otherCrewSize + (x + 1) * crewZCount).ToArray();
                    for (var i = 0; i < crewZCount; i++)
                        toApply[otherCrewSize + (x + 1) * (i + 1) - 1] = moveCommand;
                    result.AddRange(toApply);
                }
            }

            var botPositions = new List<Vec>();
            for (var z = 0; z < crewZCount; z++)
                for (var x = 0; x < crewXCount; x++)
                    botPositions.Add(new Vec(x * crewDistance, N - 1, z * crewDistance));

            return (result, botPositions);
        }

        [NotNull]
        private List<ICommand> GoToVerticalFirst([NotNull] Vec pos, [NotNull] Vec target, int stepLength)
        {
            var result = new List<ICommand>();
            result.AddRange(StraightGoTo(pos, new Vec(pos.X, target.Y, pos.Z), stepLength));
            pos = new Vec(pos.X, target.Y, pos.Z);
            result.AddRange(StraightGoTo(pos, new Vec(target.X, target.Y, pos.Z), stepLength));
            pos = new Vec(target.X, target.Y, pos.Z);
            result.AddRange(StraightGoTo(pos, new Vec(target.X, target.Y, target.Z), stepLength));
            return result;
        }

        [NotNull]
        private List<ICommand> GoToVerticalLast([NotNull] Vec pos, [NotNull] Vec target, int stepLength)
        {
            var result = new List<ICommand>();
            result.AddRange(StraightGoTo(pos, new Vec(target.X, pos.Y, pos.Z), stepLength));
            pos = new Vec(target.X, pos.Y, pos.Z);
            result.AddRange(StraightGoTo(pos, new Vec(target.X, pos.Y, target.Z), stepLength));
            pos = new Vec(target.X, pos.Y, target.Z);
            result.AddRange(StraightGoTo(pos, new Vec(target.X, target.Y, target.Z), stepLength));
            return result;
        }

        [NotNull]
        private List<ICommand> StraightGoTo(Vec botPosition, Vec target, int stepLength)
        {
            stepLength = Math.Min(stepLength, maxSpeed);
            var res = new List<ICommand>();
            var step = (target - botPosition).Sign();
            var pos = botPosition;
            while (pos != target)
            {
                var delta = step * Math.Min(stepLength, (target - pos).MLen());
                pos += delta;
                res.Add(new SMove(new LongLinearDifference(delta)));
            }
            return res;
        }
    }
}