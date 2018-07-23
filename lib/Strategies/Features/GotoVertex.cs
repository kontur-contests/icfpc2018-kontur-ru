using System.Linq;

using lib.Models;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class GotoVertex : ReachAnyTarget
    {
        private readonly Vec vertex;

        public GotoVertex(DeluxeState state, Bot bot, Region region, Vec vertex)
            : base(state, bot, () => vertex
                                         .GetNears()
                                         .Where(v => v.IsInCuboid(state.R)
                                                     && !v.IsInRegion(region)
                                                     && !state.IsVolatile(bot, v))
                                         .OrderBy(v => state.Matrix[v])
                                         .ThenByDescending(v => v.Y)
                                         .ThenBy(v => v.MDistTo(bot.Position)))
        {
            this.vertex = vertex;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(vertex)}: {vertex}";
        }
    }
}