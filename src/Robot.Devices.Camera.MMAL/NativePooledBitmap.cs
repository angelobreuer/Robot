namespace Robot.Devices.Camera.MMAL
{
    using System;
    using System.Drawing;

    internal sealed class NativePooledBitmap : IPooledBitmap
    {
        public NativePooledBitmap(Bitmap bitmap, byte[] buffer)
        {
            Bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
            Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public Bitmap Bitmap { get; private set; }

        public byte[] Buffer { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Bitmap?.Dispose();
            Bitmap = null!;
            Buffer = null!;
        }
    }
}
