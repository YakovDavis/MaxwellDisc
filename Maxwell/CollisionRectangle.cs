using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
namespace Maxwell
{
    public class CollisionRectangle
    {
        private Vector2 v1;
        private Vector2 v2;
        private Vector2 v3;
        private Vector2 v4;
        private Point center;
        private float rotation;

        public CollisionRectangle(Point c, int a, int b, float r)
        {
            v1 = new Vector2(a / 2, b / 2);
            v2 = new Vector2(a / 2, - b / 2);
            v3 = new Vector2(- a / 2, - b / 2);
            v4 = new Vector2(- a / 2, b / 2);

            v1 = Vector2.Transform(v1, (Matrix.CreateRotationZ(r)));
            v2 = Vector2.Transform(v2, (Matrix.CreateRotationZ(r)));
            v3 = Vector2.Transform(v3, (Matrix.CreateRotationZ(r)));
            v4 = Vector2.Transform(v4, (Matrix.CreateRotationZ(r)));

            center = c;
            rotation = r;
        }

        public void Rotate(float r)
        {
            v1 = Vector2.Transform(v1, (Matrix.CreateRotationZ(r)));
            v2 = Vector2.Transform(v2, (Matrix.CreateRotationZ(r)));
            v3 = Vector2.Transform(v3, (Matrix.CreateRotationZ(r)));
            v4 = Vector2.Transform(v4, (Matrix.CreateRotationZ(r)));

            rotation += r;
        }

        public void SetRotation(float r)
        {
            v1 = Vector2.Transform(v1, (Matrix.CreateRotationZ(r - rotation)));
            v2 = Vector2.Transform(v2, (Matrix.CreateRotationZ(r - rotation)));
            v3 = Vector2.Transform(v3, (Matrix.CreateRotationZ(r - rotation)));
            v4 = Vector2.Transform(v4, (Matrix.CreateRotationZ(r - rotation)));

            rotation = r;
        }

    }
}
