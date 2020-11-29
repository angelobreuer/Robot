namespace Robot.Devices.Camera.MMAL
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using MMALSharp;
    using MMALSharp.Common;

    public sealed class MmalCamera : ICamera
    {
        /// <inheritdoc/>
        public ValueTask DisposeAsync() => default;

        /// <inheritdoc/>
        public async IAsyncEnumerable<IPooledBitmap> ReadAllAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var captureHandler = new MmalCameraOutputCaptureHandler(this);

            while (!cancellationToken.IsCancellationRequested)
            {
                await MMALCamera.Instance.TakePicture(captureHandler, MMALEncoding.BMP, MMALEncoding.RGB24);
                yield return captureHandler.Current;
            }
        }

        /// <inheritdoc/>
        public async Task<IPooledBitmap> ReadAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var captureHandler = new MmalCameraOutputCaptureHandler(this);
            await MMALCamera.Instance.TakePicture(captureHandler, MMALEncoding.BMP, MMALEncoding.RGB24);
            return captureHandler.Current;
        }
    }
}
