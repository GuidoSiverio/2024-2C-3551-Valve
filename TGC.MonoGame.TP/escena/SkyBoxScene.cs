using System;
using System.Collections.Generic;
using BepuPhysics.Constraints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.escena
{

    class SkyBoxScene
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderTextures = "Textures/";


        private Model Model { get; set; }
        private TextureCube Texture { get; set; }
        private Effect Effect { get; set; }
        private float Size { get; set; }

        public SkyBoxScene(ContentManager content, float size)
        {
            Model = content.Load<Model>(ContentFolder3D + "skybox/cube");
            Effect = content.Load<Effect>(ContentFolderEffects + "Skybox");
            Texture = content.Load<TextureCube>(ContentFolderTextures + "Skybox/skybox");
            Size = size;            
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (var mesh in Model.Meshes)
                {
                    foreach (var meshPart in mesh.MeshParts)
                    {
                        meshPart.Effect = Effect;
                        meshPart.Effect.Parameters["World"].SetValue(world * Matrix.CreateScale(Size));// * Matrix.CreateTranslation(cameraPosition));
                        meshPart.Effect.Parameters["View"].SetValue(view);
                        meshPart.Effect.Parameters["Projection"].SetValue(projection);
                        meshPart.Effect.Parameters["SkyBoxTexture"].SetValue(Texture);
                        meshPart.Effect.Parameters["CameraPosition"].SetValue(Vector3.Zero);
                    }
                    mesh.Draw();
                }
            }

        }
    }
}
