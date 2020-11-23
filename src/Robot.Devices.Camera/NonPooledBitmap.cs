namespace Robot.Devices.Camera
{
    using System;
    using System.Drawing;

    public sealed class NonPooledBitmap : IPooledBitmap
    {
        public NonPooledBitmap(Bitmap bitmap)
        {
            Bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
        }

        /// <inheritdoc/>
        public Bitmap Bitmap { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Bitmap.Dispose();
        }
    }
}
