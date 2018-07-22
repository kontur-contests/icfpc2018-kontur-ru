using System.Collections.Generic;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class FillVoxel : SimpleSingleBotStrategyBase
    {
        private readonly Vec whatToFill;
        private readonly Vec fromPos;
        
        public FillVoxel(DeluxeState state, Bot bot, Vec whatToFill, Vec fromPos)
            : base(state, bot)
        {
            this.whatToFill = whatToFill;
            this.fromPos = fromPos;
        }
        
        protected override IEnumerable<StrategyResult> Run()
        {
            if (bot.Position != fromPos)
            {
                var move = new MoveSingleBot(state, bot, fromPos);
                yield return Wait(move);

                if (move.Status == StrategyStatus.Failed)
                    yield return Failed();
            }
            yield return Do(new Fill(new NearDifference(whatToFill - fromPos)));
        }
    }
}