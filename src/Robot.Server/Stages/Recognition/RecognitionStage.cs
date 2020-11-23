namespace Robot.Server.Stages.Recognition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ML;
    using Microsoft.ML.Data;
    using Robot.Devices.Camera;
    using Robot.ObjectRecognition;
    using Robot.ObjectRecognition.Data;
    using Robot.ObjectRecognition.Parser;

    internal sealed class RecognitionStage : IStage<IReadOnlyList<IPooledBitmap>, IReadOnlyList<IReadOnlyList<YoloBoundingBox>>>
    {
        private readonly ObjectRecognizer _objectRecognizer;
        private readonly YoloOutputParser _outputParser;

        public RecognitionStage(ObjectRecognizer objectRecognizer, YoloOutputParser outputParser)
        {
            _objectRecognizer = objectRecognizer ?? throw new ArgumentNullException(nameof(objectRecognizer));
            _outputParser = outputParser ?? throw new ArgumentNullException(nameof(outputParser));
        }

        /// <inheritdoc/>
        public async ValueTask<IReadOnlyList<IReadOnlyList<YoloBoundingBox>>> ProcessAsync(IReadOnlyList<IPooledBitmap> bitmaps, IAsyncStageProgress progress, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ITransformer onnxTransformer;
            using (await progress.OpenScopeAsync("Loading ONNX model", cancellationToken))
            {
                onnxTransformer = _objectRecognizer.LoadModel("TinyYolo2_model.onnx");
            }

            IReadOnlyList<IReadOnlyList<YoloBoundingBox>> boundingBoxes;
            using (await progress.OpenScopeAsync("Analyzing data", cancellationToken))
            {
                var data = bitmaps.Select(x => new ImageData { Image = x.Bitmap });
                var dataView = _objectRecognizer.MlContext.Data.LoadFromEnumerable(new[] { data.First() }); // TODO: remove first

                boundingBoxes = onnxTransformer
                    .Transform(dataView)
                    .GetColumn<float[]>("grid")
                    .Select(x => _outputParser.ParseOutputs(x))
                    .ToList();

                foreach (var bitmap in bitmaps)
                {
                    bitmap.Dispose();
                }
            }

            return boundingBoxes;
        }
    }
}
