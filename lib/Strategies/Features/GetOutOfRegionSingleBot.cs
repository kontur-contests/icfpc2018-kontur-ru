using System.Linq;

using lib.Models;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class GetOutOfRegionSingleBot : ReachAnyTarget
    {
        public GetOutOfRegionSingleBot(State state, Bot bot, params Region[] regions)
            : base(state, bot, () => regions
                                         .SelectMany(v => v.SelectMany(x => x.GetMNeighbours(state.Matrix)))
                                         .Distinct()
                                         .Where(v => regions.All(rr => !v.IsInRegion(rr))
                                                     && !state.IsVolatile(bot, v))
                                         .OrderBy(v => state.Matrix[v])
                                         .ThenByDescending(v => v.Y)
                                         .ThenBy(v => v.MDistTo(bot.Position)))
        {
        }
    }
}