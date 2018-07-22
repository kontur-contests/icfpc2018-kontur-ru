using lib.Commands;
using lib.Models;

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

        protected StrategyResult Do(ICommand command)
        {
            state.SetBotCommand(bot, command);
            return Wait();
        }
    }
}