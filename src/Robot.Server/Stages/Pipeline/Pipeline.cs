namespace Robot.Server.Stages.Pipeline
{
    using System;
    using System.Collections.Immutable;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public readonly struct Pipeline<TIn, TOut>
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

        public async ValueTask<TOut> ProcessAsync(TIn value, IAsyncStageProgress progress, ILoggerFactory loggerFactory, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentValue = (object?)value;
            var pipelineLogger = loggerFactory.CreateLogger<Pipeline<TIn, TOut>>();

            pipelineLogger.LogDebug("Starting pipeline with value: {Value}.", currentValue);

            foreach (var item in _items)
            {
                var startTime = DateTimeOffset.Now;

                pipelineLogger.LogDebug("Started stage {Identifier} at {Time}.", item.Identifier, startTime);

                using (await progress.OpenScopeAsync(item.Identifier, cancellationToken))
                {
                    var logger = loggerFactory.CreateLogger(item.Identifier);
                    currentValue = await item.ProcessAsync(currentValue, progress, logger, cancellationToken);
                }

                pipelineLogger.LogDebug("Completed stage {Identifier} (took {Time}ms): {Value}.",
                    item.Identifier, (DateTimeOffset.Now - startTime).TotalMilliseconds, currentValue);
            }

            pipelineLogger.LogDebug("Completed pipeline with value: {Value}.", currentValue);

            return (TOut)currentValue!;
        }
    }

    public static class Pipeline
    {
        public static Pipeline<TIn, TOut> Create<TIn, TOut>(IStage<TIn, TOut> stage) => new(stage);
    }
}
