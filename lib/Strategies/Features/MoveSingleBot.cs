using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class MoveSingleBot : IStrategy
    {
        private readonly DeluxeState state;
        private readonly Bot bot;
        private readonly Vec target;
        private List<ICommand> commands;

        public MoveSingleBot(DeluxeState state, Bot bot, Vec target)
        {
            this.state = state;
            this.bot = bot;
            this.target = target;
        }

        public IStrategy[] Tick()
        {
            var first = false;
            if (commands == null)
            {
                first = true;
                commands = new PathFinder(state, bot.Position, target).TryFindPath();
                commands?.Reverse();
                if (commands == null)
                {
                    Status = StrategyStatus.Failed;
                    return null;
                }
            }

            if (commands.Count == 0)
            {
                Status = StrategyStatus.Done;
                return null;
            }

            if (!first)
            {
                var commandPositions = commands[commands.Count - 1].GetVolatileCells(bot);
                if (commandPositions.Any(pos => state.VolatileCells.ContainsKey(pos)))
                {
                    commands = new PathFinder(state, bot.Position, target).TryFindPath();
                    commands?.Reverse();
                    if (commands == null)
                    {
                        Status = StrategyStatus.Failed;
                        return null;
                    }
                }
            }

            state.SetBotCommand(bot, commands[commands.Count - 1]);
            commands.RemoveAt(commands.Count - 1);
            return null;
        }

        public StrategyStatus Status { get; private set; }
    }
}