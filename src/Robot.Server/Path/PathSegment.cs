namespace Robot.Server.Path
{
    using System.Drawing;

    public readonly struct PathSegment
    {
        public PathSegment(Point point1, Point point2)
        {
            Point1 = point1;
            Point2 = point2;
        }

        public Point Point1 { get; }

        public Point Point2 { get; }
    }
}
