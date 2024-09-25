using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP
{
    public class FollowCamera
    {
        public Matrix ViewMatrix { get; private set; }
        public Matrix ProjectionMatrix { get; private set; }

        private Vector3 Offset { get; }
        private float AspectRatio { get; }
        private float FieldOfView { get; }
        private float NearPlane { get; }
        private float FarPlane { get; }

        public FollowCamera(float aspectRatio, Vector3 offset)
        {
            AspectRatio = aspectRatio;
            Offset = offset;
            FieldOfView = MathHelper.PiOver4;
            NearPlane = 0.1f;
            FarPlane = 250000f;

            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
        }

        public void Update(Vector3 tankPosition, Vector3 tankForward)
        {
            // Create a rotation matrix from the tank's forward vector
            Matrix tankRotation = Matrix.CreateWorld(Vector3.Zero, tankForward, Vector3.Up);

            // Apply the rotation to the offset
            Vector3 cameraOffset = Vector3.Transform(Offset, tankRotation);

            // Calculate the camera position by adding the offset to the tank's position
            Vector3 cameraPosition = tankPosition + cameraOffset;
            Vector3 cameraTarget = tankPosition + tankForward;

            ViewMatrix = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);
        }
    }
}