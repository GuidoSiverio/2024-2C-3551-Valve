using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP;

public class PlanoEscenario
{
    private VertexPositionColor[] vertices; // Vértices para el plano
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        // Definir los vértices del plano con color marrón
        vertices = new VertexPositionColor[4]
        {
            new VertexPositionColor(new Vector3(-50, 0, -50), Color.Brown), // Vértice 1
            new VertexPositionColor(new Vector3(50, 0, -50), Color.Brown),  // Vértice 2
            new VertexPositionColor(new Vector3(-50, 0, 50), Color.Brown),  // Vértice 3
            new VertexPositionColor(new Vector3(50, 0, 50), Color.Brown)    // Vértice 4
        };

        // Definir los índices para dibujar los dos triángulos del plano.
        var indices = new ushort[6]
        {
            0, 1, 3, // Primer triángulo
            0, 3, 2  // Segundo triángulo
        };

        // Crear el VertexBuffer
        _vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionColor.VertexDeclaration, 4, BufferUsage.None);
        _vertexBuffer.SetData(vertices);

        // Crear el IndexBuffer
        _indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.None);
        _indexBuffer.SetData(indices);
    }

    public void Draw(Effect effect)
    {
        var graphicsDevice = effect.GraphicsDevice;

        // Configurar el dispositivo gráfico para dibujar
        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;

        // Dibujar el plano
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }
    }

    public void Dispose()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
    }
}
