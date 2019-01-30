using System;
using Microsoft.Xna.Framework;

namespace Maxwell
{
    public class MiscTools
    {
        private Random rnd;
        private Color[] col;

        public MiscTools(Random r)
        {
            rnd =r;
            col = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.LightBlue, Color.Blue, Color.Purple};
        }

        public MiscTools()
        {
        }

        public Color RandomColor()
        {
            return col[rnd.Next(col.Length)];
        }

        public Point RandomSpawn(Point c, int r)
        {
            int rx = rnd.Next(2 * r) - r;
            int ry = Convert.ToInt32(Math.Sqrt(r * r - rx * rx));
            return new Point(rx + c.X, c.Y - ry);
        }

        public int Div2 (float f)
        {
            return Convert.ToInt32(f / 2);
        }

        public int Div2(int i)
        {
            return Convert.ToInt32(i / 2);
        }

        public int Floor(float f)
        {
            return Convert.ToInt32(f);
        }

        public Rectangle CenterRectange(int x, int y, int w, int h)
        {
            return new Rectangle(x - Div2(w), y - Div2(h), w, h);
        }

        public Rectangle BottomCenterRectangle(int x, int y, int w, int h)
        {
            return new Rectangle(x - Div2(w), y - h, w, h);
        }
    }
}
