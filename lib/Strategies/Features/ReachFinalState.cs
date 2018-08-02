using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class ReachFinalState : Strategy
    {
        public ReachFinalState(State state)
            : base(state)
        {
        }

        protected override async StrategyTask<bool> Run()
        {
            await WhenAll(state.Bots.Select(x => new ReachTarget(state, x, Vec.Zero)));

            var master = state.Bots.SingleOrDefault(b => b.Position == Vec.Zero);
            if (master == null)
                return false;

            var botsLeft = state.Bots.Except(new[] { master }).ToList();
            var fusionPositions = Vec.Zero.GetNears().Where(n => n.IsInCuboid(state.Matrix.R)).ToList();
            var targets = botsLeft.Select((bot, i) => (bot, vec: fusionPositions[i % fusionPositions.Count]))
                                  .ToDictionary(x => x.bot, x => x.vec);
            while (botsLeft.Count > 0)
            {
                Bot merging = null;
                var strategies = new List<IStrategy>();
                for (var i = 0; i < botsLeft.Count; i++)
                {
                    var bot = botsLeft[i];
                    if (!fusionPositions.Contains(bot.Position))
                        strategies.Add(new ReachTarget(state, bot, targets[bot]));
                    else if (merging == null)
                    {
                        merging = bot;
                        strategies.Add(MergeNears(master, bot));
                    }
                }
                await WhenAll(strategies);
               
                if (merging != null)
                    botsLeft.Remove(merging);
            }

            if (state.Harmonics == Harmonics.High)
                await Do(state.Bots.Single(), new Flip());
            return await Halt();
        }
    }
}