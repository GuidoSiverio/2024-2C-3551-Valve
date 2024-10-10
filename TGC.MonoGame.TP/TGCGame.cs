using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Content.Models;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP
{
    public class TGCGame : Game
    {
        // Ubicación de modelos, efectos, sonidos y texturas
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        private GraphicsDeviceManager Graphics { get; }

        // Matrices de mundo, vista y proyección
        private Matrix World { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }

        // Modelos, efectos y cámara
        private TankScene Panzer { get; set; }
        private ArbolScene Arbol { get; set; }
        private RockScene Roca { get; set; }
        public SpriteBatch SpriteBatch { get; set; }

        private FollowCamera FollowCamera { get; set; }
        private Projectile Projectile { get; set; }

        private bool isProjectileFired = false;
        private List<TankScene> enemyTanks = new List<TankScene>();

        public TGCGame()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            GraphicsDevice.BlendState = BlendState.Opaque;
            Graphics.ApplyChanges();
            World = Matrix.Identity;
            View = Matrix.CreateLookAt(Vector3.One * 1500, Vector3.Zero, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 250000f);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 600, -1000));
            Panzer = new TankScene(Content, false);
            enemyTanks.Add(new TankScene(Content, true));
            enemyTanks.Add(new TankScene(Content, true));
            enemyTanks.Add(new TankScene(Content, true));
            Arbol = new ArbolScene(Content, 50);
            Roca = new RockScene(Content, 50);
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Si el espacio es presionado, se dispara el proyectil
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && !isProjectileFired)
            {
                Projectile = new Projectile(Content, Panzer);
                isProjectileFired = true;
            }

            if (isProjectileFired)
                Projectile.Update(gameTime);

            // Actualizar el tanque pasando la lista de objetos (en este caso las rocas)
            List<object> gameObjects = new List<object> { Roca }; // En el futuro podrías añadir más tipos de objetos aquí.
            Panzer.Update(gameTime, Keyboard.GetState(), Mouse.GetState(), gameObjects);

            // Actualiza la cámara para seguir al tanque
            FollowCamera.Update(Panzer.Position, Panzer.TurretRotation, Panzer.TurretElevation);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            Matrix View = FollowCamera.ViewMatrix;
            Matrix ProjectionCamera = FollowCamera.ProjectionMatrix;

            // Dibuja todos los objetos de la escena
            Arbol.Draw(World, View, ProjectionCamera);
            Roca.Draw(World, View, ProjectionCamera);
            Panzer.Draw(World, View, ProjectionCamera);
            enemyTanks.ForEach(tank => tank.Draw(World, View, ProjectionCamera));

            if (isProjectileFired)
                Projectile.Draw(World, View, ProjectionCamera);

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            // Libera los recursos.
            Content.Unload();
            base.UnloadContent();
        }
    }
}
