using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Content.Models
{
    class TankScene
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";

        private Model Model { get; set; }
        private Effect Effect { get; set; }
        private Texture2D TankTexture { get; set; }

        public Vector3 Position { get; set; }
        public float Rotation { get; private set; }
        public float TurretRotation { get; private set; } // Turret horizontal rotation
        public float TurretElevation { get; private set; } // Turret vertical rotation
        private float InclinationAngle { get; set; } = 0f; // Inclination for the tank

        private float Speed { get; set; } = 500f;
        private float RotationSpeed { get; set; } = 0.5f;
        private float HorizontalMouseSensitivity { get; set; } = 0.005f; // Sensitivity for turret rotation horizontally
        private float VerticalMouseSensitivity { get; set; } = 0.0025f; // Sensitivity for turret rotation vertically

        public Matrix turretWorld { get; set; }
        public Matrix PanzerMatrix { get; set; }
        private MouseState PreviousMouseState;
        public bool isColliding { get; set; } = false;

        public OrientedBoundingBox TankBox { get; private set; } // Bounding box for the tank

        private bool isInclining = false; // Flag for tank inclination
        private float inclinationDuration = 0.5f; // Duración de la inclinación en segundos
        private float inclinationTime = 0f; // Tiempo transcurrido de inclinación

        public TankScene(ContentManager content, bool isEnemy)
        {
            Model = content.Load<Model>(ContentFolder3D + "Panzer/Panzer");
            TankTexture = content.Load<Texture2D>(ContentFolder3D + "Panzer/PzVl_Tiger_I");
            Effect = content.Load<Effect>(ContentFolderEffects + "BasicShader");

            List<Vector3> vertices = new List<Vector3>();

            // Collect vertices to create OrientedBoundingBox
            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Effect;

                    VertexBuffer vertexBuffer = meshPart.VertexBuffer;
                    VertexPositionNormalTexture[] vertexData = new VertexPositionNormalTexture[vertexBuffer.VertexCount];
                    vertexBuffer.GetData(vertexData);

                    foreach (VertexPositionNormalTexture vertex in vertexData)
                    {
                        vertices.Add(vertex.Position);
                    }
                }
            }

            // Initialize the bounding box for the tank using the vertices collected
            TankBox = OrientedBoundingBox.CreateFromPoints(vertices.ToArray());

            // Set the position of the tank depending on if it is an enemy or not
            if (isEnemy)
            {
                Random random = new Random();
                var MaxDistance = 10000f;

                float randomX = (float)(random.NextDouble() * 2 - 1) * MaxDistance;
                float randomY = 0;
                float randomZ = (float)(random.NextDouble() * 2 - 1) * MaxDistance;

                Position = new Vector3(randomX, randomY, randomZ);
            }
            else
            {
                Position = Vector3.Zero;
                PreviousMouseState = Mouse.GetState(); // Initialize previous mouse state
            }

            Rotation = 0f;
            TurretRotation = 0f; // Initialize turret horizontal rotation
            TurretElevation = 0f; // Initialize turret vertical rotation
        }

        public void UpdateBoundingBox()
        {
            // Update the Tank's bounding box based on its current position and rotation
            TankBox.Center = Position;
            TankBox.Rotate(Matrix.CreateRotationY(Rotation));
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState, List<object> objects)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Check for collision and apply inclination if needed
            CheckCollision(objects, deltaTime);

            // Tank Movement (WASD)
            if (!isColliding)  // Avoid movement if colliding
            {
                if (keyboardState.IsKeyDown(Keys.A))
                    Rotation += RotationSpeed * deltaTime;
                if (keyboardState.IsKeyDown(Keys.D))
                    Rotation -= RotationSpeed * deltaTime;

                var forward = Vector3.Transform(Vector3.Backward, Matrix.CreateRotationY(Rotation));
                if (keyboardState.IsKeyDown(Keys.W))
                {
                    Position += forward * Speed * deltaTime;
                }
                if (keyboardState.IsKeyDown(Keys.S))
                {
                    Position -= forward * Speed * deltaTime;
                }
            }

            // Turret Rotation (Mouse)
            float mouseDeltaX = mouseState.X - PreviousMouseState.X;
            float mouseDeltaY = mouseState.Y - PreviousMouseState.Y;

            TurretRotation -= HorizontalMouseSensitivity * mouseDeltaX;
            TurretElevation += VerticalMouseSensitivity * mouseDeltaY; 

            // Clamp turret elevation to a range (e.g., between -30 and 5 degrees)
            TurretElevation = MathHelper.Clamp(TurretElevation, MathHelper.ToRadians(-30f), MathHelper.ToRadians(5f));

            // Store current mouse state for next frame
            PreviousMouseState = mouseState;

            // Update bounding box after moving the tank
            UpdateBoundingBox();
        }

        private void CheckCollision(List<object> objects, float deltaTime)
        {
            bool collided = false;

            // Recorre todos los objetos en la lista
            foreach (var obj in objects)
            {
                if (obj is RockScene rockScene)
                {
                    rockScene.HandleTankCollision(this);  // Maneja la colisión desde RockScene

                    // Recorre todos los cilindros asociados a la roca
                    foreach (var rockCylinder in rockScene.GetBoundingCylinders())
                    {
                        if (rockCylinder.Contains(Position) == ContainmentType.Contains)
                        {
                            if (!isInclining)
                            {
                                isInclining = true;
                                ApplyInclination();
                            }
                            collided = true;
                            break;
                        }
                    }

                    // Si hubo colisión, no necesitas seguir buscando
                    if (collided)
                        break;
                }
            }

            // Si no hay colisión, resetea la inclinación gradualmente
            if (!collided)
            {
                ResetInclination(deltaTime, 2f); // Pasa el deltaTime y una velocidad para que vuelva a su inclinación original
            }
        }


        public void ApplyInclination()
        {
            // Aplica una inclinación temporal cuando el tanque colisiona con una roca
            InclinationAngle = MathHelper.Lerp(InclinationAngle, MathHelper.ToRadians(-30f), 0.1f); // Inclinación de -30 grados aplicada suavemente
        }

        public void ResetInclination(float deltaTime, float resetSpeed)
        {
            // Gradualmente retorna la inclinación a 0 usando el deltaTime y la velocidad de reseteo
            InclinationAngle = MathHelper.Lerp(InclinationAngle, 0f, deltaTime * resetSpeed);

            // Verifica si ya ha vuelto a su posición normal
            if (MathF.Abs(InclinationAngle) < 0.01f) // Tolerancia para no quedar en un loop infinito
            {
                InclinationAngle = 0f;
                isInclining = false;
            }
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            // Tank base transformation (movement, body rotation, and inclination)
            var tankWorld = Matrix.CreateRotationY(Rotation) *
                            Matrix.CreateRotationX(InclinationAngle) * // Apply inclination
                            Matrix.CreateTranslation(Position);

            // Turret transformation (additional turret rotation and elevation)
            var turretRotationMatrix = Matrix.CreateRotationY(TurretRotation);
            var turretElevationMatrix = Matrix.CreateRotationX(TurretElevation);

            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(projection);
            Effect.Parameters["ModelTexture"].SetValue(TankTexture);

            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            foreach (var mesh in Model.Meshes)
            {
                var relativeTransform = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                Matrix worldMatrix;

                // Turret and cannon should move together
                if (mesh.Name.Contains("Turret") || mesh.Name.Contains("Cannon"))
                {
                    turretWorld = turretElevationMatrix * turretRotationMatrix * Matrix.CreateTranslation(Position);
                    worldMatrix = relativeTransform * turretWorld * world;
                }
                else
                {
                    worldMatrix = relativeTransform * tankWorld * world;
                }

                Effect.Parameters["World"].SetValue(worldMatrix);
                mesh.Draw();
            }
        }
    }
}
