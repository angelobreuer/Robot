namespace Robot.Server.Geolocation
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    public sealed class RingLocator
    {
        private readonly List<SequenceCandidate>[] _candidates;
        private readonly MotionDetector _motionDetector;
        private readonly RingSequence[] _sequences;
        private readonly int _stepCount;
        private readonly int _stepSize;
        private int _timeOffset;

        public RingLocator(IEnumerable<RingSequence> sequences)
        {
            _motionDetector = new MotionDetector();
            _sequences = sequences.ToArray();
            _stepSize = _sequences.SelectMany(x => x.Timings).Aggregate(GetGreatestCommonDivisor);
            _stepCount = _sequences.SelectMany(x => x.Timings).Sum() / _stepSize;

            _candidates = new List<SequenceCandidate>[_sequences.Length * _stepCount];

            for (var index = 0; index < _candidates.Length; index++)
            {
                _candidates[index] = new List<SequenceCandidate>();
            }
        }

        public List<SequenceCandidate>[] CAnd => _candidates;

        public static int GetGreatestCommonDivisor(int x, int y)
        {
            var x2 = x;
            var y2 = y;

            while (x2 % y2 is not 0)
            {
                var result = x2 % y2;
                x2 = y2;
                y2 = result;
            }

            return y2;
        }

        public void AddCandidate(RgbColor color, RingSequence ringSequence, int relativeOffset, Rectangle bounds)
        {
            var index = GetStepIndex(relativeOffset);
            var stepOffset = (index * _stepSize) - relativeOffset;
            var candidate = new SequenceCandidate(color, ringSequence, stepOffset, bounds);
            _candidates[index].Add(candidate);
        }

        public bool Analyze(ImageGrid grid, int frameDelay)
        {
            var tiles = _motionDetector.DetectChanges(grid);
            var relativeDelay = frameDelay - _timeOffset;

            if (tiles.IsDefaultOrEmpty)
            {
                // more data is needed
                return false;
            }

            _timeOffset = frameDelay;

            foreach (var tile in tiles)
            {
                var averagedColor = tile.Average();
                var sequence = _sequences.OrderBy(x => GetDistance(x.Color, averagedColor)).First();
                AddCandidate(averagedColor, sequence, relativeDelay, tile.OriginalBounds);
            }

            return true;
        }

        public int GetStepIndex(int offset)
        {
            var windowSize = _stepCount * _stepSize;
            var stepOffset = offset % windowSize;
            return (int)MathF.Floor(stepOffset / (float)_stepSize);
        }

        private static float GetDistance(RgbColor color1, RgbColor color2)
        {
            var dr = MathF.Sqrt(MathF.Pow(color1.R, 2) + MathF.Pow(color2.R, 2));
            var dg = MathF.Sqrt(MathF.Pow(color1.G, 2) + MathF.Pow(color2.G, 2));
            var db = MathF.Sqrt(MathF.Pow(color1.B, 2) + MathF.Pow(color2.B, 2));

            return (dr + dg + db) / 3F;
        }

        public readonly struct SequenceCandidate
        {
            public SequenceCandidate(RgbColor color, RingSequence sequence, int relativeOffset, Rectangle bounds)
            {
                Color = color;
                Sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
                RelativeOffset = relativeOffset;
                Bounds = bounds;
            }

            public Rectangle Bounds { get; }

            public RgbColor Color { get; }

            public int RelativeOffset { get; }

            public RingSequence Sequence { get; }
        }
    }
}
