using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Content.Models
{
    class RockScene
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";

        private Model Model { get; set; }
        private Effect Effect { get; set; }
        private List<Matrix> WorldMatrices { get; set; }
        private List<BoundingCylinder> RockBoundingCylinders { get; set; } // Lista de Bounding Cylinders

        public const float MaxDistance = 10000f;
        public const float MinDistance = 1000f;
        private static Random random = new Random();
        private Texture2D RockTexture { get; set; }

        public RockScene(ContentManager content, int numberOfModels)
        {
            Model = content.Load<Model>(ContentFolder3D + "escena/Rock_1");
            RockTexture = content.Load<Texture2D>(ContentFolder3D + "escena/Yeni klasör/Rock_1_Base_Color");
            Effect = content.Load<Effect>(ContentFolderEffects + "BasicShader");

            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = Effect;
            }

            WorldMatrices = new List<Matrix>();
            RockBoundingCylinders = new List<BoundingCylinder>(); // Inicializa la lista de Bounding Cylinders

            int maxAttempts = 100; // Límite de intentos para posicionar una roca

            for (int i = 0; i < numberOfModels; i++)
            {
                Vector3 newPosition;
                bool positionAccepted;
                int attempt = 0;

                do
                {
                    float randomX = (float)(random.NextDouble() * 2 - 1) * MaxDistance;
                    float randomY = 0;
                    float randomZ = (float)(random.NextDouble() * 2 - 1) * MaxDistance;
                    newPosition = new Vector3(randomX, randomY, randomZ);
                    positionAccepted = true;

                    // Asegurarse de que las rocas no estén muy cerca unas de otras
                    foreach (var matrix in WorldMatrices)
                    {
                        if (Vector3.Distance(newPosition, matrix.Translation) < MinDistance)
                        {
                            positionAccepted = false;
                            break;
                        }
                    }

                    attempt++;

                    if (attempt > maxAttempts)
                    {
                        // Romper el bucle si se exceden los intentos
                        Console.WriteLine("No se encontró un lugar adecuado para una roca después de varios intentos.");
                        break;
                    }

                } while (!positionAccepted);

                var worldMatrix = Matrix.CreateTranslation(newPosition);
                WorldMatrices.Add(worldMatrix);

                // Calcula y almacena el Bounding Cylinder para cada roca
                BoundingCylinder boundingCylinder = CalculateBoundingCylinder(worldMatrix);
                RockBoundingCylinders.Add(boundingCylinder);
            }
        }

        private BoundingCylinder CalculateBoundingCylinder(Matrix worldMatrix)
        {
            // En este ejemplo, consideramos las rocas como cilindros con una altura y radio predeterminados.
            float cylinderHeight = 500f;  // Altura del cilindro (ajusta según el tamaño de la roca)
            float cylinderRadius = 250f;  // Radio del cilindro (ajusta según el tamaño de la roca)

            Vector3 cylinderPosition = worldMatrix.Translation;  // La posición del cilindro coincide con la de la roca

            return new BoundingCylinder(cylinderPosition, cylinderRadius, cylinderHeight / 2); // La mitad de la altura
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            Effect.Parameters["World"].SetValue(world);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(projection);
            Effect.Parameters["ModelTexture"].SetValue(RockTexture);

            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            // Dibujar solo las rocas que están en las listas (es decir, que no han sido eliminadas)
            for (int i = 0; i < WorldMatrices.Count; i++)
            {
                foreach (var mesh in Model.Meshes)
                {
                    var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                    Effect.Parameters["World"].SetValue(meshWorld * WorldMatrices[i]);
                    mesh.Draw();
                }
            }
        }

        public void HandleTankCollision(TankScene tank)
        {

            for (int i = 0; i < RockBoundingCylinders.Count; i++)
            {
                if (RockBoundingCylinders[i].Contains(tank.Position) == ContainmentType.Contains)
                {
                    // La roca ha sido colisionada, la removemos y ajustamos el tanque
                    RockBoundingCylinders.RemoveAt(i);
                    WorldMatrices.RemoveAt(i);

                    break;  // Terminar después de la primera colisión
                }
            }

        }

        // Método para obtener los BoundingCylinders
        public List<BoundingCylinder> GetBoundingCylinders()
        {
            return RockBoundingCylinders;
        }
    }
}
