namespace Robot.Server.Geolocation
{
    using System.Drawing;

    public readonly struct Ring
    {
        public Point Bottom { get; }

        public Point Left { get; }

        public Point Right { get; }

        public Point Top { get; }
    }
}
