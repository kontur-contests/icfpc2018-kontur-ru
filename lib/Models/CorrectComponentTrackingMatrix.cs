using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using lib.Utils;

namespace lib.Models
{
    public class CorrectComponentTrackingMatrix : IMatrix
    {
        private Vec GetGroundedOrSelf(Vec vec)
        {
            if (vec.Y == 0)
                return vec;
            return vec.GetMNeighbours().FirstOrDefault(p => p.IsInCuboid(R) && isGrounded.Get(p));
        }

        public int filledCellsCount;
        public int groundedCellsCount;

        public bool this[int x, int y, int z]
        {
            get => Voxels[x, y, z];
            set
            {
                if (value && !Voxels[x, y, z])
                {
                    filledCellsCount++;
                    var groundedNeigh = GetGroundedOrSelf(new Vec(x, y, z));
                    if (groundedNeigh == null)
                    {
                        Voxels[x, y, z] = true;
                        return;
                    }
                    Ground(new Vec(x, y, z), groundedNeigh);
                    Bfs(new List<Vec> {new Vec(x, y, z)});
                    
                }
                else if (!value && Voxels[x, y, z])
                {
                    filledCellsCount--;
                    var unknownCells = GetUnknownCells(new Vec(x, y, z));
                    foreach (var cell in unknownCells)
                        Unground(cell);
                    var groundedCells = unknownCells.Select(GetGroundedOrSelf).Where(cell => cell != null).ToList();
                    Bfs(groundedCells);
                }
                Voxels[x, y, z] = value;
            }
        }

        private List<Vec> GetChildCells(Vec vec)
        {
            return vec.GetMNeighbours().Where(p => p.IsInCuboid(R)).Where(p => parentCell.Get(p) == vec && Voxels.Get(p)).ToList();
        }

        private List<Vec> GetUnknownCells(Vec vec)
        {
            var used = new HashSet<Vec>(new[] {vec});
            var queue = new Queue<Vec>(new[] {vec});
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var next in GetChildCells(current))
                {
                    if (!used.Contains(next))
                    {
                        used.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }
            return used.ToList();
        }

        public bool this[Vec pos] { get => this[pos.X, pos.Y, pos.Z]; set => this[pos.X, pos.Y, pos.Z] = value; }

        public int R => Voxels.GetLength(0);
        public bool[,,] Voxels { get; }
        private Vec[,,] parentCell;
        private readonly bool[,,] isGrounded;
        public bool HasNonGroundedVoxels => groundedCellsCount != filledCellsCount;

        public CorrectComponentTrackingMatrix(Matrix matrix)
        {
            Voxels = matrix.Voxels;
            parentCell = new Vec[R, R, R];
            isGrounded = new bool[R, R, R];

            var groundCells = new List<Vec>();

            for (var x = 0; x < R; x++)
                for (var z = 0; z < R; z++)
                {
                    if (Voxels[x, 0, z])
                    {
                        Ground(new Vec(x, 0, z), null);
                        groundCells.Add(new Vec(x, 0, z));
                    }
                    for (var y = 0; y < R; y++)
                        if (Voxels[x, y, z])
                            filledCellsCount++;
                }
            Bfs(groundCells);
        }

        private void Bfs([NotNull] List<Vec> startPositions)
        {
            var queue = new Queue<Vec>(startPositions);
            while (queue.Count > 0)
            {
                var position = queue.Dequeue();
                foreach (var nextPosition in position.GetMNeighbours().Where(p => p.IsInCuboid(R)))
                {
                    if (Voxels.Get(nextPosition) && !isGrounded.Get(nextPosition))
                    {
                        queue.Enqueue(nextPosition);
                        Ground(nextPosition, position);
                    }
                }
            }
        }

        public void Ground(Vec vec, Vec groundParent)
        {
            if (isGrounded.Get(vec))
                return;
            isGrounded.Set(vec, true);
            parentCell.Set(vec, groundParent);
            groundedCellsCount++;
        }

        public void Unground(Vec vec)
        {
            if (!isGrounded.Get(vec))
                return;
            isGrounded.Set(vec, false);
            parentCell.Set(vec, null);
            groundedCellsCount--;
        }
    }
}