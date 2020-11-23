namespace Robot.Server.Stages.Recognition
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Robot.Devices.Camera;

    internal sealed class ScalingStage : IStage<IReadOnlyList<IPooledBitmap>, IReadOnlyList<IPooledBitmap>>
    {
        /// <inheritdoc/>
        public ValueTask<IReadOnlyList<IPooledBitmap>> ProcessAsync(IReadOnlyList<IPooledBitmap> value, IAsyncStageProgress progress, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const int DesiredWidth = 416;
            const int DesiredHeight = 416;

            var bitmaps = new List<IPooledBitmap>();

            foreach (var bitmap in value)
            {
                if (bitmap.Bitmap.Width == DesiredWidth && bitmap.Bitmap.Height == DesiredHeight)
                {
                    // pass-through
                    bitmaps.Add(bitmap);
                }
                else
                {
                    // rescale
                    var resizedBitmap = new Bitmap(bitmap.Bitmap, DesiredWidth, DesiredHeight);
                    bitmaps.Add(new NonPooledBitmap(resizedBitmap));
                    // TODO: dispose old
                }
            }

            return new ValueTask<IReadOnlyList<IPooledBitmap>>(bitmaps);
        }
    }
}
