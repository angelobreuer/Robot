namespace Robot.Server.Geolocation
{
    using System;
    using System.Collections.Immutable;
    using System.Numerics;

    public sealed class MotionDetector
    {
        private Vector3[]? _lastFrame;

        public ImmutableArray<ImageTile> DetectChanges(ImageGrid grid)
        {
            using var enumerator = grid.GetEnumerator();
            var index = 0;

            if (_lastFrame is null)
            {
                _lastFrame = GC.AllocateUninitializedArray<Vector3>(grid.Tiles);

                while (enumerator.MoveNext())
                {
                    _lastFrame[index++] = enumerator.Current.Sum();
                }

                return ImmutableArray<ImageTile>.Empty;
            }

            if (_lastFrame.Length != grid.Tiles)
            {
                throw new InvalidOperationException("Last frame's tile count does not match the currents.");
            }

            var builder = ImmutableArray.CreateBuilder<ImageTile>();

            while (enumerator.MoveNext())
            {
                var previousColor = _lastFrame[index];
                var currentColor = enumerator.Current.Sum();

                _lastFrame[index++] = currentColor;

                if (Vector3.Distance(previousColor * Transform, currentColor * Transform) > 30000)
                {
                    builder.Add(enumerator.Current);
                }
            }

            return builder.ToImmutable();
        }

        private static readonly Vector3 Transform = new(.3F, 1F, 1.7F);
    }
}
