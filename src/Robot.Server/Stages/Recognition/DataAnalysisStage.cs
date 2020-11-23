namespace Robot.Server.Stages.Recognition
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Robot.ObjectRecognition;

    internal sealed class DataAnalysisStage : IStage<IReadOnlyList<IReadOnlyList<YoloBoundingBox>>, IReadOnlyList<TableBoundary>>
    {
        /// <inheritdoc/>
        public async ValueTask<IReadOnlyList<TableBoundary>> ProcessAsync(IReadOnlyList<IReadOnlyList<YoloBoundingBox>> value, IAsyncStageProgress progress, CancellationToken cancellationToken = default)
        {
            // TODO: use whole data
            return value[0].Select(x => new TableBoundary(
                x: (int)x.Dimensions.X,
                y: (int)x.Dimensions.Y,
                width: (int)x.Dimensions.Width,
                height: (int)x.Dimensions.Height)).ToArray();
        }
    }
}
