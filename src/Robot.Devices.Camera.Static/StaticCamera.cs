namespace Robot.Devices.Camera.Static
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class StaticCamera : ICamera
    {
        private readonly IEnumerator<string> _enumerator;

        public StaticCamera(string directory, string filter)
        {
            _enumerator = Directory.EnumerateFiles(directory, filter, SearchOption.AllDirectories).GetEnumerator();
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            _enumerator.Dispose();
            return default;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IPooledBitmap> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            while (!cancellationToken.IsCancellationRequested && _enumerator.MoveNext())
            {
                var bitmap = new Bitmap(_enumerator.Current);
                yield return new NonPooledBitmap(bitmap);
            }
        }

        /// <inheritdoc/>
        public Task<IPooledBitmap> ReadAsync(CancellationToken cancellationToken = default)
        {
            if (!_enumerator.MoveNext())
            {
                return Task.FromException<IPooledBitmap>(new Exception("No more frames available."));
            }

            var bitmap = new Bitmap(_enumerator.Current);
            return Task.FromResult<IPooledBitmap>(new NonPooledBitmap(bitmap));
        }
    }
}
