namespace Robot.Devices.Camera.DirectShow
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using AForge.Video;
    using AForge.Video.DirectShow;

    public sealed class DirectShowCamera : ICamera
    {
        private readonly SemaphoreSlim _semaphoreSlim;
        private IPooledBitmap? _bitmap;
        private TaskCompletionSource<IPooledBitmap>? _taskCompletionSource;
        private VideoCaptureDevice? _videoCaptureDevice;

        public DirectShowCamera()
        {
            var monikerString = new FilterInfoCollection(FilterCategory.VideoInputDevice)[0].MonikerString;

            _semaphoreSlim = new SemaphoreSlim(1, 1);

            _videoCaptureDevice = new VideoCaptureDevice(monikerString);
            _videoCaptureDevice.Start();
            _videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_videoCaptureDevice is null)
            {
                return;
            }

            _semaphoreSlim.Dispose();

            await Task.Run(() =>
            {
                _videoCaptureDevice.SignalToStop();
                _videoCaptureDevice.WaitForStop();
                _videoCaptureDevice = null;
            });
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IPooledBitmap> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            if (_videoCaptureDevice is null)
            {
                yield break;
            }

            cancellationToken.ThrowIfCancellationRequested();

            // acquire lock
            await _semaphoreSlim.WaitAsync(cancellationToken);

            // ensure the lock is released
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _bitmap?.Dispose();
                    var taskCompletionSource = _taskCompletionSource = new TaskCompletionSource<IPooledBitmap>();
                    yield return await taskCompletionSource.Task;
                }
            }
            finally
            {
                // release lock
                _semaphoreSlim.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<IPooledBitmap> ReadAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // acquire lock
            await _semaphoreSlim.WaitAsync(cancellationToken);

            // ensure the lock is released
            try
            {
                _bitmap?.Dispose();
                var taskCompletionSource = _taskCompletionSource = new TaskCompletionSource<IPooledBitmap>();
                return await taskCompletionSource.Task;
            }
            finally
            {
                // release lock
                _semaphoreSlim.Release();
            }
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            using (eventArgs.Frame)
            {
                if (_taskCompletionSource is not null)
                {
                    _bitmap = new NonPooledBitmap((Bitmap)eventArgs.Frame.Clone());
                    _taskCompletionSource?.TrySetResult(_bitmap);
                }
            }
        }
    }
}
