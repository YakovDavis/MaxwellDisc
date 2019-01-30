using System;
using Microsoft.Xna.Framework;
namespace Maxwell
{
    public interface IEnemy
    {
        Rectangle GetRenderRectangle();

        Rectangle GetCollisionRectangle();

        float GetRot();

        Color GetCol();

        string GetTexture();

        float DistanceToBottomCenter();

        float GetAngle();

        void Update(GameTime gameTime);
    }
}
