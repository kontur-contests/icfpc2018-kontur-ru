using lib.Commands;
using lib.Models;
using lib.Strategies.Features.Async;

namespace lib.Strategies.Features
{
    public class Do : BotStrategy
    {
        private readonly ICommand command;

        public Do(State state, Bot bot, ICommand command)
            : base(state, bot)
        {
            this.command = command;
        }

        protected override async StrategyTask<bool> Run()
        {
            state.SetBotCommand(Bot, command);
            return await WhenNextTurn();
        }
    }
}