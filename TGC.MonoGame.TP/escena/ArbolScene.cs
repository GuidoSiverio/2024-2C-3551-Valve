﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TGC.MonoGame.TP.Content.Models
{
    class ArbolScene
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";

        private Model Model { get; set; }

        public const float MaxDistance = 10000f;
        public const float MinDistance = 1000f;


        private List<Matrix> WorldMatrices { get; set; }

        private Effect Effect { get; set; }
        private static Random random = new Random();

        public ArbolScene(ContentManager content, int numberOfModels)
        {
            Model = content.Load<Model>(ContentFolder3D + "escena/plant");

            Effect = content.Load<Effect>(ContentFolderEffects + "BasicShader");

            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = Effect;
            }

            WorldMatrices = new List<Matrix>();
            for (int i = 0; i < numberOfModels; i++)
            {
                Vector3 newPosition;
                bool positionAccepted;
                do
                {
                    float randomX = (float)(random.NextDouble() * 2 - 1) * MaxDistance;
                    float randomY = 0;
                    float randomZ = (float)(random.NextDouble() * 2 - 1) * MaxDistance;

                    newPosition = new Vector3(randomX, randomY, randomZ);
                    positionAccepted = true;
                    foreach (var matrix in WorldMatrices)
                    {
                        if (Vector3.Distance(newPosition, matrix.Translation) < MinDistance)
                        {
                            positionAccepted = false;
                            break;
                        }
                    }
                } while (!positionAccepted);

                WorldMatrices.Add(Matrix.CreateTranslation(newPosition));
            }
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            Effect.Parameters["World"].SetValue(world);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(projection);
            Effect.Parameters["DiffuseColor"].SetValue(Color.Green.ToVector3());

            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            foreach (var mesh in Model.Meshes)
            {
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];

                foreach (var worldMatrix in WorldMatrices)
                {
                    Effect.Parameters["World"].SetValue(meshWorld * worldMatrix);

                    mesh.Draw();
                }
            }
        }
    }
}