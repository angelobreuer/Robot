namespace Robot.Server.Geolocation
{
    using System;
    using System.Collections.Immutable;

    public sealed class MotionDetector
    {
        private RgbColor[]? _lastFrame;

        public ImmutableArray<ImageTile> DetectChanges(ImageGrid grid)
        {
            using var enumerator = grid.GetEnumerator();
            var index = 0;

            if (_lastFrame is null)
            {
                _lastFrame = GC.AllocateUninitializedArray<RgbColor>(grid.Tiles);

                while (enumerator.MoveNext())
                {
                    _lastFrame[index++] = enumerator.Current.Average();
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
                var currentColor = enumerator.Current.Average();

                _lastFrame[index++] = currentColor;

                if (RgbComparer.Compare(previousColor, currentColor) > 0.4)
                {
                    builder.Add(enumerator.Current);
                }
            }

            return builder.ToImmutable();
        }
    }
}
