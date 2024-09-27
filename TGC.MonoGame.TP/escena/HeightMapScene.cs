using System;
using System.Collections.Generic;
using BepuPhysics.Constraints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Content.Models
{

    class HeightMapScene
    {
        //public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderTextures = "Textures/";

        private Texture2D TerrenoHeightMap { get; set; }
        private float _scaleXZ = 50f; //factor de escala para el suelo ancho y largo
        private float _scaleY = 4f; //factor de escala para la altura 
        //private int PrimitiveCount { get; set; }
        //private Texture2D HeightMapTexture { get; set; }
        private VertexBuffer TerrainVertexBuffer { get; set; }
        //private IndexBuffer TerrainIndexBuffer { get; set; }
        
        //private BasicEffect Effect { get; set; }
        private Texture2D TerrenoColorMapTexture {  get; set; }
        private Texture2D TerrenoTexture { get; set; }
        private Texture2D TerrenoTexture2 { get; set; }
        private Effect TerrenoEfecto { get; set; }

        //Valor de Y para cada par (x,z) del Heightmap
        public int[,] HeightmapData { get; private set; }
        //centro del terreno
        public Vector3 Center { get; private set; }


    public HeightMapScene(GraphicsDevice graphicsDevice, ContentManager content)
        {
            TerrenoEfecto = content.Load<Effect>(ContentFolderEffects + "Terrain"); //importante que sea el efecto terrain
            TerrenoHeightMap = content.Load<Texture2D>(ContentFolderTextures + "hmap5/heightmap");
            TerrenoColorMapTexture = content.Load<Texture2D>(ContentFolderTextures + "hmap5/colormap");
            TerrenoTexture = content.Load<Texture2D>(ContentFolderTextures + "hmap5/texturemap");
            TerrenoTexture2 = content.Load<Texture2D>(ContentFolderTextures + "hmap5/texturemap3");

            LoadHeightmap(graphicsDevice, TerrenoHeightMap, 150, 10, Vector3.Zero);

            //float[,] heightMap= LoadHeightMap(HeightMap);

            //PrimitiveCount = (heightMap.GetLength(0) -1) * (heightMap.GetLength(1) -1) * 2;

            //CreateVertexBuffer(heightMap, graphicsDevice);

            //CreateIndexBuffer(heightMap, graphicsDevice);

            //TerrainTexture = content.Load<Texture2D>(ContentFolderTextures + "terrain-texture-3");

            
            //Effect = new BasicEffect(graphicsDevice)
            //{
            //    World = Matrix.Identity,
            //    TextureEnabled = true,
            //    Texture = TerrainTexture
            //};
            //Effect.EnableDefaultLighting();

        }

        public void Draw(Matrix world, Matrix view, Matrix projection, GraphicsDevice graphicsDevice)
        {

            TerrenoEfecto.Parameters["World"].SetValue(world);
            TerrenoEfecto.Parameters["View"].SetValue(view);
            TerrenoEfecto.Parameters["Projection"].SetValue(projection);
            //TerrenoEfecto.View = view;
            //TerrenoEfecto.Projection = projection;
            //TerrenoEfecto.Parameters["DiffuseColor"].SetValue(Color.Red.ToVector3());
            TerrenoEfecto.Parameters["texColorMap"].SetValue(TerrenoColorMapTexture);
            TerrenoEfecto.Parameters["texDiffuseMap"].SetValue(TerrenoTexture);
            TerrenoEfecto.Parameters["texDiffuseMap2"].SetValue(TerrenoTexture2);

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
        /*
        private void CreateVertexBuffer(float[,] heightMap, GraphicsDevice graphicsDevice)
        {

            int widthHeightMap = heightMap.GetLength(0);//tomamos la cantidad de filas que seria el ancho del mapa
            int LegthHeightMap = heightMap.GetLength(1);//tomamos la cantidad de columnas que seria el largo del mapa

            var offsetX = widthHeightMap * _scaleXZ * 0.5f;
            var offsetZ = LegthHeightMap * _scaleXZ * 0.5f;

            // Create temporary array of vertices.
            var vertices = new VertexPositionTexture[widthHeightMap * LegthHeightMap];

            var index = 0;
            Vector3 position;
            Vector2 textureCoordinates;

            for (var x = 0; x < widthHeightMap; x++)
                for (var z = 0; z < LegthHeightMap; z++)
                {
                    position = new Vector3(x * _scaleXZ - offsetX, heightMap[x, z] * _scaleY, z * _scaleXZ - offsetZ);
                    textureCoordinates = new Vector2((float)x / widthHeightMap, (float)z / LegthHeightMap);
                    vertices[index] = new VertexPositionTexture(position, textureCoordinates);
                    index++;
                }

            // Create the actual vertex buffer
            TerrainVertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionTexture.VertexDeclaration, widthHeightMap * LegthHeightMap, BufferUsage.None);
            TerrainVertexBuffer.SetData(vertices);
        }

        private void CreateIndexBuffer(float[,] heightMap, GraphicsDevice graphicsDevice)
        {
            int quadsInX = heightMap.GetLength(0) - 1;
            int quadsInZ = heightMap.GetLength(1) - 1;

            var indexCount = 3 * 2 * quadsInX * quadsInZ;

            var indices = new ushort[indexCount];
            var index = 0;

            int right;
            int top;
            int bottom;

            var vertexCountX = quadsInX + 1;
            for (var x = 0; x < quadsInX; x++)
                for (var z = 0; z < quadsInZ; z++)
                {
                    right = x + 1;
                    bottom = z * vertexCountX;
                    top = (z + 1) * vertexCountX;

                    //  d __ c  
                    //   | /|
                    //   |/_|
                    //  a    b

                    var a = (ushort)(x + bottom);
                    var b = (ushort)(right + bottom);
                    var c = (ushort)(right + top);
                    var d = (ushort)(x + top);

                    // ACB
                    indices[index] = a;
                    index++;
                    indices[index] = c;
                    index++;
                    indices[index] = b;
                    index++;

                    // ADC
                    indices[index] = a;
                    index++;
                    indices[index] = d;
                    index++;
                    indices[index] = c;
                    index++;
                }

            TerrainIndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indexCount, BufferUsage.None);
            TerrainIndexBuffer.SetData(indices);
        }
        */
    }
}
