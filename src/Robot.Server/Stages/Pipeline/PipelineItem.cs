namespace Robot.Server.Stages.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class PipelineItem<TItemIn, TItemOut> : IPipelineItem
    {
        private readonly IStage<TItemIn, TItemOut> _stage;

        public PipelineItem(IStage<TItemIn, TItemOut> stage)
        {
            _stage = stage ?? throw new ArgumentNullException(nameof(stage));
        }

        /// <inheritdoc/>
        public string Identifier => _stage.GetType().FullName!;

        /// <inheritdoc/>
        public async Task<object?> ProcessAsync(object? value, IAsyncStageProgress progress, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _stage.ProcessAsync((TItemIn)value!, progress, cancellationToken);
        }
    }
}
