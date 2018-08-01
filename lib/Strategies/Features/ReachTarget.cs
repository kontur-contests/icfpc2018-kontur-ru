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

        public ReachTarget(State state, Bot bot, Vec target)
            : base(state, bot)
        {
            this.target = target;
        }

        protected override sealed async StrategyTask<bool> Run()
        {
            for (int attempt = 0; attempt < state.Bots.Count; attempt++)
            {
                if (Bot.Position == target)
                    return true;

                if (state.IsVolatile(Bot, target))
                    return false;

                var hasPath = new PathFinderNeighbours(state.Matrix, Bot.Position, target, x => !state.IsVolatile(Bot, x)).TryFindPath(out var used);
                if (hasPath)
                {
                    if (state.GetOwner(Bot.Position) == Bot)
                    {
                        var neighbor = Bot.Position.GetNeighbors().First(n => used.Contains(n));

                        var prevPosition = Bot.Position;
                        await Do(new SMove(neighbor - Bot.Position));

                        state.Unown(Bot, prevPosition);
                        await Do(new Fill(prevPosition - Bot.Position));
                    }
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

                if (moveTarget != Bot.Position)
                {
                    if (state.GetOwner(Bot.Position) == Bot)
                    {
                        var neighbor = Bot.Position.GetNeighbors().First(n => used.Contains(n));

                        var prevPosition = Bot.Position;
                        await Do(new SMove(neighbor - Bot.Position));

                        state.Unown(Bot, prevPosition);
                        await Do(new Fill(prevPosition - Bot.Position));
                    }
                    if (!await Move(Bot, moveTarget))
                        continue;
                }

                var drillTarget = Bot.Position.GetMNeighbours(state.Matrix).OrderBy(n => n.MDistTo(target)).First();
                if (state.IsVolatile(Bot, drillTarget))
                {
                    await WhenNextTurn();
                    continue;
                }

                if (!state.Matrix[drillTarget])
                {
                    var prevPosition = Bot.Position;
                    await Do(new SMove(drillTarget - Bot.Position));

                    if (state.GetOwner(prevPosition) == Bot)
                    {
                        state.Unown(Bot, prevPosition);
                        await Do(new Fill(prevPosition - Bot.Position));
                    }
                }
                else
                {
                    state.Own(Bot, drillTarget);
                    await Do(new Voidd(drillTarget - Bot.Position));

                    var prevPosition = Bot.Position;
                    await Do(new SMove(drillTarget - Bot.Position));

                    if (state.GetOwner(prevPosition) == Bot)
                    {
                        state.Unown(Bot, prevPosition);
                        await Do(new Fill(prevPosition - Bot.Position));
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