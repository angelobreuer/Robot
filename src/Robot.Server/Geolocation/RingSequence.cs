namespace Robot.Server.Geolocation
{
    using System;
    using System.Collections.Generic;

    public sealed class RingSequence
    {
        public RingSequence(string name, RgbColor color, int tolerance, IReadOnlyList<int> timings)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Color = color;
            Tolerance = tolerance;
            Timings = timings;
        }

        public RgbColor Color { get; }

        public string Name { get; }

        public IReadOnlyList<int> Timings { get; }

        public int Tolerance { get; }
    }
}
