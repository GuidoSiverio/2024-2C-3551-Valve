using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.Content.Models
{
    class TankScene
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";

        private Model Model { get; set; }
        private Effect Effect { get; set; }

        public Vector3 Position { get; private set; }
        public float Rotation { get; private set; }
        private float Speed { get; set; } = 500f; 
        private float RotationSpeed { get; set; } = 0.5f; 

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
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
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
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            var tankWorld = Matrix.CreateRotationY(Rotation) * Matrix.CreateTranslation(Position);
            Effect.Parameters["World"].SetValue(tankWorld * world);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(projection);
            Effect.Parameters["DiffuseColor"].SetValue(Color.Red.ToVector3());

            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            foreach (var mesh in Model.Meshes)
            {
                var relativeTransform = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                Effect.Parameters["World"].SetValue(relativeTransform * tankWorld * world);
                mesh.Draw();
            }
        }
    }
}