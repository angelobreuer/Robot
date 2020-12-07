namespace Robot.Server.LensCorrection
{
    using System;
    using System.Buffers;

    public static class FishEyeCorrection
    {
        public static ImageGrid Correct(ImageGrid grid, float strength)
        {
            if (strength is 0)
            {
                strength = 0.00001F;
            }

            var widthSquared = MathF.Pow(grid.Width, 2);
            var heightSquared = MathF.Pow(grid.Height, 2);
            var correctionRadius = MathF.Sqrt(widthSquared + heightSquared) / strength;

            var buffer = ArrayPool<RgbColor>.Shared.Rent(grid.Data.Length);

            try
            {
                CorrectFishEye(grid, correctionRadius, buffer);
                return new ImageGrid(buffer, grid.TilesX, grid.TilesY, grid.TileSize, ArrayPool<RgbColor>.Shared);
            }
            finally
            {
                ArrayPool<RgbColor>.Shared.Return(buffer);
            }
        }

        private static void CorrectFishEye(ImageGrid grid, float correctionRadius, RgbColor[] buffer)
        {
            // calculate the coordinates of the center of the image
            var centerX = grid.Width / 2;
            var centerY = grid.Height / 2;

            // iterate through all horizontal lines
            for (var y = 0; y < grid.Height; y++)
            {
                // calculate the y-coordinate currently processing and precompute the squared value
                // of the current center y-coordinate being processed
                var currentY = y - centerY;
                var currentYSquared = MathF.Pow(currentY, 2);

                // iterate through all vertical lines in the current horizontal scan line
                for (var x = 0; x < grid.Width; x++)
                {
                    // calculate the x-coordinate currently processing and compute the squared value
                    // of the current center x-coordinate being processed
                    var currentX = x - centerX;
                    var currentXSquared = MathF.Pow(currentX, 2);

                    // compute the distance between the original center and the current point
                    var distance = MathF.Sqrt(currentXSquared + currentYSquared);

                    // compute the current correction radius to perform correction with
                    var radius = distance / correctionRadius;

                    // the theta used as a factor to correct the fish-eye distortion, pre-initialize
                    // with 1F to avoid division through zero errors
                    var theta = 1F;

                    // check if the radius is not zero, to avoid division through zero errors
                    if (radius is not 0.0F)
                    {
                        // compute the theta
                        theta = MathF.Atan(radius) / radius;
                    }

                    // calculate the coordinates of the pixel to retrieve the value from
                    var sourceX = (int)MathF.Floor(centerX + (theta * currentX));
                    var sourceY = (int)MathF.Floor(centerY + (theta * currentY));

                    // retrieve the value of the source pixel
                    var value = grid.GetPixel(sourceX, sourceY);

                    // set the current target pixel to the corrected pixel
                    buffer[x + (y * grid.Width)] = value;
                }
            }
        }
    }
}
