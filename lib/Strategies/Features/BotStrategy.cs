using lib.Commands;
using lib.Models;
using lib.Strategies.Features.Async;

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

        protected StrategyTask Do(ICommand command)
        {
            state.SetBotCommand(bot, command);
            return WhenNextTurn();
        }

        public override string ToString()
        {
            return $"{base.ToString()}[{bot.Bid} at {bot.Position}]";
        }
    }
}