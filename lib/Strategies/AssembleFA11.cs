using System;
using System.Collections.Generic;
using System.Linq;

using lib.Commands;
using lib.Models;
using lib.Primitives;
using lib.Strategies.Features;
using lib.Strategies.Features.Async;
using lib.Utils;

namespace lib.Strategies
{
    public class PlaneAssembler : Strategy
    {
        private readonly Bot[] bots;
        private readonly Region plane;

        public PlaneAssembler(DeluxeState state, Bot[] bots, Region plane)
            : base(state)
        {
            this.bots = bots;
            this.plane = plane;
        }

        protected override async StrategyTask<bool> Run()
        {
            var vertices = plane.Vertices().Distinct().ToList();
            var strategies = new IStrategy[bots.Length];
            for (var i = 0; i < bots.Length; i++)
            {
                var bot = bots[i];
                vertices.Sort(i, vertices.Count - i, Comparer<Vec>.Create((a, b) => a.MDistTo(bot.Position).CompareTo(b.MDistTo(bot.Position))));

                var target = vertices[i]
                    .GetNears()
                    .Where(n => n.IsInCuboid(R) && !n.IsInRegion(plane))
                    .OrderBy(v => state.Matrix[v])
                    .First();
                strategies[i] = new Drill(state, bot, target);
            }
            await WhenAll(strategies);

            // todo High if problems

            for (int i = 0; i < bots.Length; i++)
                state.SetBotCommand(bots[i], new GFill(new NearDifference(vertices[i] - bots[i].Position), new FarDifference(plane.Opposite(vertices[i]) - vertices[i])));
            return await WhenNextTurn();
        }
    }

    public class AssembleOneBox : Strategy
    {
        public AssembleOneBox(DeluxeState state)
            : base(state)
        {
        }

        protected override async StrategyTask<bool> Run()
        {
            var split = new Split(state, state.Bots.Single(), 4);
            await split;

            await new Assembler4(state, split.Bots);
            return await Finalize();
        }
    }

    public class Assembler4 : Strategy
    {
        private readonly Bot[] bots;
        private Region region;

        public Assembler4(DeluxeState state, IEnumerable<Bot> bots4)
            : base(state)
        {
            var minX = state.TargetMatrix.GetFilledVoxels().Min(v => v.X);
            var minY = state.TargetMatrix.GetFilledVoxels().Min(v => v.Y);
            var minZ = state.TargetMatrix.GetFilledVoxels().Min(v => v.Z);
            var maxX = state.TargetMatrix.GetFilledVoxels().Max(v => v.X);
            var maxY = state.TargetMatrix.GetFilledVoxels().Max(v => v.Y);
            var maxZ = state.TargetMatrix.GetFilledVoxels().Max(v => v.Z);
            region = new Region(new Vec(minX, minY, minZ), new Vec(maxX, maxY, maxZ));
            bots = bots4.ToArray();
            if (bots.Length != 4)
                throw new InvalidOperationException("We need exactly 4 bots");
        }

        protected override async StrategyTask<bool> Run()
        {
            var s = region.Start;
            var e = region.End;
            var z0 = new Region(s, e.WithZ(s.Z));
            var z1 = new Region(s.WithZ(e.Z), e);
            var x0 = new Region(s, e.WithX(s.X));
            var x1 = new Region(s.WithX(e.X), e);
            var y0 = new Region(s, e.WithY(s.Y));
            var y1 = new Region(s.WithY(e.Y), e);
            await new PlaneAssembler(state, bots, z0);
            await new PlaneAssembler(state, bots, z1);
            await new PlaneAssembler(state, bots, x0);
            await new PlaneAssembler(state, bots, x1);
            await new PlaneAssembler(state, bots, y0);
            await new PlaneAssembler(state, bots, y1);
            return true;
        }
    }
}