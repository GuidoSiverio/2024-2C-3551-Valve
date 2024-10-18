﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.Content.Models
{
    public class TankScene
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";

        private Model Model { get; set; }
        private Effect Effect { get; set; }

        public Vector3 Position { get; set; }
        public float Rotation { get; private set; }
        public float TurretRotation { get; private set; } // Turret horizontal rotation
        public float TurretElevation { get; private set; } // Turret vertical rotation

        private float Speed { get; set; } = 500f;
        private float RotationSpeed { get; set; } = 0.5f;
        private float MouseSensitivity { get; set; } = 0.005f; // Sensitivity for turret rotation
        
        public float _currentLife { get; set; }

        private MouseState PreviousMouseState;

        public TankScene(ContentManager content)
        {
            Model = content.Load<Model>(ContentFolder3D + "Panzer/Panzer");
            Effect = content.Load<Effect>(ContentFolderEffects + "BasicShader");
            foreach (var mesh in Model.Meshes)
            {                
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = Effect;
            }

            Position = Vector3.Zero;
            Rotation = 0f;
            TurretRotation = 0f; // Initialize turret horizontal rotation
            TurretElevation = 0f; // Initialize turret vertical rotation
            PreviousMouseState = Mouse.GetState(); // Initialize previous mouse state
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyboardState.IsKeyDown(Keys.A))
                Rotation += RotationSpeed * deltaTime;
            if (keyboardState.IsKeyDown(Keys.D))
                Rotation -= RotationSpeed * deltaTime;

            var forward = Vector3.Transform(Vector3.Backward, Matrix.CreateRotationY(Rotation));
            if (keyboardState.IsKeyDown(Keys.W))
                Position += forward * Speed * deltaTime;
            if (keyboardState.IsKeyDown(Keys.S))
                Position -= forward * Speed * deltaTime;

            // Calculate mouse movement delta for turret rotation
            float mouseDeltaX = mouseState.X - PreviousMouseState.X;
            float mouseDeltaY = mouseState.Y - PreviousMouseState.Y;

            TurretRotation -= MouseSensitivity * mouseDeltaX;
            TurretElevation += MouseSensitivity * mouseDeltaY; 

            // Clamp turret elevation to a range (e.g., between -45 and 45 degrees)
            TurretElevation = MathHelper.Clamp(TurretElevation, MathHelper.ToRadians(-45f), MathHelper.ToRadians(45f));

            // Store current mouse state for next frame
            PreviousMouseState = mouseState;
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            // Tank base transformation (movement and body rotation)
            var tankWorld = Matrix.CreateRotationY(Rotation) * Matrix.CreateTranslation(Position);
            // Turret transformation (additional turret rotation and elevation)
            var turretRotationMatrix = Matrix.CreateRotationY(TurretRotation);
            var turretElevationMatrix = Matrix.CreateRotationX(TurretElevation);
            var turretWorld = turretElevationMatrix * turretRotationMatrix * tankWorld;

            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(projection);

            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            foreach (var mesh in Model.Meshes)
            {
                var relativeTransform = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                // Determine the appropriate transform and color based on the mesh name
                Matrix worldMatrix;
                Color color;

                if (mesh.Name.Contains("Turret"))
                {
                    worldMatrix = relativeTransform * turretWorld * world;
                    color = Color.Red; 
                }
                else if (mesh.Name.Contains("Cannon"))
                {
                    worldMatrix = relativeTransform * turretWorld * world;
                    color = Color.Blue; 
                }
                else
                {
                    worldMatrix = relativeTransform * tankWorld * world;
                    color = Color.Green;
                }

                Effect.Parameters["World"].SetValue(worldMatrix);
                Effect.Parameters["DiffuseColor"].SetValue(color.ToVector3());
                mesh.Draw();
            }
        }
    }
}