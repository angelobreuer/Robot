namespace Robot.ObjectRecognition
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ML;
    using Microsoft.ML.Data;
    using Robot.ObjectRecognition.Data;

    public class ObjectRecognizer
    {
        public ObjectRecognizer()
        {
            MlContext = new MLContext();
        }

        public MLContext MlContext { get; }

        public static float[][] PredictDataUsingModel(IDataView testData, ITransformer model)
        {
            var scoredData = model.Transform(testData);
            return scoredData.GetColumn<float[]>("grid").ToArray();
        }

        public ITransformer LoadModel(string modelLocation)
        {
            // Create IDataView from empty list to obtain input data schema
            var data = MlContext.Data.LoadFromEnumerable(new List<ImageData>());

            // Define scoring pipeline
            var pipeline = MlContext.Transforms.ExtractPixels(outputColumnName: "image", inputColumnName: nameof(ImageData.Image))
                .Append(MlContext.Transforms.ApplyOnnxModel(modelFile: modelLocation, outputColumnNames: new[] { "grid" }, inputColumnNames: new[] { "image" }));

            // Fit scoring pipeline
            var model = pipeline.Fit(data);

            return model;
        }

        public struct ImageNetSettings
        {
            public const int imageHeight = 416;
            public const int imageWidth = 416;
        }
    }
}
