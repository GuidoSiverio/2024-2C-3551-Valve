using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TGC.MonoGame.TP{
    class Projectile
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";

        private Model Model { get; set; }

        private Texture2D ProjectileTexture { get; set; }

        private Effect Effect { get; set; }

        public Projectile(ContentManager content)
        {
            Model = content.Load<Model>(ContentFolder3D + "cannonball/Cannonball");
            ProjectileTexture = content.Load<Texture2D>(ContentFolder3D + "cannonball/Cannonball texture");

            Effect = content.Load<Effect>(ContentFolderEffects + "BasicShader");

            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = Effect;
            }  
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            Effect.Parameters["World"].SetValue(world);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(projection);
            //Effect.Parameters["DiffuseColor"].SetValue(Color.Green.ToVector3());
            Effect.Parameters["ModelTexture"].SetValue(ProjectileTexture);

            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            foreach (var mesh in Model.Meshes)
            {
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];                
                Effect.Parameters["World"].SetValue(meshWorld * world * Matrix.CreateScale(20f));
                mesh.Draw();
                
            }
        }

    }
}