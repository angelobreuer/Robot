namespace Robot.ObjectRecognition.Data
{
    using Microsoft.ML.Data;

    public sealed class ObjectPrediction
    {
        [ColumnName("grid")]
        public float[] Grid;
    }
}
