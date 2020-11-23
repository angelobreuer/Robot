namespace Robot.Server.Stages.Pipeline
{
    using System.Threading;
    using System.Threading.Tasks;

    internal interface IPipelineItem
    {
        string Identifier { get; }

        Task<object?> ProcessAsync(object? value, IAsyncStageProgress progress, CancellationToken cancellationToken = default);
    }
}
