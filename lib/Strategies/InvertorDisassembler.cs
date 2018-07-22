using System;
using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Utils;

using MoreLinq;

namespace lib.Strategies
{
    public class InvertorDisassembler : IAmSolver
    {
        private readonly IAmSolver assembler;
        private readonly Matrix sourceMatrix;
        private readonly Matrix targetMatrix;

        public InvertorDisassembler(IAmSolver assembler, Matrix sourceMatrix, Matrix targetMatrix = null)
        {
            this.assembler = assembler;
            this.sourceMatrix = sourceMatrix;
            this.targetMatrix = targetMatrix ?? new Matrix(sourceMatrix.R);
        }

        public IEnumerable<ICommand> Solve()
        {
            var commands = new Queue<ICommand>(assembler.Solve());
            return ReverseCommands(commands, targetMatrix, sourceMatrix);
        }

        public static IEnumerable<ICommand> ReverseCommands(Queue<ICommand> commands, Matrix initialMatrix, Matrix finalMatrix)
        {
            var ticks = new List<List<(Vec botPos, ICommand command)>>();
            var state = new DeluxeState(initialMatrix, finalMatrix);
            while (commands.Any())
            {
                state.StartTick();
                var reversedTick = new List<(Vec botPos, ICommand command)>();
                var primaryBidBySecondaryBotPosition = new Dictionary<Vec, int>();
                var mByPrimaryBotPosition = new Dictionary<Vec, int>();
                foreach (var bot in state.Bots.OrderBy(b => b.Bid))
                {
                    var bid = bot.Bid;
                    var command = commands.Dequeue();
                    if (command is SMove smove) reversedTick.Add((bot.Position + smove.Shift.Shift, new SMove(new LongLinearDifference(-smove.Shift.Shift))));
                    else if (command is Fill fill) reversedTick.Add((bot.Position, new Voidd(fill.Shift)));
                    else if (command is Voidd voidd) reversedTick.Add((bot.Position, new Fill(voidd.Shift)));
                    else if (command is Wait wait) reversedTick.Add((bot.Position, wait));
                    else if (command is Flip flip) reversedTick.Add((bot.Position, flip));
                    else if (command is LMove move) reversedTick.Add((bot.Position + move.firstShift.Shift + move.secondShift.Shift, new LMove(new ShortLinearDifference(-move.secondShift.Shift), new ShortLinearDifference(-move.firstShift.Shift))));
                    else if (command is Fission fission)
                    {
                        reversedTick.Add((bot.Position, new FusionP(fission.shift)));
                        reversedTick.Add((bot.Position + fission.shift.Shift, new FusionS(new NearDifference(-fission.shift.Shift))));
                    }
                    else if (command is FusionP fussionP)
                    {
                        var botPos = bot.Position;
                        var secondaryBotPosition = botPos + fussionP.shift.Shift;
                        if (mByPrimaryBotPosition.ContainsKey(botPos))
                        {
                            reversedTick.Add((bot.Position, new Fission(fussionP.shift, mByPrimaryBotPosition[botPos])));
                        }
                        else
                        {
                            primaryBidBySecondaryBotPosition.Add(secondaryBotPosition, bid);
                        }
                    }
                    else if (command is FusionS fussionS)
                    {
                        var botPos = bot.Position;
                        var primaryBotPosition = botPos + fussionS.shift.Shift;
                        if (primaryBidBySecondaryBotPosition.ContainsKey(botPos))
                        {
                            reversedTick.Add((primaryBotPosition, new Fission(new NearDifference(-fussionS.shift.Shift), bot.Seeds.Count)));
                        }
                        else
                            mByPrimaryBotPosition.Add(primaryBotPosition, bot.Seeds.Count);
                    }
                    else if (command is Halt)
                    {
                    }
                    else
                    {
                        throw new NotImplementedException(command.ToString());
                    }
                    state.SetBotCommand(bot, command);
                }
                if (reversedTick.Any())
                {
                    ticks.Add(reversedTick);
                }
                state.EndTick();
            }
            ticks.Reverse();
            state = new DeluxeState(finalMatrix, initialMatrix);
            var result = new List<ICommand>();
            foreach (var tick in ticks)
            {
                state.StartTick();
                var botsByPos = state.Bots.ToDictionary(b => b.Position);
                foreach (var (pos, cmd) in tick)
                {
                    state.SetBotCommand(botsByPos[pos], cmd);
                }
                result.AddRange(state.EndTick());
            }

            return result.Concat(new[] {new Halt()});
        }
    }
}