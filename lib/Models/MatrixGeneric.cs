using JetBrains.Annotations;

using lib.Utils;

namespace lib.Models
{
    public class Matrix<T>
    {
        private readonly T[,,] voxels;

        public Matrix(int n)
            : this(new T[n, n, n])
        {
        }


        public Matrix([NotNull] T[,,] voxels)
        {
            this.voxels = voxels;
            R = voxels.GetLength(0);

        }
        
        public int R { get; }
        public int N => R;

        public T this[[NotNull] Vec coord] { get => voxels[coord.X, coord.Y, coord.Z]; set => voxels[coord.X, coord.Y, coord.Z] = value; }

        public T this[int x, int y, int z] { get => voxels[x, y, z]; set => voxels[x, y, z] = value; }

        public T[,,] Voxels => this.voxels;

        public Matrix<T> Clone()
        {
            return new Matrix<T>((T[,,])voxels.Clone());
        }
    }
}