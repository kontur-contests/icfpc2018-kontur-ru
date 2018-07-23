using System.Linq;

using lib.Models;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class GotoVertex : ReachAnyTarget
    {
        public GotoVertex(DeluxeState state, Bot bot, Region region, Vec vertex)
            : base(state, bot, () => vertex
                                         .GetMNeighbours(state.Matrix)
                                         .Where(v => !v.IsInRegion(region)
                                                     && !state.VolatileCells.ContainsKey(v))
                                         .OrderBy(v => state.Matrix[v])
                                         .ThenByDescending(v => v.Y)
                                         .ThenBy(v => v.MDistTo(bot.Position)))
        {
        }
    }
}