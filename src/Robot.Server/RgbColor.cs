namespace Robot.Server
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public readonly struct RgbColor
    {
        [FieldOffset(0)]
        public readonly byte R;

        [FieldOffset(1)]
        public readonly byte G;

        [FieldOffset(2)]
        public readonly byte B;

        public RgbColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public float Intensity
        {
            get
            {
                return (int)MathF.Round((.299F * R) + (.587F * G) + (.114F * B));
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{{{R}, {G}, {B}}}";
        }
    }
}
