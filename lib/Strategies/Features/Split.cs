using System;
using System.Collections.Generic;

using lib.Models;
using lib.Strategies.Features.Async;

namespace lib.Strategies.Features
{
    public class Split : BotStrategy
    {
        private readonly int count;

        public Split(State state, Bot bot, int count = int.MaxValue)
            : base(state, bot)
        {
            this.count = Math.Min(count, bot.Seeds.Count + 1);
        }

        public List<Bot> Bots { get; } = new List<Bot>();

        protected override async StrategyTask<bool> Run()
        {
            Bots.Add(bot);
            var generator = bot;
            for (int i = 0; i < count - 1; i++)
            {
                var deriveOne = new DeriveOne(state, generator);
                if (!await deriveOne)
                    return false;

                generator = deriveOne.DerivedBot;
                Bots.Add(deriveOne.DerivedBot);
            }
            return true;
        }
    }
}