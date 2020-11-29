namespace Robot.Server.Stages.Recognition
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Robot.Devices.Camera;

    internal sealed class RecordingStage : IStage<ICamera, IReadOnlyList<IPooledBitmap>>
    {
        /// <inheritdoc/>
        public async ValueTask<IReadOnlyList<IPooledBitmap>> ProcessAsync(ICamera value, IAsyncStageProgress progress, ILogger logger, CancellationToken cancellationToken = default)
        {
            var images = new List<IPooledBitmap>(capacity: 50);

            // record frames for 5 seconds, with a maximum of 50 frames
            var stopwatch = new Stopwatch();
            stopwatch.Restart();

            await using var providingEnumerator = value.FrameReader
                .ReadAllAsync(cancellationToken)
                .GetAsyncEnumerator(cancellationToken);

            while (await providingEnumerator.MoveNextAsync() && stopwatch.ElapsedMilliseconds < 5000 && images.Count < 50)
            {
                await progress.ReportAsync(
                    progress: images.Count / 50F,
                    status: $"Loading images ({images.Count} / 50, {stopwatch.Elapsed.TotalSeconds:0} of 5s)",
                    cancellationToken: cancellationToken);

                var bitmap = providingEnumerator.Current;
                images.Add(bitmap);

                //await Task.Delay(250, cancellationToken);
            }

            return images;
        }
    }
}
