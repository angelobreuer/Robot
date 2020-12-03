namespace Robot.Server
{
    using System;
    using System.Numerics;

    public static class RgbComparer
    {
        private const float RefX = 1.8883F;
        private const float RefY = 1F;
        private const float RefZ = 0.95047F;

        public static double Compare(RgbColor color1, RgbColor color2)
        {
            return Vector3.Distance(ToNormalizedVector(color1), ToNormalizedVector(color2)) / 441.6729559300637D;
        }

        public static double CompareByAmplitude(RgbColor color1, RgbColor color2)
        {
            var r = Math.Abs(color1.R - color2.R);
            var g = Math.Abs(color1.G - color2.G);
            var b = Math.Abs(color1.B - color2.B);
            var factor = Math.Min(r, Math.Min(g, b)) / 2.55D;

            return Compare(color1, color2) * factor;
        }

        private static Vector3 ToNormalizedVector(RgbColor color)
        {
            return new Vector3(color.R * RefX, color.G * RefY, color.B * RefZ);
        }
    }
}
