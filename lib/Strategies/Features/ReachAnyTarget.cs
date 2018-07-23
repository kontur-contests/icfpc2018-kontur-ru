using System;
using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

using MoreLinq;

namespace lib.Strategies.Features
{
    public class ReachAnyTarget : BotStrategy
    {
        private readonly Func<IEnumerable<Vec>> getTargets;

        public ReachAnyTarget(DeluxeState state, Bot bot, Func<IEnumerable<Vec>> getTargets)
            : base(state, bot)
        {
            this.getTargets = getTargets;
        }

        protected override sealed async StrategyTask<bool> Run()
        {
            List<Vec> prevTargets = null;
            for (int attempt = 0; attempt < state.Bots.Count; attempt++)
            {
                var targets = getTargets().ToList();

                if (prevTargets != null && SetsAreEqual(prevTargets, targets))
                {
                    await WhenNextTurn();
                    continue;
                }
                prevTargets = targets;

                foreach (var target in targets)
                {
                    if (await Move(bot, target))
                        return true;
                }
            }
            return false;
        }

        private static bool SetsAreEqual<T>(IEnumerable<T> a, IEnumerable<T> b)
        {
            return a.ToHashSet().SetEquals(b);
        }
    }
}