namespace Robot.Server.Stages.Pipeline
{
    using System.Collections.Immutable;
    using System.Threading;
    using System.Threading.Tasks;

    public readonly struct Pipeline<TIn, TOut> : IStage<TIn, TOut>
    {
        private readonly ImmutableArray<IPipelineItem> _items;

        internal Pipeline(ImmutableArray<IPipelineItem> items)
        {
            _items = items;
        }

        internal Pipeline(IStage<TIn, TOut> stage)
        {
            _items = ImmutableArray.Create<IPipelineItem>(new PipelineItem<TIn, TOut>(stage));
        }

        public Pipeline<TIn, TStageOut> Append<TStageOut>(IStage<TOut, TStageOut> stage)
        {
            return new Pipeline<TIn, TStageOut>(_items.Add(new PipelineItem<TOut, TStageOut>(stage)));
        }

        public async ValueTask<TOut> ProcessAsync(TIn value, IAsyncStageProgress progress, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentValue = (object?)value;

            foreach (var item in _items)
            {
                using (await progress.OpenScopeAsync(item.Identifier, cancellationToken))
                {
                    currentValue = await item.ProcessAsync(currentValue, progress, cancellationToken);
                }
            }

            return (TOut)currentValue!;
        }

        /// <inheritdoc/>
        ValueTask<TOut> IStage<TIn, TOut>.ProcessAsync(TIn value, IAsyncStageProgress progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ProcessAsync(value, progress, cancellationToken);
        }
    }

    public static class Pipeline
    {
        public static Pipeline<TIn, TOut> Create<TIn, TOut>(IStage<TIn, TOut> stage) => new(stage);
    }
}
