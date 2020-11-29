namespace Robot.Server.Streaming
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Robot.Devices.Camera;

    public class VideoStream : IAsyncDisposable
    {
        private readonly Process _process;
        private readonly Task _writeTask;
        private CancellationTokenSource? _cancellationTokenSource;

        public VideoStream(ICamera camera, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.CanBeCanceled)
            {
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            }
            else
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }

            cancellationToken = _cancellationTokenSource.Token;

            _process = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-y -f rawvideo -pix_fmt bgr24 -s 640x480 -r 5 -i - -c:v libvpx -r 27 -streaming 1 -f webm -an -flush_packets 1 -",
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            })!;

            _writeTask = WriteAsync(camera, cancellationToken);
        }

        public void Cancel()
        {
            if (_cancellationTokenSource is null)
            {
                return;
            }

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_cancellationTokenSource is null)
            {
                await JoinAsync();
                return;
            }

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;

            await JoinAsync();
        }

        public Stream GetStream() => _process.StandardOutput.BaseStream;

        public Task JoinAsync() => _writeTask;

        private unsafe void PerformWrite(BitmapData bitmapData)
        {
            var buffer = new ReadOnlySpan<byte>(bitmapData.Scan0.ToPointer(), bitmapData.Height * bitmapData.Stride);
            _process.StandardInput.BaseStream.Write(buffer);
        }

        private async Task WriteAsync(ICamera camera, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var timer = new Stopwatch();
            var framesSent = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                using var frame = await camera.ReadAsync(cancellationToken);
                using var resized = new Bitmap(frame.Bitmap, 640, 480);

                var bounds = new Rectangle(x: 0, y: 0, width: resized.Width, height: resized.Height);
                var data = resized.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                try
                {
                    PerformWrite(data);
                }
                finally
                {
                    resized.UnlockBits(data);
                }

                var frameDelay = framesSent++ * (1000 / 5);
                var pauseTime = frameDelay - timer.ElapsedMilliseconds - 3000;

                if (!timer.IsRunning)
                {
                    timer.Restart();
                    framesSent = 0;
                }

                if (pauseTime > 0)
                {
                    if (pauseTime > 5000)
                    {
                        timer.Stop();
                    }
                    else
                    {
                        Console.WriteLine(pauseTime);
                        await Task.Delay((int)pauseTime, cancellationToken);
                    }
                }
            }
        }
    }
}
