namespace Robot.Devices.Camera.DirectShow
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using AForge.Video;
    using AForge.Video.DirectShow;

    public sealed class DirectShowCamera : ICamera
    {
        private readonly Channel<IPooledBitmap> _frameChannel;
        private readonly VideoCaptureDevice _source;
        private bool _disposed;

        public DirectShowCamera()
        {
            var options = new BoundedChannelOptions(20)
            {
                SingleReader = true,
                FullMode = BoundedChannelFullMode.DropWrite,
            };

            _frameChannel = Channel.CreateBounded<IPooledBitmap>(options);

            var collection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            _source = new VideoCaptureDevice(collection[0].MonikerString);
            _source.NewFrame += Source_NewFrame;
            _source.Start();
        }

        /// <inheritdoc/>
        public ChannelReader<IPooledBitmap> FrameReader => _frameChannel.Reader;

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return default;
            }

            _disposed = true;

            _source.NewFrame -= Source_NewFrame;
            _source.SignalToStop();
            _source.WaitForStop();

            return default;
        }

        private void Source_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            using (eventArgs.Frame)
            {
                var graphicsUnit = GraphicsUnit.Pixel;
                var frame = eventArgs.Frame.Clone(eventArgs.Frame.GetBounds(ref graphicsUnit), PixelFormat.Format24bppRgb);
                _frameChannel.Writer.TryWrite(new NonPooledBitmap(frame));
            }
        }
    }
}
