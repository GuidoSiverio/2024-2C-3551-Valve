using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Content.Models;


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

        //menu
        private Menu _menu;
        private HUD _hud;
        private bool _juegoIniciado = false;
        public float screenHeight;
        public float screenWidth;

        //modelos, efectos y camara 
        private TankScene Panzer { get; set; }
        private ArbolScene Arbol { get; set; }
        private RockScene Roca { get; set; }
        public SpriteBatch SpriteBatch { get; set; } //esto para que sera ?

        private FollowCamera FollowCamera { get; set; }
        private FreeCamera FreeCamera { get; set; }
        private StaticCamera _staticCamera;


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
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            World = Matrix.Identity;
            View = Matrix.CreateLookAt(Vector3.One * 1500, Vector3.Zero, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio,
                0.1f, 250000f);
            screenHeight = GraphicsDevice.Viewport.Height;
            screenWidth = GraphicsDevice.Viewport.Width;
            _staticCamera = new StaticCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(400, 200, 1300),
                Vector3.Forward, Vector3.Up);

            _menu = new Menu(GraphicsDevice, Content);
            _hud = new HUD(screenWidth, screenHeight);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 600, -1000));

            Panzer = new TankScene(Content);
            Arbol = new ArbolScene(Content, 50);
            Roca = new RockScene(Content, 50);

            _menu.LoadContent(Content);
            _hud.LoadContent(Content);

            base.LoadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            if (!_juegoIniciado)
            {
                Panzer.Position = new Vector3(-5000, 0, -11000);
                _menu.Update(ref _juegoIniciado);
            }

            if (_juegoIniciado)
            {
                FollowCamera.Update(Panzer.Position, Panzer.TurretRotation, Panzer.TurretElevation);
                Panzer.Update(gameTime, Keyboard.GetState(), Mouse.GetState());
            }

            if (_juegoIniciado && Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                _juegoIniciado = false;
                _menu.StartMusic();
            }
          
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black); //fondo

            if (!_juegoIniciado)
            {
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.BlendState = BlendState.AlphaBlend;

                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

                Camera camara = _staticCamera;
                Panzer.Draw(World, camara.View, camara.Projection);


                SpriteBatch.Begin();
                _menu.Draw(SpriteBatch);
                SpriteBatch.End();

            }
            else
            {
                Matrix View = FollowCamera.ViewMatrix;
                Matrix ProjectionCamera = FollowCamera.ProjectionMatrix;
                Arbol.Draw(World, View, ProjectionCamera);
                Roca.Draw(World, View, ProjectionCamera);
                Panzer.Draw(World, View, ProjectionCamera);
                SpriteBatch.Begin();
                _hud.Draw(SpriteBatch);
                SpriteBatch.End();
            }



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