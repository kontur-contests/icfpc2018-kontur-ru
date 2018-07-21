using JetBrains.Annotations;

using lib.Utils;

namespace lib.Models
{
    public class MatrixInt
    {
        private readonly int[,,] voxels;

        public MatrixInt(int n)
            : this(new int[n, n, n])
        {
        }


        public MatrixInt([NotNull] int[,,] voxels)
        {
            this.voxels = voxels;
            R = voxels.GetLength(0);

        }
        
        public int R { get; }
        public int N => R;

        public int this[[NotNull] Vec coord] { get => voxels[coord.X, coord.Y, coord.Z]; set => voxels[coord.X, coord.Y, coord.Z] = value; }

        public int this[int x, int y, int z] { get => voxels[x, y, z]; set => voxels[x, y, z] = value; }

        public int[,,] Voxels => this.voxels;

        public MatrixInt Clone()
        {
            return new MatrixInt((int[,,])voxels.Clone());
        }
    }
}