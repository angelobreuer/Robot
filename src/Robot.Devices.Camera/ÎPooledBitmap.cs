namespace Robot.Devices.Camera
{
    using System;
    using System.Drawing;

    public interface IPooledBitmap : IDisposable
    {
        Bitmap Bitmap { get; }
    }
}
