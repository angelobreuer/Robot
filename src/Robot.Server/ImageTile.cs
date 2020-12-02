namespace Robot.Server
{
    using System;
    using System.Drawing;
    using System.Numerics;

    public readonly struct ImageTile
    {
        private readonly ImageGrid _grid;

        public ImageTile(ImageGrid grid, int x, int y)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            X = x;
            Y = y;
        }

        public Rectangle Bounds => new(X, Y, 1, 1);

        public int Offset => ((Y * _grid.Width) + X) * _grid.TileSize;

        public Rectangle OriginalBounds => new(X * _grid.TileSize, Y * _grid.TileSize, _grid.TileSize, _grid.TileSize);

        public int Size => _grid.TileSize;

        public int SizeSquared => Size * Size;

        public int X { get; }

        public int Y { get; }

        public ref RgbColor this[int x, int y]
        {
            get
            {
                if (x >= Size || y >= Size || x < 0 || y < 0)
                {
                    throw new IndexOutOfRangeException();
                }

                return ref _grid.Data[Offset + (y * _grid.Width) + x];
            }
        }

        public RgbColor Average()
        {
            var r = 0UL;
            var g = 0UL;
            var b = 0UL;
            var offset = Offset;
            var buffer = _grid.Data;

            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var color = buffer[offset++];

                    r += color.R;
                    g += color.G;
                    b += color.B;
                }

                offset += _grid.Width - Size;
            }

            var sizeSquared = (ulong)SizeSquared;

            return new RgbColor(
                r: (byte)(r / sizeSquared),
                g: (byte)(g / sizeSquared),
                b: (byte)(b / sizeSquared));
        }

        public unsafe Vector3 Sum()
        {
            var offset = Offset;
            var buffer = _grid.Data;
            var vector = new Vector3();

            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var color = buffer[offset++];

                    vector.X += color.R;
                    vector.Y += color.G;
                    vector.Z += color.B;
                }

                offset += _grid.Width - Size;
            }

            return vector;
        }
    }
}
