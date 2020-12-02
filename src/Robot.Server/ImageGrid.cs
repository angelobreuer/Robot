namespace Robot.Server
{
    using System;
    using System.Buffers;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.CompilerServices;

    public sealed class ImageGrid : IEnumerable<ImageTile>, IDisposable
    {
        private readonly ArrayPool<RgbColor>? _arrayPool;
        private RgbColor[]? _data;

        public ImageGrid(RgbColor[] data, int tilesX, int tilesY, int tileSize, ArrayPool<RgbColor>? arrayPool = null)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _arrayPool = arrayPool;

            TilesX = tilesX;
            TilesY = tilesY;
            TileSize = tileSize;
        }

        public RgbColor[] Data
        {
            get
            {
                EnsureNotDisposed();
                return _data;
            }
        }

        public int Height => TilesY * TileSize;

        public int Tiles => TilesX * TilesY;

        public int TileSize { get; }

        public int TilesX { get; }

        public int TilesY { get; }

        public int Width => TilesX * TileSize;

        public ImageTile this[int x, int y]
        {
            get
            {
                EnsureNotDisposed();

                if (x >= TilesX || y >= TilesY || x < 0 || y < 0)
                {
                    throw new IndexOutOfRangeException();
                }

                return new ImageTile(this, x, y);
            }
        }

        public ImageTile this[int index]
        {
            get
            {
                EnsureNotDisposed();

                if (index < 0 || index >= Tiles)
                {
                    throw new IndexOutOfRangeException();
                }

                var y = Math.DivRem(index, TilesX, out var x);
                return new ImageTile(this, x, y);
            }
        }

        public unsafe static ImageGrid FromBitmap(Bitmap bitmap, int tileSize)
        {
            if (bitmap.Width % tileSize is not 0 || bitmap.Height % tileSize is not 0)
            {
                throw new InvalidOperationException("Image width and height must be divisible through the specified tile size.");
            }

            var bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);

            try
            {
                var buffer = GC.AllocateUninitializedArray<RgbColor>(data.Width * data.Height);

                fixed (byte* bufferPtr = &Unsafe.As<RgbColor, byte>(ref buffer[0]))
                {
                    var totalLength = data.Stride * data.Height;
                    Buffer.MemoryCopy(data.Scan0.ToPointer(), bufferPtr, totalLength, totalLength);
                }

                return new ImageGrid(buffer, bitmap.Width / tileSize, bitmap.Height / tileSize, tileSize);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        public unsafe static Bitmap ToBitmap(ImageGrid grid)
        {
            grid.EnsureNotDisposed();

            var bitmap = new Bitmap(grid.Width, grid.Height, PixelFormat.Format32bppArgb);
            var bounds = new Rectangle(0, 0, grid.Width, grid.Height);
            var data = bitmap.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            try
            {
                fixed (byte* inputPtr = &Unsafe.As<RgbColor, byte>(ref grid._data[0]))
                {
                    Unsafe.CopyBlock(data.Scan0.ToPointer(), inputPtr, (uint)(data.Stride * data.Height));
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_data is null)
            {
                return;
            }

            _arrayPool?.Return(_data);
            _data = null;
        }

        /// <inheritdoc/>
        public IEnumerator<ImageTile> GetEnumerator() => new TileEnumerator(this);

        public ref RgbColor GetPixel(int x, int y)
        {
            EnsureNotDisposed();

            if (x >= Width || y >= Height || x < 0 || y < 0)
            {
                throw new IndexOutOfRangeException();
            }

            return ref _data[(y * Width) + x];
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => new TileEnumerator(this);

        [MemberNotNull(nameof(_data))]
        private void EnsureNotDisposed()
        {
            if (_data is null)
            {
                throw new ObjectDisposedException(nameof(ImageGrid));
            }
        }

        private sealed class TileEnumerator : IEnumerator<ImageTile>
        {
            private readonly ImageGrid _tiles;
            private int _index;

            public TileEnumerator(ImageGrid tiles)
            {
                _tiles = tiles ?? throw new ArgumentNullException(nameof(tiles));
            }

            /// <inheritdoc/>
            public ImageTile Current
            {
                get
                {
                    var y = Math.DivRem(_index, _tiles.TilesX, out var x);
                    return new ImageTile(_tiles, x, y);
                }
            }

            /// <inheritdoc/>
            object IEnumerator.Current => Current;

            /// <inheritdoc/>
            public void Dispose()
            {
            }

            /// <inheritdoc/>
            public bool MoveNext()
            {
                var newIndex = _index + 1;

                if (newIndex >= _tiles.Tiles)
                {
                    return false;
                }

                _index = newIndex;
                return true;
            }

            /// <inheritdoc/>
            public void Reset()
            {
                _index = 0;
            }
        }
    }
}
