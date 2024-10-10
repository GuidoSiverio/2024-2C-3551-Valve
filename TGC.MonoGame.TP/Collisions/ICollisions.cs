using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Collisions
{
    public interface ICollidable
    {
        bool Intersects(OrientedBoundingBox other);
        void OnCollide(ICollidable other);
        OrientedBoundingBox GetBoundingBox();
    }
}
