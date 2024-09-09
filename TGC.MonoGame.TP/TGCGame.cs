using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP
{
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        private PlanoEscenario _planoEscenario;

        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }
        private Model Model { get; set; }
        private Effect Effect { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }
        private FollowCamera FollowCamera { get; set; }
        private Model TankModel { get; set; }
        private Matrix TankWorld { get; set; }
        
        // Variables para el tanque
        private float tankSpeed = 200f;
        private float tankRotation = 0f;
        private float rotationSpeed = 1f;
        private float aceleration = 1f;
        private char lastUsed = ' ';
        private Vector3 tankPosition = Vector3.Up;

        public TGCGame()
        {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            var rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };
            GraphicsDevice.RasterizerState = rasterizerState;

            View = Matrix.CreateLookAt(new Vector3(0, 20, 70), Vector3.Zero, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);

            TankWorld = Matrix.Identity;
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            Model = Content.Load<Model>(ContentFolder3D + "tgc-logo/tgc-logo");
            Effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");

            TankModel = Content.Load<Model>("Models/T90");

            // Si el efecto no es un BasicEffect, asegúrate de configurar los parámetros correctos
            // En lugar de castear a BasicEffect, usa parámetros de tu propio efecto
            var modelEffect = Effect; // Usa el efecto cargado directamente para todos los modelos si es un efecto personalizado
            
            _planoEscenario = new PlanoEscenario();
            _planoEscenario.LoadContent(GraphicsDevice);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 movementDirection = Vector3.Zero;

            if (keyboardState.IsKeyDown(Keys.W))
            {
                tankSpeed += 200f * deltaTime; // Aceleración hacia adelante
                movementDirection += Vector3.Forward;
                lastUsed = 'w';
            }
            else if (keyboardState.IsKeyDown(Keys.S))
            {
                tankSpeed += 200f * deltaTime; // Aceleración hacia atrás
                movementDirection += Vector3.Backward;
                lastUsed = 's';
            }
            else
            {
                if (lastUsed == 'w')
                {
                    tankSpeed -= 400f * deltaTime; // Desaceleración hacia adelante
                    movementDirection += Vector3.Forward;
                    tankSpeed = Math.Max(tankSpeed, 0);
                }
                if (lastUsed == 's')
                {
                    tankSpeed -= 400f * deltaTime; // Desaceleración hacia atrás
                    movementDirection += Vector3.Backward;
                    tankSpeed = Math.Max(tankSpeed, 0);
                }
            }

            if (movementDirection != Vector3.Zero)
            {
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    tankRotation += rotationSpeed * deltaTime;
                }
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    tankRotation -= rotationSpeed * deltaTime;
                }

                movementDirection.Normalize();
                Matrix rotationMatrix = Matrix.CreateRotationY(tankRotation);
                movementDirection = Vector3.Transform(movementDirection, rotationMatrix);

                tankPosition += movementDirection * tankSpeed * aceleration * deltaTime;
            }

            TankWorld = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationY(tankRotation) *
                        Matrix.CreateTranslation(tankPosition);
                        
            FollowCamera.Update(gameTime, TankWorld);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Red);

            Effect.Parameters["World"].SetValue(Matrix.Identity);
            Effect.Parameters["View"].SetValue(FollowCamera.View);
            Effect.Parameters["Projection"].SetValue(FollowCamera.Projection);
            Effect.Parameters["DiffuseColor"].SetValue(Color.Brown.ToVector3());

            _planoEscenario.Draw(Effect);

            foreach (var mesh in TankModel.Meshes)
            {
                // Actualiza el parámetro World en el efecto para cada mesh
                Effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * TankWorld);
                mesh.Draw();
            }

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            Content.Unload();
            _planoEscenario.Dispose();
            base.UnloadContent();
        }
    }
}
