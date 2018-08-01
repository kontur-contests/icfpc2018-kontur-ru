using lib.Commands;
using lib.Models;
using lib.Utils;

namespace lib.Strategies.Features
{
    public abstract class BotStrategy : Strategy
    {
        protected BotStrategy(State state, Bot bot)
            : base(state)
        {
            Bot = bot;
        }

        public Bot Bot { get; }

        protected IStrategy Do(ICommand command)
        {
            return Do(Bot, command);
        }

        protected IStrategy Move(Vec target)
        {
            return Move(Bot, target);
        }

        public override string ToString()
        {
            return $"{base.ToString()}[{Bot.Bid} at {Bot.Position}]";
        }
    }
}