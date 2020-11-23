namespace Robot.ObjectRecognition
{
    using System.Drawing;

    public class YoloBoundingBox
    {
        public Color BoxColor { get; set; }

        public float Confidence { get; set; }

        public BoundingBoxDimensions Dimensions { get; set; }

        public string Label { get; set; }

        public RectangleF Rect
        {
            get { return new RectangleF(Dimensions.X, Dimensions.Y, Dimensions.Width, Dimensions.Height); }
        }
    }
}
