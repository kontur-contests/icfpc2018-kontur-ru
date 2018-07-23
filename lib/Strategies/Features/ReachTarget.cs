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
                if (state.VolatileCells.ContainsKey(target))
                    return false;

                var hasPath = new PathFinderNeighbours(state.Matrix, bot.Position, target, x => !state.VolatileCells.ContainsKey(x)).TryFindPath(out var used);
                if (hasPath)
                {
                    if (await Move(bot, target))
                        return true;
                    continue;
                }
                
                var moveTarget = used
                    .Where(v => new Region(v, target).Dim == 1
                                && !new Region(v, target).Any(x => x != bot.Position && state.VolatileCells.ContainsKey(x)))
                    .OrderBy(v => v.MDistTo(target)).FirstOrDefault();
                if (moveTarget == null)
                {
                    await WhenNextTurn();
                    continue;
                }
                if (!await Move(bot, moveTarget))
                    continue;

                var drillTarget = bot.Position.GetMNeighbours(state.Matrix).OrderBy(n => n.MDistTo(target)).First();
                if (state.VolatileCells.ContainsKey(drillTarget))
                {
                    await WhenNextTurn();
                    continue;
                }

                if (!state.Matrix[drillTarget])
                    await Do(new SMove(drillTarget - bot.Position));
                else
                {
                    await Do(new Voidd(drillTarget - bot.Position));

                    //if (state.VolatileCells.ContainsKey(drillTarget))
                    //{
                    //    // 
                    //}

                    //await Do(new SMove(drillTarget - bot.Position));
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