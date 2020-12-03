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

        public void AddCandidate(RingSequence ringSequence, int relativeOffset, Rectangle bounds)
        {
            var index = GetStepIndex(relativeOffset);
            var stepOffset = (index * _stepSize) - relativeOffset;
            var candidate = new SequenceCandidate(ringSequence, stepOffset, bounds);
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

            foreach (var sequence in tiles.GroupBy(CreateGroupKey).Where(x => x.Key is not null))
            {
                var point1 = new Point(int.MaxValue, int.MaxValue);
                var point2 = new Point(int.MinValue, int.MinValue);

                foreach (var tile in sequence)
                {
                    var bounds = tile.OriginalBounds;

                    if (bounds.Left < point1.X)
                    {
                        point1.X = bounds.Left;
                    }

                    if (bounds.Top < point1.Y)
                    {
                        point1.Y = bounds.Top;
                    }

                    if (bounds.Right > point2.X)
                    {
                        point2.X = bounds.Right;
                    }

                    if (bounds.Bottom > point2.Y)
                    {
                        point2.Y = bounds.Bottom;
                    }
                }

                var rectangle = new Rectangle(
                    x: point1.X,
                    y: point1.Y,
                    width: point2.X - point1.X,
                    height: point2.Y - point1.Y);

                AddCandidate(sequence.Key!, relativeDelay, rectangle);
            }

            return true;
        }

        public int GetStepIndex(int offset)
        {
            var windowSize = _stepCount * _stepSize;
            var stepOffset = offset % windowSize;
            return (int)MathF.Floor(stepOffset / (float)_stepSize);
        }

        private RingSequence? CreateGroupKey(ImageTile tile)
        {
            var averagedColor = tile.Average();

            return _sequences
                .Select(x => (Item: x, Distance: RgbComparer.CompareByAmplitude(x.Color, averagedColor)))
                .Where(x => x.Distance < 0.2)
                .OrderBy(x => x.Distance)
                .FirstOrDefault().Item;
        }

        public readonly struct SequenceCandidate
        {
            public SequenceCandidate(RingSequence sequence, int relativeOffset, Rectangle bounds)
            {
                Sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
                RelativeOffset = relativeOffset;
                Bounds = bounds;
            }

            public Rectangle Bounds { get; }

            public int RelativeOffset { get; }

            public RingSequence Sequence { get; }
        }
    }
}
