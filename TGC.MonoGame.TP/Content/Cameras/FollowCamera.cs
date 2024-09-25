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

        public void Update(Vector3 tankPosition, float turretRotation, float turretElevation)
        {
            // Apply the combined rotation to the offset
            Vector3 cameraOffset = Vector3.Transform(Offset, Matrix.CreateRotationY(turretRotation) * Matrix.CreateRotationX(turretElevation));
            Vector3 cameraPosition = tankPosition + cameraOffset;
            Vector3 cameraTarget = tankPosition + Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(turretRotation) * Matrix.CreateRotationX(turretElevation));

            ViewMatrix = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);
        }
    }
}