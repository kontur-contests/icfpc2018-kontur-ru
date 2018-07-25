using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class MiniBoxDisassembler : Strategy
    {
        private readonly Bot[] bots;
        private readonly Region miniBox;

        public MiniBoxDisassembler(State state, Bot[] bots, Region miniBox)
            : base(state)
        {
            this.bots = bots;
            this.miniBox = miniBox;
        }

        protected override async StrategyTask<bool> Run()
        {
            var vertices = miniBox.Vertices().ToList();
            var strategies = new IStrategy[bots.Length];
            for (var i = 0; i < bots.Length; i++)
            {
                var bot = bots[i];
                vertices.Sort(i, vertices.Count - i, Comparer<Vec>.Create((a, b) => a.MDistTo(bot.Position).CompareTo(b.MDistTo(bot.Position))));

                var target = vertices[i]
                    .GetNears()
                    .Where(n => n.IsInCuboid(R) && !n.IsInRegion(miniBox))
                    .OrderBy(v => state.Matrix[v])
                    .First();
                strategies[i] = new Drill(state, bot, target);
            }
            await WhenAll(strategies);

            // todo High if problems

            for (int i = 0; i < bots.Length; i++)
                state.SetBotCommand(bots[i], new GVoid(new NearDifference(vertices[i] - bots[i].Position), new FarDifference(miniBox.Opposite(vertices[i]) - vertices[i])));
            return await WhenNextTurn();
        }
    }
}