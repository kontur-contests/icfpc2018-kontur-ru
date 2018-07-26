using lib.Commands;
using lib.Models;
using lib.Utils;

namespace lib.Strategies.Features
{
    public abstract class BotStrategy : Strategy
    {
        protected readonly Bot bot;

        protected BotStrategy(State state, Bot bot)
            : base(state)
        {
            this.bot = bot;
        }

        protected IStrategy Do(ICommand command)
        {
            return Do(bot, command);
        }

        protected IStrategy Move(Vec target)
        {
            return Move(bot, target);
        }

        public override string ToString()
        {
            return $"{base.ToString()}[{bot.Bid} at {bot.Position}]";
        }
    }
}