using lib.Commands;
using lib.Models;
using lib.Strategies.Features.Async;

namespace lib.Strategies.Features
{
    public abstract class SimpleSingleBotStrategyBase : SimpleStrategyBase
    {
        protected readonly Bot bot;

        protected SimpleSingleBotStrategyBase(DeluxeState state, Bot bot)
            : base(state)
        {
            this.bot = bot;
        }

        protected StrategyTask Do(ICommand command)
        {
            state.SetBotCommand(bot, command);
            return WhenNextTurn();
        }
    }
}