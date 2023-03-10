using System;
using Microsoft.Xna.Framework;
namespace Maxwell
{
    public class Disc : IEnemy
    {
        private const float aSpeed = 0.005f;
        private const float speed = 0.1f;
        private Vector2 velocity;
        private Vector2 position;
        private Color col;
        private float rot;
        private Point target;
        private bool wasReversed;
        private Point bottomCenter;
        private string texture;

        private MiscTools mt;

        public Disc(string tex, Color c, Point pos, Point tar, Point bc)
        {
            position = pos.ToVector2();
            col = c;
            target = tar;
            texture = tex;
            bottomCenter = bc;
            velocity = new Vector2(target.X - pos.X, target.Y - pos.Y);
            velocity.Normalize();
            velocity *= speed;
            wasReversed = false;
            mt = new MiscTools();
        }

        public Rectangle GetRenderRectangle()
        {
            return new Rectangle(mt.Floor(position.X), mt.Floor(position.Y), 96, 96);
        }

        public Rectangle GetCollisionRectangle()
        {
            return new Rectangle(mt.Floor(position.X), mt.Floor(position.Y), 36, 36);
        }

        public float GetRot()
        {
            return rot;
        }

        public Color GetCol()
        {
            return col;
        }

        public string GetTexture()
        {
            return texture;
        }

        public float DistanceToBottomCenter()
        {
            return Convert.ToSingle(Math.Sqrt((bottomCenter.X - position.X) * (bottomCenter.X - position.X) + (bottomCenter.Y - position.Y) * (bottomCenter.Y - position.Y)));
        }

        public void Reverse()
        {
            velocity *= -1;
            wasReversed = true;
        }

        public bool WasReversed()
        {
            return wasReversed;
        }

        public float GetAngle()
        {
            return Convert.ToSingle(Math.Atan2(velocity.X, velocity.Y));
        }

        public void Update(GameTime gameTime)
        {
            if (rot < Convert.ToSingle(2 * Math.PI))
            {
                rot += aSpeed * Convert.ToSingle(gameTime.ElapsedGameTime.TotalMilliseconds);
            }
            else
            {
                rot += aSpeed * Convert.ToSingle(gameTime.ElapsedGameTime.TotalMilliseconds) - Convert.ToSingle(2 * Math.PI);
            }
            position.X += Convert.ToSingle(velocity.X * gameTime.ElapsedGameTime.TotalMilliseconds);
            position.Y += Convert.ToSingle(velocity.Y * gameTime.ElapsedGameTime.TotalMilliseconds);
        }
    }
}
