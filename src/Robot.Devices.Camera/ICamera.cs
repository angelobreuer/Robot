namespace Robot.Devices.Camera
{
    using System;
    using System.Threading.Channels;

    public interface ICamera : IAsyncDisposable
    {
        ChannelReader<IPooledBitmap> FrameReader { get; }
    }
}
