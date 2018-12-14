using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
    }
}
