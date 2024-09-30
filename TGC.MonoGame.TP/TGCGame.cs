using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Content.Models;
using TGC.MonoGame.TP.escena;


namespace TGC.MonoGame.TP
{
    public class TGCGame : Game
    {
        //ubicacion de modelos, efectos, sonidos y textura
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        private GraphicsDeviceManager Graphics { get; }

        //Matriz mundo, vista y proyeccion
        private Matrix World { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }


        //modelos, efectos y camara 
        private TankScene Panzer { get; set; }
        private ArbolScene Arbol { get; set; }
        private RockScene Roca { get; set; }
        private HeightMapScene HeightMap { get; set; }

        private Vector3 _traslationH;
        private SkyBoxScene SkyBox { get; set; }

        private FollowCamera FollowCamera { get; set; }
        private FreeCamera FreeCamera { get; set; }
        
        private Projectile Projectile { get; set; }

        private Matrix ProjectilePosition = Matrix.Identity;

        private Vector3 position = Vector3.Zero;

        private bool isProjectileFired = false;

        private Vector3 projectileVelocity = Vector3.Zero;

        private List<TankScene> enemyTanks = new List<TankScene>();

        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            World = Matrix.Identity;
            View = Matrix.CreateLookAt(Vector3.One * 1500, Vector3.Zero, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 250000f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 600, -1000));

            Panzer = new TankScene(Content, false);
            enemyTanks.Add(new TankScene(Content, true));
            enemyTanks.Add(new TankScene(Content, true));
            enemyTanks.Add(new TankScene(Content, true));
            Arbol = new ArbolScene(Content,50);
            Roca = new RockScene(Content,50);


            HeightMap = new HeightMapScene(GraphicsDevice, Content);
            //SkyBox = new SkyBoxScene(Content,50);

            base.LoadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            _traslationH = new Vector3(0,HeightMap.Height(0, 0),0);

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit(); //para salir facilmente

            if (Keyboard.GetState().IsKeyDown(Keys.Space)){
                Projectile = new Projectile(Content, Panzer);
                //projectileVelocity += position * 1000f;
                isProjectileFired = true;
            }
                   

            if (isProjectileFired){
                Projectile.Update(gameTime);
                //position -=  projectileVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                //ProjectilePosition += Matrix.CreateTranslation(position);
            }
            
            Panzer.Update(gameTime, Keyboard.GetState(),Mouse.GetState());
            var posicionActual = Panzer.Position;
            posicionActual.Y += HeightMap.Height(Panzer.Position.X, Panzer.Position.Z);

            FollowCamera.Update(posicionActual, Panzer.TurretRotation,Panzer.TurretElevation);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black); //fondo

            Matrix View = FollowCamera.ViewMatrix;
            Matrix ProjectionCamera = FollowCamera.ProjectionMatrix;

            HeightMap.Draw(World, View, Projection, GraphicsDevice);

            Arbol.Draw(World, View, ProjectionCamera, HeightMap);
            Roca.Draw(World, View, ProjectionCamera, HeightMap);
            Panzer.Draw(World, View, ProjectionCamera, HeightMap);

            
            enemyTanks.ForEach(tank => tank.Draw(World, View, ProjectionCamera, HeightMap));
            if (isProjectileFired)
                Projectile.Draw(World, View, ProjectionCamera);
            

            //SkyBox.Draw(World, View, FreeCamera.);

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }
    }
}