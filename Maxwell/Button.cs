using System;
using Microsoft.Xna.Framework;
namespace Maxwell
{
    public class Button
    {
        private Rectangle box;

        private string text;

        public Rectangle Box
        {
            get
            {
                return box;
            }
            set
            {
                box = value;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        public Button(int x1, int y1, int x2, int y2, string s)
        {
            box = new Rectangle(x1, y1, x2, y2);
            text = s;
        }

        public bool IsOnButton(int x, int y)
        {
            if ((x > x1) && (x < x2) && (y > y1) && (y < y2))
                return true;
            return false
        }
    }
}
