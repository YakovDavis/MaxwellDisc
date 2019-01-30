using System;
using Microsoft.Xna.Framework;

namespace Maxwell
{
    public class Reflector
    {
        private Vector2 position;
        private Color col;
        private float rot;
        private Point center;
        private float radius;
        private float width;
        private Rectangle[] colliders;
        private Vector2[] collVectors;

        public Reflector(Point cen, float r, float w, Color c)
        {
            center = cen;
            col = c;
            rot = 0;
            radius = r;
            width = w;
            colliders = new Rectangle[4];
            collVectors = new Vector2[4];
            int wtemp = Convert.ToInt32(width / 8);
            for (int i = 0; i < 4; i++)
            {
                collVectors[i] = new Vector2(-36 + i * 24 , r - i*i + 3 * i + 130);
                collVectors[i] = Vector2.Transform(collVectors[i], Matrix.CreateRotationZ(rot));
            }
            for (int i = 0; i < 4; i++)
            {
                colliders[i] = new Rectangle(center.X - Convert.ToInt32(Vector2.Subtract(collVectors[i], new Vector2(wtemp, wtemp)).X), center.Y - Convert.ToInt32(Vector2.Subtract(collVectors[i], new Vector2(wtemp, wtemp)).Y), 2 * wtemp, 2 * wtemp);
            }
            position.X = radius * Convert.ToSingle(Math.Sin(rot));
            position.Y = radius * Convert.ToSingle(Math.Cos(rot));
        }

        public float GetRot()
        {
            return rot;
        }

        public Color GetCol()
        {
            return col;
        }

        public Point GetPos()
        {
            return new Point(Convert.ToInt32(- position.X + center.X), Convert.ToInt32(center.Y - position.Y));
        }

        public bool Collides(Rectangle disc)
        {
            bool flag = false;
            foreach (Rectangle c in colliders)
                if (c.Intersects(disc))
                    flag = true;
            return flag;
        }

        public void SetRotation(float rad)
        {
            float prev = rot;
            rot = rad;
            if (rot > Convert.ToSingle(Math.PI / 2))
                rot = Convert.ToSingle(Math.PI / 2);
            if (rot < -Convert.ToSingle(Math.PI / 2) + 0.05f)
                rot = -Convert.ToSingle(Math.PI / 2);
            position = Vector2.Transform(position, Matrix.CreateRotationZ(rot - prev));
            for (int i = 0; i < colliders.Length; i++)
            {
                collVectors[i] = Vector2.Transform(collVectors[i], Matrix.CreateRotationZ(rot-prev));
            }
            int w = Convert.ToInt32(width / 8);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].Location = new Point(center.X - Convert.ToInt32(Vector2.Subtract(collVectors[i], new Vector2(w, w)).X), center.Y - Convert.ToInt32(Vector2.Subtract(collVectors[i], new Vector2(w, w)).Y));
            }
        }

        public Rectangle[] GetColliders()
        {
            return colliders;
        }
    }
}