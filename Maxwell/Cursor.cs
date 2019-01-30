using System;
using Microsoft.Xna.Framework;
namespace Maxwell
{
    public class Cursor
    {
        private Vector2 accuratePos;
        private float sensation;
        private int halfSize;

        public Cursor(int x, int y, int size, float sens)
        {
            accuratePos = new Vector2(x, y);
            sensation = sens;
            halfSize = Convert.ToInt32(size / 2);
        }

        public Rectangle GetRenderRectangle()
        {
            return new Rectangle(Convert.ToInt32(accuratePos.X) - halfSize,
                                 Convert.ToInt32(accuratePos.Y) - halfSize,
                                 halfSize * 2,
                                 halfSize * 2);
        }

        public void Move(Vector2 displacement)
        {
            accuratePos += displacement * sensation;
        }

        public int Size
        {
            get { return halfSize * 2; }
            set { halfSize = Convert.ToInt32(value / 2); }
        }

        public float Sensation
        {
            get { return sensation; }
            set { sensation = value; }
        }

        public Vector2 AccuratePosition
        {
            get { return accuratePos; }
            set { accuratePos = value; }
        }
    }
}
