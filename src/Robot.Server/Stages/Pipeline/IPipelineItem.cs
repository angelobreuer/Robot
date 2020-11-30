namespace Robot.Server.Stages.Pipeline
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    internal interface IPipelineItem
    {
        string Identifier { get; }

        Task<object?> ProcessAsync(object? value, IAsyncStageProgress progress, ILogger logger, CancellationToken cancellationToken = default);
    }
}
