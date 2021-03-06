﻿namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public struct Rect
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public Rect(int x, int y, int width, int height)
        {
            this = new Rect();
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
    }
}
