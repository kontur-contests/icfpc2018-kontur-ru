using System;
using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Utils;

namespace lib.Models
{
    public class MutableState
    {
        public MutableState()
        {
        }

        public MutableState(Matrix problem)
        {
            Bots = new List<Bot> {new Bot {Bid = 1, Position = Vec.Zero, Seeds = Enumerable.Range(2, 19).ToList()}};
            Energy = 0;
            Harmonics = Harmonics.Low;
            Matrix = new ComponentTrackingMatrix(problem);
        }

        public long Energy { get; set; }
        public Harmonics Harmonics { get; set; }
        public ComponentTrackingMatrix Matrix { get; set; }
            public List<Bot> Bots { get; set; }

        public void Tick(Queue<ICommand> trace)
        {
            var botCommands
                = Bots.OrderBy(b => b.Bid).Select(bot => new {bot, command = trace.Dequeue()}).ToList();
            var volitileCells =
                from bc in botCommands
                from cell in bc.command.GetVolatileCells(this, bc.bot)
                select new {bc, cell};
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
        }

        public void EnsureIsFinal()
        {
            if (Bots.Any())
                throw new InvalidOperationException($"State is not final");
        }

        public void EnsureWellFormed()
        {
            if (Harmonics == Harmonics.Low && Matrix.HasNonGroundedVoxels)
            {
                throw new InvalidOperationException($"Low Harmonics while non grounded voxel present");
            }
        }
    }
}