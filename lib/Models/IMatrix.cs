using JetBrains.Annotations;

using lib.Utils;

namespace lib.Models
{
    public interface IMatrix
    {
        bool this[int x, int y, int z] { get; set; }
        bool this[Vec pos] { get; set; }

        int R { get; }

        bool[,,] Voxels { get; }
    }

    public static class MatrixExtensions
    {
        public static bool IsInside(this IMatrix matrix, Vec v)
        {
            return
                0 <= v.X && v.X < matrix.R &&
                0 <= v.Y && v.Y < matrix.R &&
                0 <= v.Z && v.Z < matrix.R;
        }
        
        public static bool IsFilledVoxel(this IMatrix matrix, [NotNull] Vec vec)
        {
            return matrix[vec] == true;
        }
        
        public static bool IsVoidVoxel(this IMatrix matrix, [NotNull] Vec vec)
        {
            return matrix[vec] == false;
        }

        public static void Fill(this IMatrix matrix, [NotNull] Vec vec)
        {
            matrix[vec] = true;
        }

        public static void Void(this IMatrix matrix, [NotNull] Vec vec)
        {
            matrix[vec] = false;
        }
    }
}