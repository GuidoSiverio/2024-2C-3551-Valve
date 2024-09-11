using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public class FreeCamera
{
    private Vector3 Position { get; set; }
    private float Yaw { get; set; }
    private float Pitch { get; set; }

    private readonly float _speed;
    private readonly float _rotationSpeed;
    private Vector2 _previousMousePosition;

    public Matrix ViewMatrix { get; set; }

    public FreeCamera(Vector3 startPosition, float speed, float rotationSpeed)
    {
        Position = startPosition;
        Yaw = 0.0f;
        Pitch = 0.0f;
        _speed = speed;
        _rotationSpeed = rotationSpeed;
        _previousMousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        UpdateViewMatrix();
    }

    public void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Vector2 mousePosition = mouseState.Position.ToVector2();
        Vector2 delta = mousePosition - _previousMousePosition;
        _previousMousePosition = mousePosition;

        Yaw += delta.X * _rotationSpeed * deltaTime;
        Pitch -= delta.Y * _rotationSpeed * deltaTime;

        Pitch = MathHelper.Clamp(Pitch, -MathHelper.PiOver2, MathHelper.PiOver2);

        Matrix cameraRotation = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, 0.0f);
        Vector3 forward = Vector3.Transform(Vector3.Forward, cameraRotation);
        Vector3 right = Vector3.Transform(Vector3.Right, cameraRotation);

        forward.Normalize();
        right.Normalize();
        if (keyboardState.IsKeyDown(Keys.W))
            Position += forward * _speed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.S))
            Position -= forward * _speed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.A))
            Position -= right * _speed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.D))
            Position += right * _speed * deltaTime;
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