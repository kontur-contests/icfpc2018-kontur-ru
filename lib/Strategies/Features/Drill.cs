using System;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class Drill : BotStrategy
    {
        private readonly Vec target;

        public Drill(State state, Bot bot, Vec target)
            : base(state, bot)
        {
            this.target = target;
        }

        protected override async StrategyTask<bool> Run()
        {
            while (true)
            {
                var hasPath = new PathFinderNeighbours(state.Matrix, Bot.Position, target, x => !state.IsVolatile(Bot, x)).TryFindPath(out var used);
                if (hasPath)
                {
                    var commands = new PathFinder(state, Bot, target).TryFindPath();
                    if (commands == null)
                        throw new InvalidOperationException("WTF??");
                    if (await Move(Bot, target))
                        return true;
                    continue;
                }

                var moveTarget = used
                                     .Where(v => new Region(v, target).Dim == 1 
                                                 && !new Region(v, target).Any(x => x != Bot.Position && state.IsVolatile(Bot, x)))
                                     .OrderBy(v => v.MDistTo(target)).FirstOrDefault();
                if (moveTarget == null)
                {
                    await WhenNextTurn();
                    continue;
                }
                if (!await Move(Bot, moveTarget))
                    continue;

                var drillTarget = Bot.Position.GetMNeighbours(state.Matrix).OrderBy(n => n.MDistTo(target)).First();
                if (state.IsVolatile(Bot, drillTarget))
                {
                    await WhenNextTurn();
                    continue;
                }
                if (state.Matrix[drillTarget])
                    await Do(new Voidd(new NearDifference(drillTarget - Bot.Position)));
            }
        }
    }
}