using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class ReachTarget : BotStrategy
    {
        private readonly Vec target;

        public ReachTarget(DeluxeState state, Bot bot, Vec target)
            : base(state, bot)
        {
            this.target = target;
        }

        protected override sealed async StrategyTask<bool> Run()
        {
            for (int attempt = 0; attempt < state.Bots.Count; attempt++)
            {
                if (bot.Position == target)
                    return true;

                if (state.IsVolatile(bot, target))
                    return false;

                var hasPath = new PathFinderNeighbours(state.Matrix, bot.Position, target, x => !state.IsVolatile(bot, x)).TryFindPath(out var used);
                if (hasPath)
                {
                    if (state.GetOwner(bot.Position) == bot)
                    {
                        var neighbor = bot.Position.GetNeighbors().First(n => used.Contains(n));

                        var prevPosition = bot.Position;
                        await Do(new SMove(neighbor - bot.Position));

                        state.Unown(bot, prevPosition);
                        await Do(new Fill(prevPosition - bot.Position));
                    }
                    if (await Move(bot, target))
                        return true;
                    continue;
                }
                
                var moveTarget = used
                    .Where(v => new Region(v, target).Dim == 1
                                && !new Region(v, target).Any(x => x != bot.Position && state.IsVolatile(bot, x)))
                    .OrderBy(v => v.MDistTo(target)).FirstOrDefault();
                if (moveTarget == null)
                {
                    await WhenNextTurn();
                    continue;
                }

                if (moveTarget != bot.Position)
                {
                    if (state.GetOwner(bot.Position) == bot)
                    {
                        var neighbor = bot.Position.GetNeighbors().First(n => used.Contains(n));

                        var prevPosition = bot.Position;
                        await Do(new SMove(neighbor - bot.Position));

                        state.Unown(bot, prevPosition);
                        await Do(new Fill(prevPosition - bot.Position));
                    }
                    if (!await Move(bot, moveTarget))
                        continue;
                }

                var drillTarget = bot.Position.GetMNeighbours(state.Matrix).OrderBy(n => n.MDistTo(target)).First();
                if (state.IsVolatile(bot, drillTarget))
                {
                    await WhenNextTurn();
                    continue;
                }

                if (!state.Matrix[drillTarget])
                {
                    var prevPosition = bot.Position;
                    await Do(new SMove(drillTarget - bot.Position));

                    if (state.GetOwner(prevPosition) == bot)
                    {
                        state.Unown(bot, prevPosition);
                        await Do(new Fill(prevPosition - bot.Position));
                    }
                }
                else
                {
                    state.Own(bot, drillTarget);
                    await Do(new Voidd(drillTarget - bot.Position));

                    var prevPosition = bot.Position;
                    await Do(new SMove(drillTarget - bot.Position));

                    if (state.GetOwner(prevPosition) == bot)
                    {
                        state.Unown(bot, prevPosition);
                        await Do(new Fill(prevPosition - bot.Position));
                    }
                }
            }

            return false;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(target)}: {target}";
        }
    }
}