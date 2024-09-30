using System;
using System.Collections.Generic;
using BepuPhysics.Constraints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.escena
{

    class HeightMapScene
    {
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderTextures = "Textures/";

        private Texture2D TerrenoHeightMap { get; set; }
        private VertexBuffer TerrainVertexBuffer { get; set; }
        private Texture2D TerrenoColorMapTexture {  get; set; }
        private Texture2D TerrenoTexture1 { get; set; }
        private Texture2D TerrenoTexture2 { get; set; }
        private Texture2D TerrenoNormal1 { get; set; }
        private Texture2D TerrenoNormal2 { get; set; }
        private Effect TerrenoEfecto { get; set; }

        //Valor de Y para cada par (x,z) del Heightmap
        public int[,] HeightmapData { get; private set; }

        private float _scaleXZ;
        private float _scaleY;
        //centro del terreno
        public Vector3 Center { get; private set; }


    public HeightMapScene(GraphicsDevice graphicsDevice, ContentManager content)
        {
            TerrenoEfecto = content.Load<Effect>(ContentFolderEffects + "Terrain"); //importante que sea el efecto terrain
            TerrenoHeightMap = content.Load<Texture2D>(ContentFolderTextures + "HeightMapA/heightmap");
            TerrenoColorMapTexture = content.Load<Texture2D>(ContentFolderTextures + "HeightMapA/colormap2");
            TerrenoTexture1 = content.Load<Texture2D>(ContentFolderTextures + "Textura3/texturemap");
            TerrenoNormal1 = content.Load<Texture2D>(ContentFolderTextures + "Textura3/normalmap");
            TerrenoTexture2 = content.Load<Texture2D>(ContentFolderTextures + "Textura7/texturemap");            
            TerrenoNormal2 = content.Load<Texture2D>(ContentFolderTextures + "Textura7/normalmap");

            LoadHeightmap(graphicsDevice, TerrenoHeightMap, 200, 8, Vector3.Zero);

        }

        public void Draw(Matrix world, Matrix view, Matrix projection, GraphicsDevice graphicsDevice)
        {

            TerrenoEfecto.Parameters["World"].SetValue(world);
            TerrenoEfecto.Parameters["View"].SetValue(view);
            TerrenoEfecto.Parameters["Projection"].SetValue(projection);
            TerrenoEfecto.Parameters["texColorMap"].SetValue(TerrenoColorMapTexture);
            TerrenoEfecto.Parameters["texDiffuseMap"].SetValue(TerrenoTexture1);
            TerrenoEfecto.Parameters["texDiffuseMap2"].SetValue(TerrenoTexture2);
            TerrenoEfecto.Parameters["texNormalMap1"].SetValue(TerrenoNormal1);
            TerrenoEfecto.Parameters["texNormalMap2"].SetValue(TerrenoNormal2);

            graphicsDevice.SetVertexBuffer(TerrainVertexBuffer);

            foreach (var pass in TerrenoEfecto.CurrentTechnique.Passes)
            {
                pass.Apply();
                //graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, PrimitiveCount);
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, TerrainVertexBuffer.VertexCount / 3);
            }

        }


//============================FUNCIONES AUX===================================================================
        private int[,] LoadHeightMap(Texture2D texture)
        {
            var texels = new Color[texture.Width * texture.Height];

            // Obtains each texel color from the texture, note that this is an expensive operation
            texture.GetData(texels);

            var heightmap = new int[texture.Width, texture.Height];

            for (var x = 0; x < texture.Width; x++)
                for (var y = 0; y < texture.Height; y++)
                {
                    var pixel = texels[y * texture.Width + x];
                    //heightmap[x, y] = pixel.R;
                    var intensity = pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f;
                    heightmap[x, y] = (int)intensity;
                }

            return heightmap;
        }

        public void LoadHeightmap(GraphicsDevice graphicsDevice, Texture2D heightmap, float scaleXZ, float scaleY, Vector3 center)
        {
            Center = center;
            _scaleXZ = scaleXZ;
            _scaleY = scaleY;

            float tx_scale = 1; // 50f;

            //cargar heightmap
            HeightmapData = LoadHeightMap(heightmap);
            var width = HeightmapData.GetLength(0);
            var length = HeightmapData.GetLength(1);

            float min_h = 256;
            float max_h = 0;
            for (var i = 0; i < width; i++)
                for (var j = 0; j < length; j++)
                {
                    //HeightmapData[i, j] = 256 - HeightmapData[i, j];
                    if (HeightmapData[i, j] > max_h)
                        max_h = HeightmapData[i, j];
                    if (HeightmapData[i, j] < min_h)
                        min_h = HeightmapData[i, j];
                }

            //Cargar vertices
            var totalVertices = 2 * 3 * (HeightmapData.GetLength(0) - 1) * (HeightmapData.GetLength(1) - 1);
            var dataIdx = 0;
            var data = new VertexPositionNormalTexture[totalVertices];

            center.X = center.X * scaleXZ - width / 2f * scaleXZ;
            center.Y = center.Y * scaleY;
            center.Z = center.Z * scaleXZ - length / 2f * scaleXZ;

            var N = new Vector3[width, length];
            for (var i = 0; i < width - 1; i++)
                for (var j = 0; j < length - 1; j++)
                {
                    var v1 = new Vector3(center.X + i * scaleXZ, center.Y + HeightmapData[i, j] * scaleY,
                        center.Z + j * scaleXZ);
                    var v2 = new Vector3(center.X + i * scaleXZ, center.Y + HeightmapData[i, j + 1] * scaleY,
                        center.Z + (j + 1) * scaleXZ);
                    var v3 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + HeightmapData[i + 1, j] * scaleY,
                        center.Z + j * scaleXZ);
                    N[i, j] = Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));
                }

            for (var i = 0; i < width - 1; i++)
                for (var j = 0; j < length - 1; j++)
                {
                    //Vertices
                    var v1 = new Vector3(center.X + i * scaleXZ, center.Y + HeightmapData[i, j] * scaleY,
                        center.Z + j * scaleXZ);
                    var v2 = new Vector3(center.X + i * scaleXZ, center.Y + HeightmapData[i, j + 1] * scaleY,
                        center.Z + (j + 1) * scaleXZ);
                    var v3 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + HeightmapData[i + 1, j] * scaleY,
                        center.Z + j * scaleXZ);
                    var v4 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + HeightmapData[i + 1, j + 1] * scaleY,
                        center.Z + (j + 1) * scaleXZ);

                    //Coordendas de textura
                    var t1 = new Vector2(i / (float)width, j / (float)length) * tx_scale;
                    var t2 = new Vector2(i / (float)width, (j + 1) / (float)length) * tx_scale;
                    var t3 = new Vector2((i + 1) / (float)width, j / (float)length) * tx_scale;
                    var t4 = new Vector2((i + 1) / (float)width, (j + 1) / (float)length) * tx_scale;

                    //Cargar triangulo 1
                    data[dataIdx] = new VertexPositionNormalTexture(v1, N[i, j], t1);
                    data[dataIdx + 1] = new VertexPositionNormalTexture(v2, N[i, j + 1], t2);
                    data[dataIdx + 2] = new VertexPositionNormalTexture(v4, N[i + 1, j + 1], t4);

                    //Cargar triangulo 2
                    data[dataIdx + 3] = new VertexPositionNormalTexture(v1, N[i, j], t1);
                    data[dataIdx + 4] = new VertexPositionNormalTexture(v4, N[i + 1, j + 1], t4);
                    data[dataIdx + 5] = new VertexPositionNormalTexture(v3, N[i + 1, j], t3);

                    dataIdx += 6;
                }

            //Crear vertexBuffer
            TerrainVertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, totalVertices, BufferUsage.WriteOnly);
            TerrainVertexBuffer.SetData(data);
        }


        public float Height(float x, float z)
        {
            var width = HeightmapData.GetLength(0);
            var length = HeightmapData.GetLength(1);

            var pos_i = x / _scaleXZ + width / 2.0f;
            var pos_j = z / _scaleXZ + length / 2.0f;
            var pi = (int)pos_i;
            var fracc_i = pos_i - pi;
            var pj = (int)pos_j;
            var fracc_j = pos_j - pj;

            if (pi < 0)
                pi = 0;
            else if (pi >= width)
                pi = width - 1;

            if (pj < 0)
                pj = 0;
            else if (pj >= length)
                pj = length - 1;

            var pi1 = pi + 1;
            var pj1 = pj + 1;
            if (pi1 >= width)
                pi1 = width - 1;
            if (pj1 >= length)
                pj1 = length - 1;

            // 2x2 percent closest filtering usual:
            var H0 = HeightmapData[pi, pj];
            var H1 = HeightmapData[pi1, pj];
            var H2 = HeightmapData[pi, pj1];
            var H3 = HeightmapData[pi1, pj1];
            var H = (H0 * (1 - fracc_i) + H1 * fracc_i) * (1 - fracc_j) + (H2 * (1 - fracc_i) + H3 * fracc_i) * fracc_j;

            return H * _scaleY;
        }
    }
}
