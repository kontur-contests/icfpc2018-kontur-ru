using System.Collections.Generic;
using System.Linq;

using lib.Utils;

using MoreLinq;

namespace lib.Models
{
    public class ComponentTrackingMatrix : IMatrix
    {
        // 0 - void
        // >0 - filled. value = newComponentIndex of connected component
        private readonly int[,,] componentIndexes;
        private readonly Dictionary<int, int> componentSize = new Dictionary<int, int>();
        public readonly HashSet<int> groundedComponents = new HashSet<int>();
        private readonly Matrix matrix;
        private int nextComponentIndex;

        public bool HasNonGroundedVoxels => componentSize.Keys.Any(compIndex => !groundedComponents.Contains(compIndex));

        public ComponentTrackingMatrix(Matrix matrix)
        {
            this.matrix = matrix;
            componentIndexes = new int[R, R, R];
            FillComponentIndexes();
        }

        private void FillComponentIndexes()
        {
            nextComponentIndex = 1;
            for (int x = 0; x < R; x++)
                for (int y = 0; y < R; y++)
                    for (int z = 0; z < R; z++)
                    {
                        if (matrix[x, y, z] && componentIndexes[x, y, z] == 0)
                        {
                            var size = FillComponent(new Vec(x, y, z), nextComponentIndex);
                            componentSize[nextComponentIndex] = size;
                            nextComponentIndex++;
                        }
                    }
        }

        private int FillComponent(Vec start, int newComponentIndex)
        {
            var size = 1;
            var q = new Queue<Vec>();
            var oldComponentIndex = componentIndexes[start.X, start.Y, start.Z];
            q.Enqueue(start);
            AssignComponentIndexTo(start, newComponentIndex);
            while (q.Count != 0)
            {
                var p = q.Dequeue();
                foreach (var n in p.GetMNeighbours())
                {
                    if (this.IsInside(n) && this[n] && componentIndexes[n.X, n.Y, n.Z] == oldComponentIndex)
                    {
                        AssignComponentIndexTo(n, newComponentIndex);
                        size++;
                        q.Enqueue(n);
                    }
                }
            }
            return size;
        }

        public void AssignComponentIndexTo(Vec voxel, int componentIndex)
        {
            componentIndexes[voxel.X, voxel.Y, voxel.Z] = componentIndex;
            if (voxel.Y == 0)
                groundedComponents.Add(componentIndex);
        }

        public bool this[int x, int y, int z]
        {
            get => matrix[x, y, z];
            set
            {
                matrix[x, y, z] = value;
                JoinConnectedComponentsWithVoxel(x, y, z);
            }
        }

        private void JoinConnectedComponentsWithVoxel(int x, int y, int z)
        {
            var voxel = new Vec(x, y, z);
            var ns = voxel.GetMNeighbours().Where(n => this.IsInside(n) && componentIndexes[n.X, n.Y, n.Z] != 0).ToList();
            if (ns.Count == 0)
            {
                AssignComponentIndexTo(voxel, nextComponentIndex);
                componentSize[nextComponentIndex] = 1;
                nextComponentIndex++;
                return;
            }
            var componentPoint = ns.MaxBy(n => componentSize[componentIndexes[n.X, n.Y, n.Z]]).First();
            var componentIndex = componentIndexes[componentPoint.X, componentPoint.Y, componentPoint.Z];
            AssignComponentIndexTo(voxel, componentIndex);
            componentSize[componentIndex]++;
            foreach (var n in ns)
            {
                var componentToKill = componentIndexes[n.X, n.Y, n.Z];
                if (componentToKill != componentIndex)
                {
                    var size = FillComponent(n, componentIndex);
                    componentSize[componentIndex] += size;
                    componentSize.Remove(componentToKill);
                }
            }
        }

        public bool this[Vec pos] { get => matrix[pos]; set => this[pos.X, pos.Y, pos.Z] = value; }

        public int R => matrix.R;
        public bool[,,] Voxels => matrix.Voxels;
    }
}