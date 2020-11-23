namespace Robot.ObjectRecognition.Data
{
    using System.Drawing;
    using Microsoft.ML.Transforms.Image;

    public sealed class ImageData
    {
        [ImageType(416, 416)]
        public Bitmap Image { get; set; }
    }
}
