namespace Robot.Server.Geolocation
{
    using System.Collections.Generic;

    public sealed class RingSequence
    {
        public RingSequence(RgbColor color, int tolerance, IReadOnlyList<int> timings)
        {
            Color = color;
            Tolerance = tolerance;
            Timings = timings;
        }

        public RgbColor Color { get; }

        public IReadOnlyList<int> Timings { get; }

        public int Tolerance { get; }
    }
}
