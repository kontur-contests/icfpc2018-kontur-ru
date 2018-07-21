using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using lib.Commands;
using lib.Utils;

namespace lib.Models
{
    public class MutableState
    {
        [NotNull]
        private readonly Matrix targetMatrix;

        public MutableState([NotNull] Matrix targetMatrix)
        {
            this.targetMatrix = targetMatrix;
            Bots = new List<Bot> { new Bot { Bid = 1, Position = Vec.Zero, Seeds = Enumerable.Range(2, 19).ToList() } };
            Energy = 0;
            Harmonics = Harmonics.Low;
            BuildingMatrix = new ComponentTrackingMatrix(new Matrix(targetMatrix.N));
        }

        public List<long> EnergyHistory { get; } = new List<long>();
        public long Energy { get; set; }
        public Harmonics Harmonics { get; set; }

        public ComponentTrackingMatrix BuildingMatrix { get; set; }
        public List<Bot> Bots { get; set; }

        public void Tick(Queue<ICommand> trace)
        {
            var botCommands
                = Bots.OrderBy(b => b.Bid).Select(bot => new { bot, command = trace.Dequeue() }).ToList();

            Energy += Harmonics == Harmonics.High
                          ? 30 * targetMatrix.R * targetMatrix.R * targetMatrix.R
                          : 3 * targetMatrix.R * targetMatrix.R * targetMatrix.R;
            Energy += 20 * Bots.Count;


            var volitileCells =
                from bc in botCommands
                from cell in bc.command.GetVolatileCells(this, bc.bot)
                select new { bc, cell };
            var badGroup = volitileCells.GroupBy(c => c.cell).FirstOrDefault(g => g.Count() > 1);
            if (badGroup != null)
            {
                throw new InvalidOperationException($"Common volatile cell {badGroup.Key}. " +
                                                    $"Bots: {string.Join(", ", badGroup.Select(b => b.bc.bot.Bid))} " +
                                                    $"Commands: {string.Join(", ", badGroup.Select(b => b.bc.command))} ");
            }
            //TODO: Validate pair commands (Fission-Fusion)
            foreach (var botCommand in botCommands)
            {
                botCommand.command.Apply(this, botCommand.bot);
            }
            EnsureWellFormed();
            EnergyHistory.Add(Energy);
        }

        public void EnsureIsFinal()
        {
            if (Bots.Any())
                throw new InvalidOperationException("State is not final");
            for (int i = 0; i < targetMatrix.N; i++)
                for (int j = 0; j < targetMatrix.N; j++)
                    for (int k = 0; k < targetMatrix.N; k++)
                        if (targetMatrix[i, j, k] != BuildingMatrix[i, j, k])
                            throw new InvalidOperationException("BuildingMatrix differs from targetMatrix");
        }

        public void EnsureWellFormed()
        {
            if (Harmonics == Harmonics.Low && BuildingMatrix.HasNonGroundedVoxels)
            {
                throw new InvalidOperationException($"Low Harmonics while non grounded voxel present");
            }
        }
    }
}