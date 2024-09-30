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
        private ArbolScene Arbol {  get; set; }
        private RockScene Roca { get; set; }
        private HeightMapScene HeightMap { get; set; }
        private SkyBoxScene SkyBox { get; set; }
        public SpriteBatch SpriteBatch { get; set; }//esto para que sera ?

        private FreeCamera FreeCamera { get; set; }

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
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            FreeCamera = new FreeCamera(new Vector3(-2000, 800, -100), 500f, 0.5f);

            Panzer = new TankScene(Content);
            Arbol = new ArbolScene(Content,50);
            Roca = new RockScene(Content,50);


            HeightMap = new HeightMapScene(GraphicsDevice, Content);
            //SkyBox = new SkyBoxScene(Content,50);

            base.LoadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))                
                Exit();//para salir facilmente


            FreeCamera.Update(gameTime, Keyboard.GetState(), Mouse.GetState());
           
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);//fondo

            Matrix View = FreeCamera.ViewMatrix;
            HeightMap.Draw(World, View, Projection, GraphicsDevice);
            float alturaHeigth = HeightMap.Height(0,0);

            Arbol.Draw(World, View, Projection, HeightMap);
            Roca.Draw(World, View, Projection, HeightMap);
            Panzer.Draw(World * Matrix.CreateTranslation(0,alturaHeigth,0), View, Projection);
            
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