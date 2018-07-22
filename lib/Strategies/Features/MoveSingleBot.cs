using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class MoveSingleBot : SimpleSingleBotStrategyBase
    {
        private readonly Vec target;
        
        public MoveSingleBot(DeluxeState state, Bot bot, Vec target)
            : base(state, bot)
        {
            this.target = target;
        }

        protected override async StrategyTask<bool> Run()
        {
            var commands = new PathFinder(state, bot.Position, target).TryFindPath();
            commands?.Reverse();
            if (commands == null)
                return false;

            var first = true;
            while (commands.Count > 0)
            {
                if (first)
                    first = false;
                else
                {
                    var commandPositions = commands[commands.Count - 1].GetVolatileCells(bot);
                    if (commandPositions.Any(pos => state.VolatileCells.ContainsKey(pos)))
                    {
                        commands = new PathFinder(state, bot.Position, target).TryFindPath();
                        commands?.Reverse();
                        if (commands == null)
                            return false;
                    }
                }

                await Do(commands[commands.Count - 1]);
                commands.RemoveAt(commands.Count - 1);
            }

            return true;
        }
    }
}