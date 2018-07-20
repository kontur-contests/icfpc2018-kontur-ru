using JetBrains.Annotations;

namespace lib.Models
{
    public class Model
    {
        private readonly bool[,,] voxels;

        public Model([NotNull] bool[,,] voxels)
        {
            this.voxels = voxels;
            R = voxels.GetLength(0);
        }

        public Model([NotNull] params string[] zLayers)
        {
            R = zLayers.Length;
            voxels = new bool[R, R, R];
            for (int x = 0; x < R; x++)
                for (int y = 0; y < R; y++)
                    for (int z = 0; z < R; z++)
                    {
                        var yLayers = zLayers[z].Split('|');
                        voxels[x, y, z] = yLayers[y][x] == '1';
                    }
        }

        public int R { get; }

        [NotNull]
        public static Model Load([NotNull] byte[] content)
        {
            var r = content[0];
            var voxels = new bool[r, r, r];
            var bit = 0;
            for (int x = 0; x < r; x++)
                for (int y = 0; y < r; y++)
                    for (int z = 0; z < r; z++)
                    {
                        byte b = content[1 + bit / 8];
                        bool isFull = (b >> (bit % 8) & 1) == 1;
                        voxels[x, y, z] = isFull;
                        bit++;
                    }
            return new Model(voxels);
        }

        [NotNull]
        public byte[] Save()
        {
            var content = new byte[1 + (R * R * R + 7) / 8];
            content[0] = (byte)R;
            var bit = 0;
            for (int x = 0; x < R; x++)
                for (int y = 0; y < R; y++)
                    for (int z = 0; z < R; z++)
                    {
                        bool isFull = voxels[x, y, z];
                        if (isFull)
                            content[1 + bit / 8] |= (byte)(1 << (bit % 8));
                        bit++;
                    }
            return content;
        }

        public bool this[[NotNull] Vec coord] { get => voxels[coord.X, coord.Y, coord.Z]; set => voxels[coord.X, coord.Y, coord.Z] = value; }

        public bool this[int x, int y, int z] { get => voxels[x, y, z]; set => voxels[x, y, z] = value; }
    }
}