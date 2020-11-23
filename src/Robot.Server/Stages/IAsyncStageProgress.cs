namespace Robot.Server.Stages
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAsyncStageProgress
    {
        ValueTask<IDisposable> OpenScopeAsync(string name, CancellationToken cancellationToken = default);

        ValueTask ReportAsync(float progress, string status, CancellationToken cancellationToken = default);
    }
}
