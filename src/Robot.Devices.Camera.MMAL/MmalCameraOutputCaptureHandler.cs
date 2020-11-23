namespace Robot.Devices.Camera.MMAL
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using MMALSharp.Common;
    using MMALSharp.Handlers;

    public sealed class MmalCameraOutputCaptureHandler : IOutputCaptureHandler
    {
        private readonly MmalCamera _camera;
        private int _imagesProcessed;

        public MmalCameraOutputCaptureHandler(MmalCamera camera)
        {
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public void PostProcess()
        {
        }

        /// <inheritdoc/>
        public unsafe void Process(ImageContext context)
        {
            var length = (context.Resolution.Width * 3) + context.Resolution.Height;
            var buffer = GC.AllocateUninitializedArray<byte>(length, pinned: true);

            var typedReferenceToBuffer = __makeref(buffer);
            var bufferPtr = **(IntPtr**)&typedReferenceToBuffer;

            fixed (byte* sourcePtr = context.Data)
            {
                Buffer.MemoryCopy(sourcePtr, bufferPtr.ToPointer(), length, context.Data.Length);
            }

            var bitmap = new Bitmap(
                width: context.Resolution.Width,
                height: context.Resolution.Height,
                stride: 3 * 3 * context.Resolution.Width,
                format: PixelFormat.Format24bppRgb,
                scan0: bufferPtr);

            _camera.EnqueueBitmap(new NativePooledBitmap(bitmap, buffer));
            _imagesProcessed++;
        }

        /// <inheritdoc/>
        public string TotalProcessed()
        {
            return $"{_imagesProcessed} processed.";
        }
    }
}
