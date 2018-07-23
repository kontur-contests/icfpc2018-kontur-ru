using System.Linq;

using lib.Models;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class GetOutOfRegionSingleBot : ReachAnyTarget
    {
        public GetOutOfRegionSingleBot(DeluxeState state, Bot bot, Region region)
            : base(state, bot, () => region
                                         .SelectMany(v => v.GetMNeighbours(state.Matrix))
                                         .Distinct()
                                         .Where(v => !v.IsInRegion(region)
                                                     && !state.VolatileCells.ContainsKey(v)
                                                     && !state.Matrix[v])
                                         .OrderByDescending(v => v.Y)
                                         .ThenBy(v => v.MDistTo(bot.Position)))
        {
        }
    }
}