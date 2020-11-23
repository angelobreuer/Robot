namespace Robot.Devices.Camera.MMAL
{
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using MMALSharp;
    using MMALSharp.Common;

    public sealed class MmalCamera : ICamera
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Channel<IPooledBitmap> _frameChannel;
        private bool _disposed;

        public MmalCamera()
        {
            var options = new BoundedChannelOptions(20)
            {
                SingleReader = true,
                FullMode = BoundedChannelFullMode.DropWrite,
            };

            _frameChannel = Channel.CreateBounded<IPooledBitmap>(options);
            _cancellationTokenSource = new CancellationTokenSource();
            _ = RunCaptureAsync();
        }

        /// <inheritdoc/>
        public ChannelReader<IPooledBitmap> FrameReader => _frameChannel.Reader;

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _frameChannel.Writer.Complete();

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            await foreach (var frame in _frameChannel.Reader.ReadAllAsync())
            {
                frame.Dispose();
            }
        }

        internal void EnqueueBitmap(IPooledBitmap pooledBitmap)
        {
            if (!_frameChannel.Writer.TryWrite(pooledBitmap))
            {
                // drop
                pooledBitmap.Dispose();
            }
        }

        private async Task RunCaptureAsync()
        {
            using var captureHandler = new MmalCameraOutputCaptureHandler(this);
            var cancellationToken = _cancellationTokenSource.Token;

            while (!cancellationToken.IsCancellationRequested)
            {
                await MMALCamera.Instance.TakePicture(captureHandler, MMALEncoding.BMP, MMALEncoding.RGB24);
            }
        }
    }
}
