using System;
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

        public MiniBoxDisassembler(DeluxeState state, Bot[] bots, Region miniBox)
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

    public class Disassembler8 : Strategy
    {
        const int longDist = 30;
        private readonly Bot[] bots;

        public Disassembler8(DeluxeState state, IEnumerable<Bot> bots8)
            : base(state)
        {
            bots = bots8.ToArray();
            if (bots.Length != 8)
                throw new InvalidOperationException("We need exactly 8 bots");
        }

        protected override async StrategyTask<bool> Run()
        {
            while (true)
            {
                var highest = GetHighest();
                if (highest == -1)
                    return true;

                var layerBox = FindLayerRegion(highest);
                var miniBoxes = SplitLayerBox(layerBox);
                foreach (var miniBox in miniBoxes)
                {
                    if (IsEmpty(miniBox))
                        continue;
                    await new MiniBoxDisassembler(state, bots, miniBox);
                }
            }
        }

        private bool IsEmpty(Region region)
        {
            foreach (var vec in region)
            {
                if (state.Matrix[vec])
                    return false;
            }
            return true;
        }

        private IEnumerable<Region> SplitLayerBox(Region layerBox)
        {
            var startY = layerBox.Start.Y;
            var endY = layerBox.End.Y;
            for (int startX = layerBox.Start.X; startX <= layerBox.End.X; startX += longDist)
            {
                var endX = Math.Min(startX + longDist - 1, layerBox.End.X);
                for (int startZ = layerBox.Start.Z; startZ <= layerBox.End.Z; startZ += longDist)
                {
                    var endZ = Math.Min(startZ + longDist - 1, layerBox.End.Z);
                    yield return new Region(new Vec(startX, startY, startZ), new Vec(endX, endY, endZ));
                }
            }
        }

        private Region FindLayerRegion(int highest)
        {
            var region = new Region(new Vec(0, highest, 0), new Vec(R - 1, Math.Max(0, highest - longDist + 1), R - 1));
            var filledCells = region.Where(v => state.Matrix[v]).ToList();
            var minX = filledCells.Min(v => v.X);
            var minY = filledCells.Min(v => v.Y);
            var minZ = filledCells.Min(v => v.Z);
            var maxX = filledCells.Max(v => v.X);
            var maxY = filledCells.Max(v => v.Y);
            var maxZ = filledCells.Max(v => v.Z);
            return new Region(new Vec(minX, minY, minZ), new Vec(maxX, maxY, maxZ));
        }

        private int GetHighest()
        {
            for (int y = R - 1; y >= 0; y--)
            {
                for (int x = 0; x < R; x++)
                    for (int z = 0; z < R; z++)
                    {
                        if (state.Matrix[x, y, z])
                            return y;
                    }
            }
            return -1;
        }
    }
}