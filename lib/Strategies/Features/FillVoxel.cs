using System;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class FillVoxel : BotStrategy
    {
        private readonly Vec whatToFill;
        private readonly Vec fromPos;
        private readonly Func<bool> canFill;

        public FillVoxel(DeluxeState state, Bot bot, Vec whatToFill, Vec fromPos)
            : base(state, bot)
        {
            this.whatToFill = whatToFill;
            this.fromPos = fromPos;
            this.canFill = canFill ?? (() => true);
        }

        protected override async StrategyTask<bool> Run()
        {
            if (bot.Position != fromPos)
            {
                if (!await new Move(state, bot, fromPos))
                    return false;
            }

            var command = new Fill(new NearDifference(whatToFill - fromPos));
            if (command.HasVolatileConflicts(bot, state) || state.Matrix[whatToFill] || !canFill())
                return false;

            await Do(command);
            return true;
        }
    }
}