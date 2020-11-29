namespace Robot.Devices.Camera
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICamera : IAsyncDisposable
    {
        IAsyncEnumerable<IPooledBitmap> ReadAllAsync(CancellationToken cancellationToken = default);

        Task<IPooledBitmap> ReadAsync(CancellationToken cancellationToken = default);
    }
}
