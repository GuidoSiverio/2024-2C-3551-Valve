using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public class FreeCamera
{
    public Vector3 Position { get; set; }
    public float Yaw { get; set; }
    public float Pitch { get; set; }

    private float speed;
    private float rotationSpeed;
    private Vector2 previousMousePosition;

    public Matrix ViewMatrix { get; set; }

    public FreeCamera(Vector3 startPosition, float speed, float rotationSpeed)
    {
        Position = startPosition;
        Yaw = 0.0f;
        Pitch = 0.0f;
        this.speed = speed;
        this.rotationSpeed = rotationSpeed;
        previousMousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        UpdateViewMatrix();
    }

    public void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Vector2 mousePosition = mouseState.Position.ToVector2();
        Vector2 delta = mousePosition - previousMousePosition;
        previousMousePosition = mousePosition;

        Yaw += delta.X * rotationSpeed * deltaTime;
        Pitch -= delta.Y * rotationSpeed * deltaTime;

        Pitch = MathHelper.Clamp(Pitch, -MathHelper.PiOver2, MathHelper.PiOver2);

        Matrix cameraRotation = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, 0.0f);
        Vector3 forward = Vector3.Transform(Vector3.Forward, cameraRotation);
        Vector3 right = Vector3.Transform(Vector3.Right, cameraRotation);

        forward.Normalize();
        right.Normalize();
        if (keyboardState.IsKeyDown(Keys.W))
            Position += forward * speed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.S))
            Position -= forward * speed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.A))
            Position -= right * speed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.D))
            Position += right * speed * deltaTime;
        UpdateViewMatrix();
    }

    private void UpdateViewMatrix()
    {
        Vector3 forward = new Vector3((float)Math.Cos(Pitch) * (float)Math.Cos(Yaw), (float)Math.Sin(Pitch),
            (float)Math.Cos(Pitch) * (float)Math.Sin(Yaw));
        forward.Normalize();

        ViewMatrix = Matrix.CreateLookAt(Position, Position + forward, Vector3.Up);
    }
}