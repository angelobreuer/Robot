namespace Robot.Server.Stages.Recognition
{
    public readonly struct TableBoundary
    {
        public TableBoundary(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int Height { get; }

        public int Width { get; }

        public int X { get; }

        public int Y { get; }
    }
}
