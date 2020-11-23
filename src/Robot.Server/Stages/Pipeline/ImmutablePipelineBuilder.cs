namespace Robot.Server.Stages.Pipeline
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.Extensions.DependencyInjection;

    public readonly struct ImmutablePipelineBuilder<TIn, TOut>
    {
        private readonly ImmutableArray<IPipelineItem> _items;

        public ImmutablePipelineBuilder(IServiceProvider serviceProvider, IStage<TIn, TOut> stage)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _items = ImmutableArray.Create<IPipelineItem>(new PipelineItem<TIn, TOut>(stage));
        }

        private ImmutablePipelineBuilder(IServiceProvider serviceProvider, ImmutableArray<IPipelineItem> items)
        {
            _items = items;
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public Pipeline<TIn, TOut> Pipeline => new(_items);

        public IServiceProvider ServiceProvider { get; }

        public ImmutablePipelineBuilder<TIn, TStageOut> Append<TStageOut>(IStage<TOut, TStageOut> stage)
        {
            return new(ServiceProvider, _items.Add(new PipelineItem<TOut, TStageOut>(stage)));
        }

        public ImmutablePipelineBuilder<TIn, TStageOut> Append<TStageOut, TStage>()
            where TStage : IStage<TOut, TStageOut>
            => Append(ServiceProvider.GetRequiredService<TStage>());
    }

    public static class ImmutablePipelineBuilder
    {
        public static ImmutablePipelineBuilder<TIn, TOut> Create<TIn, TOut>(IServiceProvider serviceProvider, IStage<TIn, TOut> stage)
            => new(serviceProvider, stage);

        public static ImmutablePipelineBuilder<TIn, TOut> Create<TIn, TOut, TStage>(IServiceProvider serviceProvider)
            where TStage : IStage<TIn, TOut>
        {
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            return new(serviceProvider, serviceProvider.GetRequiredService<TStage>());
        }
    }
}
