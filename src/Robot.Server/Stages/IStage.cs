namespace Robot.Server.Stages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public interface IStage<TInterceptionElement> : IStage<TInterceptionElement, TInterceptionElement>
    {
    }

    public interface IStage<TIn, TOut>
    {
        ValueTask<TOut> ProcessAsync(TIn value, IAsyncStageProgress progress, ILogger logger, CancellationToken cancellationToken = default);
    }
}
