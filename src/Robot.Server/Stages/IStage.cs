namespace Robot.Server.Stages
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IStage<TInterceptionElement> : IStage<TInterceptionElement, TInterceptionElement>
    {
    }

    public interface IStage<TIn, TOut>
    {
        ValueTask<TOut> ProcessAsync(TIn value, IAsyncStageProgress progress, CancellationToken cancellationToken = default);
    }
}
