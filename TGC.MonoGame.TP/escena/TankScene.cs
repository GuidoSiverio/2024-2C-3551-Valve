using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.escena;

namespace TGC.MonoGame.TP.Content.Models
{
    class TankScene
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";

        private Model Model { get; set; }
        private Effect Effect { get; set; }
        
        private Texture2D TankTexture { get; set; }

        public Vector3 Position { get; private set; }
        public float Rotation { get; private set; }
        public float TurretRotation { get; private set; } // Turret horizontal rotation
        public float TurretElevation { get; private set; } // Turret vertical rotation

        private float Speed { get; set; } = 500f;
        private float RotationSpeed { get; set; } = 0.5f;
        private float HorizontalMouseSensitivity { get; set; } = 0.005f; // Sensitivity for turret rotation horizontally
        private float VerticalMouseSensitivity { get; set; } = 0.0025f; // Sensitivity for turret rotation vertically

        public Matrix turretWorld { get; set; }

        private MouseState PreviousMouseState;

        public TankScene(ContentManager content, Boolean isEnemy)
        {
            Model = content.Load<Model>(ContentFolder3D + "Panzer/Panzer");
            TankTexture = content.Load<Texture2D>(ContentFolder3D + "Panzer/PzVl_Tiger_I");
            Effect = content.Load<Effect>(ContentFolderEffects + "BasicShader");

            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = Effect;
            }

            if (isEnemy){
                Random random = new Random();
                var MaxDistance = 10000f;

                float randomX = (float)(random.NextDouble() * 2 - 1) * MaxDistance;
                float randomY = 0;
                float randomZ = (float)(random.NextDouble() * 2 - 1) * MaxDistance;

                Position = new Vector3(randomX, randomY, randomZ);
                
            } else {
                Position = Vector3.Zero;
                PreviousMouseState = Mouse.GetState(); // Initialize previous mouse state
            }
            Rotation = 0f;
            TurretRotation = 0f; // Initialize turret horizontal rotation
            TurretElevation = 0f; // Initialize turret vertical rotation
            
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Tank Movement (WASD)
            if (keyboardState.IsKeyDown(Keys.A))
                Rotation += RotationSpeed * deltaTime;
            if (keyboardState.IsKeyDown(Keys.D))
                Rotation -= RotationSpeed * deltaTime;

            var forward = Vector3.Transform(Vector3.Backward, Matrix.CreateRotationY(Rotation));
            if (keyboardState.IsKeyDown(Keys.W))
                Position += forward * Speed * deltaTime;
            if (keyboardState.IsKeyDown(Keys.S))
                Position -= forward * Speed * deltaTime;

            // Turret Rotation (Mouse)
            float mouseDeltaX = mouseState.X - PreviousMouseState.X;
            float mouseDeltaY = mouseState.Y - PreviousMouseState.Y;

            TurretRotation -= HorizontalMouseSensitivity * mouseDeltaX;
            TurretElevation += VerticalMouseSensitivity * mouseDeltaY; // Invert the Y-axis by adding instead of subtracting

            // Clamp turret elevation to a range (e.g., between -45 and 45 degrees)
            TurretElevation = MathHelper.Clamp(TurretElevation, MathHelper.ToRadians(-30f), MathHelper.ToRadians(5f));

            // Store current mouse state for next frame
            PreviousMouseState = mouseState;
        }

        public void Draw(Matrix world, Matrix view, Matrix projection, HeightMapScene heightMap)
        {
            // Tank base transformation (movement and body rotation)
            var tankWorld = Matrix.CreateRotationY(Rotation) * Matrix.CreateTranslation(Position);
            // Turret transformation (additional turret rotation and elevation)
            var turretRotationMatrix = Matrix.CreateRotationY(TurretRotation);
            var turretElevationMatrix = Matrix.CreateRotationX(TurretElevation);

            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(projection);
            //Effect.Parameters["DiffuseColor"].SetValue(Color.Red.ToVector3());
            Effect.Parameters["ModelTexture"].SetValue(TankTexture);

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
                    // Apply turret and cannon transformations, only applying tank position, not rotation
                    turretWorld = turretElevationMatrix * turretRotationMatrix * Matrix.CreateTranslation(Position);
                    worldMatrix = relativeTransform * turretWorld * world;
                    color = Color.Red; // Set desired color for the turret
                }
                else if (mesh.Name.Contains("Cannon"))
                {
                    // Apply turret and cannon transformations, only applying tank position, not rotation
                    turretWorld = turretElevationMatrix * turretRotationMatrix * Matrix.CreateTranslation(Position);
                    worldMatrix = relativeTransform * turretWorld * world;
                    color = Color.Blue; 
                }
                else
                {
                    // Apply only tank transformations
                    worldMatrix = relativeTransform * tankWorld * world;
                    color = Color.Green; 
                }

                float height = heightMap.Height(worldMatrix.Translation.X, worldMatrix.Translation.Z);

                Effect.Parameters["World"].SetValue(worldMatrix * Matrix.CreateTranslation(0, height, 0));
                //Effect.Parameters["DiffuseColor"].SetValue(color.ToVector3());
                mesh.Draw();
            }
        }
    }
}