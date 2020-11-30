namespace Robot.Server.Path
{
    using System.Drawing;

    public static class PathFinder
    {
        private sealed class Grid
        {
            private readonly Table[,] _tables;

            public void AddTable(Rectangle boundingBox)
            {
            }

            public Point GetCenter(Rectangle rectangle)
            {
                var x = rectangle.X + (rectangle.Width / 2);
                var y = rectangle.Y + (rectangle.Height / 2);
                return new Point(x, y);
            }
        }

        private sealed class Table
        {
            public Rectangle BoundingBox { get; }
        }
    }
}
