using System;
using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Primitives;
using lib.Utils;

using MoreLinq.Extensions;

namespace lib.Models
{
    public class DeluxeState
    {
        public Matrix SourceMatrix { get; }
        public Matrix TargetMatrix { get; }
        public CorrectComponentTrackingMatrix Matrix { get; }
        public HashSet<Bot> Bots { get; }
        public long Energy { get; set; }
        public Harmonics Harmonics { get; set; }
        public Dictionary<Vec, (Bot bot, string message)> VolatileCells { get; } = new Dictionary<Vec, (Bot, string)>();
        public int Tick = 0;

        private readonly Dictionary<Bot, ICommand> botCommands = new Dictionary<Bot, ICommand>();
        private readonly Dictionary<Region, (bool isFill, Dictionary<Vec, Bot> corners)> groupRegions = new Dictionary<Region, (bool isFill, Dictionary<Vec, Bot> corners)>();

        public DeluxeState(Matrix sourceMatrix, Matrix targetMatrix)
        {
            SourceMatrix = sourceMatrix ?? new Matrix(targetMatrix.R);
            TargetMatrix = targetMatrix ?? new Matrix(sourceMatrix.R);
            Matrix = new CorrectComponentTrackingMatrix(SourceMatrix.Clone());
            Bots = new HashSet<Bot> { new Bot { Bid = 1, Position = Vec.Zero, Seeds = Enumerable.Range(2, 39).ToList() } };
            Energy = 0;
        }

        public void StartTick()
        {
            Energy += Harmonics == Harmonics.High
                          ? 30 * TargetMatrix.R * TargetMatrix.R * TargetMatrix.R
                          : 3 * TargetMatrix.R * TargetMatrix.R * TargetMatrix.R;
            Energy += 20 * Bots.Count;
            botCommands.Clear();
            VolatileCells.Clear();
            groupRegions.Clear();
            foreach (var bot in Bots)
                VolatileCells.Add(bot.Position, (bot, "Current bot position"));
        }

        public void SetBotCommand(Bot bot, ICommand command)
        {
            if (!Bots.Contains(bot))
                throw new InvalidOperationException($"Unknown bot {bot}; Command: {command}");
            if (botCommands.TryGetValue(bot, out var duplicateCommand))
                throw new InvalidOperationException($"Bot {bot} has duplicate commands {command} and {duplicateCommand}");
            botCommands.Add(bot, command);

            if (!command.AllPositionsAreValid(Matrix, bot))
                throw new InvalidOperationException($"Incorrect command {command}");

            if (command is GroupCommand groupCommand)
            {
                var region = groupCommand.GetRegion(bot.Position);
                Dictionary<Vec, Bot> corners;
                if (!groupRegions.TryGetValue(region, out var others))
                {
                    groupRegions.Add(region, (command is GFill, corners = new Dictionary<Vec, Bot>()));
                    AddVolatileCells(bot, command, region);
                }
                else
                {
                    corners = others.corners;
                    if (others.isFill && !(command is GFill))
                        throw new InvalidOperationException($"Common volatile region {region}. " +
                                                            $"Bots: {bot}; {string.Join("; ", corners.Values)}. " +
                                                            $"Command: {command};");
                }
                var corner = bot.Position + groupCommand.NearShift;
                if (corners.TryGetValue(corner, out var conflictingBot))
                    throw new InvalidOperationException($"Common group region cell {corner}. " +
                                                        $"Bots: {bot}; {conflictingBot}.");
                corners.Add(corner, bot);
            }

            AddVolatileCells(bot, command, command.GetVolatileCells(bot));
        }

        public List<ICommand> EndTick()
        {
            Tick++;

            foreach (var bot in Bots)
            {
                if (!botCommands.ContainsKey(bot))
                    SetBotCommand(bot, new Wait());
            }
            if (botCommands.All(x => x.Value is Wait))
                throw new InvalidOperationException("All commands are WAITS - it's wrong");

            foreach (var kvp in groupRegions)
            {
                var region = kvp.Key;
                var cornersCount = 1 << region.Dim;
                if (kvp.Value.corners.Count != cornersCount)
                    throw new InvalidOperationException($"Not enough bots to construct region {region} for {(kvp.Value.isFill ? nameof(GFill) : nameof(GVoid))}. " +
                                                        $"Bots: {string.Join("; ", kvp.Value.corners.Values)}");
                var bot = kvp.Value.corners.First().Value;
                botCommands[bot].Apply(this, bot);
            }

            // todo check fusionS and fusionP and fisson

            foreach (var kvp in botCommands)
            {
                if (kvp.Value is GroupCommand)
                    continue;
                if (kvp.Value is Halt)
                {
                    if (Bots.Count > 1)
                        throw new InvalidOperationException($"Couldn't halt. Too many bots left: {string.Join("; ", Bots)}");
                    if (Harmonics != Harmonics.Low)
                        throw new InvalidOperationException("Couldn't halt in high harmonics");
                }
                kvp.Value.Apply(this, kvp.Key);
            }

            EnsureWellFormed();
            return botCommands.OrderBy(kvp => kvp.Key.Bid).Select(kvp => kvp.Value).ToList();
        }


        private void AddVolatileCells(Bot bot, ICommand command, IEnumerable<Vec> volatileCells)
        {
            foreach (var vec in volatileCells)
            {
                if (vec.Equals(bot.Position))
                    continue;

                if (VolatileCells.TryGetValue(vec, out var conflict))
                    throw new InvalidOperationException($"Common volatile cell {vec}. " +
                                                        $"Bots: {bot}; {conflict.bot}. " +
                                                        $"Commands: {command}; {conflict.message}");
                VolatileCells.Add(vec, (bot, command.ToString()));
            }
        }

        private void EnsureWellFormed()
        {
            if (Harmonics == Harmonics.Low && Matrix.HasNonGroundedVoxels)
                throw new InvalidOperationException("Low Harmonics while non grounded voxel present");
        }

        public IEnumerable<Vec> GetFilledVoxels()
        {
            return Matrix.GetFilledVoxels();
        }

        public IEnumerable<Vec> GetGroundedCellsToBuild()
        {
            var state = new HashSet<Vec>(GetFilledVoxels());
            var result = new HashSet<Vec>();
            for (int x = 0; x < Matrix.R; x++)
                for (int y = 0; y < Matrix.R; y++)
                    for (int z = 0; z < Matrix.R; z++)
                    {
                        var vec = new Vec(x, y, z);
                        if (TargetMatrix[vec]
                            && !state.Contains(vec)
                            && (y == 0 || vec.GetMNeighbours(TargetMatrix).Any(nvec => state.Contains(nvec))))
                        {
                            result.Add(vec);
                        }
                    }
            return result;
        }
    }
}