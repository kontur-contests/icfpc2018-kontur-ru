using System;
using System.Collections.Generic;
using System.Linq;

using lib.Models;
using lib.Utils;

namespace lib.Strategies.Features
{
    public class GenPlanBuilder
    {
        private static readonly int[] dists = { 30, 16, 8, 4, 1 };
        private static readonly int longDist = dists[0];

        private readonly State state;
        private readonly int R;

        public GenPlanBuilder(State state)
        {
            this.state = state;
            R = state.R;
        }

        public List<Region> CreateGenPlan()
        {
            var highest = GetHighestTarget();
            var plan = new List<Region>();
            for (int startY = 0; startY <= highest; startY += longDist)
                CreateLayerGenPlan(startY, Math.Min(highest - startY + 1, longDist), longDist, plan);
            return plan;
        }

        private void CreateLayerGenPlan(int startY, int height, int width, List<Region> plan)
        {
            var layerBox = FindLayerRegion(startY, height);
            var miniBoxes = SplitLayerBox(layerBox, width);
            foreach (var miniBox in miniBoxes)
            {
                if (ShouldBeFull(miniBox))
                    plan.Add(miniBox);
            }
            var nextDistIndex = Array.IndexOf(dists, width) + 1;
            if (nextDistIndex >= dists.Length)
                return;
            var nextDist = dists[nextDistIndex];
            for (int nextStartY = startY; nextStartY <= startY + height - 1; nextStartY += nextDist)
                CreateLayerGenPlan(nextStartY, Math.Min(nextDist, startY + height - nextStartY), nextDist, plan);
        }

        private bool ShouldBeFull(Region region)
        {
            return region.All(v => state.TargetMatrix[v]);
        }

        private int GetHighestTarget()
        {
            for (int y = R - 1; y >= 0; y--)
            {
                for (int x = 0; x < R; x++)
                    for (int z = 0; z < R; z++)
                    {
                        if (state.TargetMatrix[x, y, z])
                            return y;
                    }
            }
            return -1;
        }

        private IEnumerable<Region> SplitLayerBox(Region layerBox, int width)
        {
            var startY = layerBox.Start.Y;
            var endY = layerBox.End.Y;
            for (int startX = layerBox.Start.X; startX <= layerBox.End.X; startX += width)
            {
                var endX = Math.Min(startX + width - 1, layerBox.End.X);
                for (int startZ = layerBox.Start.Z; startZ <= layerBox.End.Z; startZ += width)
                {
                    var endZ = Math.Min(startZ + width - 1, layerBox.End.Z);
                    yield return new Region(new Vec(startX, startY, startZ), new Vec(endX, endY, endZ));
                }
            }
        }

        private Region FindLayerRegion(int startY, int dist)
        {
            var region = new Region(new Vec(0, startY, 0), new Vec(R - 1, Math.Min(R - 1, startY + dist - 1), R - 1));
            var filledCells = region.Where(v => state.TargetMatrix[v]).ToList();
            var minX = filledCells.Min(v => v.X);
            var minY = filledCells.Min(v => v.Y);
            var minZ = filledCells.Min(v => v.Z);
            var maxX = filledCells.Max(v => v.X);
            var maxY = filledCells.Max(v => v.Y);
            var maxZ = filledCells.Max(v => v.Z);
            return new Region(new Vec(minX, minY, minZ), new Vec(maxX, maxY, maxZ));
        }
    }
}