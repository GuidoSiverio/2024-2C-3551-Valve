using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TGC.MonoGame.TP.Content.Models;

namespace TGC.MonoGame.TP{
    class Projectile
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";

        private Model Model { get; set; }

        private Texture2D ProjectileTexture { get; set; }

        private Effect Effect { get; set; }

        public Vector3 Position { get; private set; }

        public Matrix ProjectilePosition { get; private set; }

        public float projectileVelocity = 5000f;

        public Vector3 Direction { get; set; }

        public Projectile(ContentManager content, TankScene panzer)
        {
            Model = content.Load<Model>(ContentFolder3D + "cannonball/Cannonball");
            ProjectileTexture = content.Load<Texture2D>(ContentFolder3D + "cannonball/Cannonball texture");

            Effect = content.Load<Effect>(ContentFolderEffects + "BasicShader");

            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = Effect;
            }  

            Position = panzer.Position + Vector3.Up * 220;
            ProjectilePosition = Matrix.CreateTranslation(Position);
            Direction = panzer.turretWorld.Forward;
        }

        public void Update(GameTime gameTime){
            
            float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position -= Direction * projectileVelocity * time;
            this.ProjectilePosition = Matrix.CreateTranslation(Position);

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
                Effect.Parameters["World"].SetValue(meshWorld * world * Matrix.CreateScale(20f) * ProjectilePosition);
                mesh.Draw();
                
            }
        }

    }
}