using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.TP.Content.Models;

namespace TGC.MonoGame.TP
{
    public class HUD
    {

        public const string ContentFolderTextures = "Textures/";
        public const string ContentFolderFonts = "Fonts/";

        private float ScreenWidth { get; set; }
        private float ScreenHeight { get; set; }

        private Rectangle lifeBar;

        private Texture2D lifeBar11;
        private Texture2D lifeBar10;
        private Texture2D lifeBar9;
        private Texture2D lifeBar8;
        private Texture2D lifeBar7;
        private Texture2D lifeBar6;
        private Texture2D lifeBar5;
        private Texture2D lifeBar4;
        private Texture2D lifeBar3;
        private Texture2D lifeBar2;
        private Texture2D lifeBar1;

        private Texture2D lifeBarTexture;


        private Texture2D CrossHairTexture;

        public float Convergence { get; set; } = 2000f;
        private Vector2 CrossHairPosition;

        private SpriteFont FontArial;


        private float puntos;

        public int _maxLifeBarWidth;
        public bool siguienteOleada;
        public bool juegoFinalizado;
        
        public float elapsedTime { get; set; }
        public float time { get; set; }
        public float Oleada { get; set; }

        public HUD(float screenWidth, float screenHeight)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;

            float barWidth = screenWidth / 10;
            float barHeight = screenHeight / 20;
            float barY = 10;
            float barX = screenWidth - 2 * barWidth + 30;

            lifeBar = new Rectangle((int)barX, (int)barY, (int)barWidth, (int)barHeight);
        }

        public void LoadContent(ContentManager Content)
        {
            CrossHairTexture = Content.Load<Texture2D>(ContentFolderTextures + "/punto-de-mira");
            FontArial = Content.Load<SpriteFont>(ContentFolderFonts + "arial");
            
            lifeBar1 = Content.Load<Texture2D>(ContentFolderTextures + "HUD/lifebar/1");
            lifeBar2 = Content.Load<Texture2D>(ContentFolderTextures + "HUD/lifebar/2");
            lifeBar3 = Content.Load<Texture2D>(ContentFolderTextures + "HUD/lifebar/3");
            lifeBar4 = Content.Load<Texture2D>(ContentFolderTextures + "HUD/lifebar/4");
            lifeBar5 = Content.Load<Texture2D>(ContentFolderTextures + "HUD/lifebar/5");
            lifeBar6 = Content.Load<Texture2D>(ContentFolderTextures + "HUD/lifebar/6");
            lifeBar7 = Content.Load<Texture2D>(ContentFolderTextures + "HUD/lifebar/7");
            lifeBar8 = Content.Load<Texture2D>(ContentFolderTextures + "HUD/lifebar/8");
            lifeBar9 = Content.Load<Texture2D>(ContentFolderTextures + "HUD/lifebar/9");
            lifeBar10 = Content.Load<Texture2D>(ContentFolderTextures + "HUD/lifebar/10");
            lifeBar11 = Content.Load<Texture2D>(ContentFolderTextures + "HUD/lifebar/11");


            lifeBarTexture = lifeBar1;

            _maxLifeBarWidth = lifeBar.Width;
            siguienteOleada = false;
            juegoFinalizado = false;
        }

        public void Update(TankScene Panzer, ref Viewport viewport, float TanksEliminados)
        {
            CrossHairPosition = new Vector2(ScreenWidth / 2 - 25, (float)((Math.Tan(Panzer.TurretElevation) * Convergence) + (ScreenHeight / 2) - 25));
            lifeBarTexture = lifeBar1;

            if (Panzer._currentLife == 50)
            {
                lifeBarTexture = lifeBar1;
            }
            else if (Panzer._currentLife == 45)
            {
                lifeBarTexture = lifeBar2;
            }
            else if (Panzer._currentLife == 40)
            {
                lifeBarTexture = lifeBar3;
            }
            else if (Panzer._currentLife == 35)
            {
                lifeBarTexture = lifeBar4;
            }
            else if (Panzer._currentLife == 30)
            {
                lifeBarTexture = lifeBar5;
            }
            else if (Panzer._currentLife == 25)
            {
                lifeBarTexture = lifeBar6;
            }
            else if (Panzer._currentLife == 20)
            {
                lifeBarTexture = lifeBar7;
            }
            else if (Panzer._currentLife == 15)
            {
                lifeBarTexture = lifeBar8;
            }
            else if (Panzer._currentLife == 10)
            {
                lifeBarTexture = lifeBar9;
            }
            else if(Panzer._currentLife == 5)
            {
                lifeBarTexture = lifeBar10;
            }
            else
            {
                lifeBarTexture = lifeBar11;
            }

            puntos = TanksEliminados;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                    CrossHairTexture,
                    CrossHairPosition,
                    null, Color.Black, 0f, Vector2.Zero, 0.1f, SpriteEffects.None, 0.8f
              );
            spriteBatch.Draw(
                lifeBarTexture,
                lifeBar,
                Color.Yellow
             );
            
            spriteBatch.DrawString(FontArial, "OLEADA: " + Oleada, new Vector2(ScreenWidth - 250, ScreenHeight - 150), Color.Blue);

            spriteBatch.DrawString(FontArial, "PUNTOS: " + puntos, new Vector2(ScreenWidth - 250, ScreenHeight - 100), Color.Blue);

            spriteBatch.DrawString(FontArial, "TIEMPO: " + (int)elapsedTime + " SEGS", new Vector2(ScreenWidth - 250, ScreenHeight - 50), Color.Blue);

            if(siguienteOleada)
            {
                spriteBatch.DrawString(FontArial, "OLEADA COMPLETADA", new Vector2(ScreenWidth - 1055, ScreenHeight - 525), Color.GreenYellow);
                spriteBatch.DrawString(FontArial, "SIGUIENTE OLEADA: " + Oleada, new Vector2(ScreenWidth - 1050, ScreenHeight - 500), Color.GreenYellow);
                spriteBatch.DrawString(FontArial, "TIEMPO SIGUIENTE OLEADA: " + (int)time + " SEGS", new Vector2(ScreenWidth - 1110, ScreenHeight - 475), Color.GreenYellow);
            }
            if(juegoFinalizado)
            {
                spriteBatch.DrawString(FontArial, "OLEADA COMPLETADA", new Vector2(ScreenWidth - 1060, ScreenHeight - 525), Color.GreenYellow);
                spriteBatch.DrawString(FontArial, "JUEGO FINALIZADO", new Vector2(ScreenWidth - 1045, ScreenHeight - 500), Color.GreenYellow);
                spriteBatch.DrawString(FontArial, "PUNTAJE FINAL: " + puntos, new Vector2(ScreenWidth - 1040, ScreenHeight - 475), Color.GreenYellow);
            }
            

        }
    }
}