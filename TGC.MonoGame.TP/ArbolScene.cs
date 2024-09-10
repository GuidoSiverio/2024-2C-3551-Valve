using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TGC.MonoGame.TP.Content.Models
{

    class ArbolScene
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";

        private Model Model { get; set; }

        public const float Distance = 2100f;

        private List<Matrix> WorldMatrices { get; set; }

        private Effect Effect { get; set; }

        public ArbolScene(ContentManager content)
        {
            Model = content.Load<Model>(ContentFolder3D + "escena/plant");

            Effect = content.Load<Effect>(ContentFolderEffects + "BasicShader");

            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = Effect;
            }

            WorldMatrices = new List<Matrix>()
            {
                Matrix.Identity,
                Matrix.CreateTranslation(Vector3.Right * Distance),
                Matrix.CreateTranslation(Vector3.Left * Distance),
                Matrix.CreateTranslation(Vector3.Forward * Distance),
                Matrix.CreateTranslation(Vector3.Backward * Distance),
                Matrix.CreateTranslation((Vector3.Forward + Vector3.Right) * Distance),
                Matrix.CreateTranslation((Vector3.Forward + Vector3.Left) * Distance),
                Matrix.CreateTranslation((Vector3.Backward + Vector3.Right) * Distance),
                Matrix.CreateTranslation((Vector3.Backward + Vector3.Left) * Distance),
            };
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            Effect.Parameters["World"].SetValue(world);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(projection);

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
