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
    
    public Matrix ViewMatrix { get; set; }

    public FreeCamera(Vector3 startPosition, float speed, float rotationSpeed)
    {
        Position = startPosition;
        Yaw = 0.0f;
        Pitch = 0.0f;
        this.speed = speed;
        this.rotationSpeed = rotationSpeed;
        UpdateViewMatrix();
    }

    public void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState)
    {
        float deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
        
        Vector3 forward = new Vector3((float) Math.Sin(Yaw),0,(float) Math.Cos(Yaw));
        forward.Normalize();
        Vector3 right = new Vector3(-forward.Z,0,forward.X);
        if (keyboardState.IsKeyDown(Keys.W))
            Position += forward * speed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.S))
            Position -= forward * speed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.A))
            Position -= right * speed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.D))
            Position += right * speed * deltaTime;
        float deltaX = mouseState.X * rotationSpeed * deltaTime;
        float deltaY = mouseState.Y * rotationSpeed * deltaTime;
        
        Yaw -= deltaX;
        Pitch -= deltaY;
        
        Pitch = MathHelper.Clamp(Pitch, -MathHelper.PiOver2, MathHelper.PiOver2);
        UpdateViewMatrix();
    }

    private void UpdateViewMatrix()
    {
        Vector3 forward = new Vector3((float)Math.Cos(Pitch) * (float)Math.Cos(Yaw), (float)Math.Sin(Pitch), (float)Math.Cos(Pitch) * (float)Math.Sin(Yaw));
        forward.Normalize();

        ViewMatrix = Matrix.CreateLookAt(Position, Position + forward, Vector3.Up);
    }
}