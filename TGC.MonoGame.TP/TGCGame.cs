using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Content.Models;


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

        private ArbolScene Arbol { get; set; }

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);

            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;

            // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }

        private GraphicsDeviceManager Graphics { get; }
        private Model Panzer { get; set; }
        private Effect PanzerEffect { get; set; }
        private Effect SceneEffect { get; set; }
        private Matrix World { get; set; }
        public SpriteBatch SpriteBatch { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }

        private FreeCamera FreeCamera { get; set; }
        //private float Rotation { get; set; }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            FreeCamera = new FreeCamera(Vector3.Zero, 10f, 0.01f);

            World = Matrix.Identity * Matrix.CreateScale(1, 1, 1) * Matrix.CreateTranslation(0, -20, 200);
            View = Matrix.CreateLookAt(Vector3.One * 2500, new Vector3(0, 0, 0), Vector3.Up);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f,
                    2500000f);

            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Panzer = Content.Load<Model>(ContentFolder3D + "Panzer/Panzer");
            PanzerEffect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            SceneEffect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            Arbol = new ArbolScene(Content, 50);


            foreach (var mesh in Panzer.Meshes)
            {
                // Un mesh puede tener mas de una mesh part (cada una puede tener su propio efecto).
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = PanzerEffect;
            }


            base.LoadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logica de actualizacion del juego.
            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                //Salgo del juego.
                Exit();
            }

            FreeCamera.Update(gameTime, Keyboard.GetState(), Mouse.GetState());

            //Rotation += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            //World = Matrix.CreateRotationY(Rotation);


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            Matrix View = FreeCamera.ViewMatrix;
            Arbol.Draw(Matrix.Identity, View, Projection);

            PanzerEffect.Parameters["View"].SetValue(View);
            PanzerEffect.Parameters["Projection"].SetValue(Projection);
            PanzerEffect.Parameters["DiffuseColor"].SetValue(Color.Red.ToVector3());


            var modelMeshesBaseTransforms = new Matrix[Panzer.Bones.Count];

            Panzer.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            foreach (var mesh in Panzer.Meshes)
            {
                var relativeTransform = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                PanzerEffect.Parameters["World"].SetValue(relativeTransform * World);
                mesh.Draw();
            }
        }

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }
    }
}