using Offsetter.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Offsetter
{
    public class WireframeRenderer : Renderer
    {
        public Vertex[] data = null!;
        public int LineWidth { get; set; } = 1;

        protected PrimitiveType drawingMode = PrimitiveType.LineStrip;

        public WireframeRenderer(Shader shader, VertexList vertices, VColor color)
            : base(shader, vertices.Count, color)
        {
            CtorCommon(vertices.ToArray());
        }

        public WireframeRenderer(Shader shader, Vertex[] vertices, VColor color)
            : base(shader, vertices.Length, color)
        {
            CtorCommon(vertices);
        }

        private void CtorCommon(Vertex[] vertices)
        {
            data = vertices;

            // Both vWireframeShaderCode and vWindowingShaderCode have the same layout.
            //    layout(location = 0) in vec2 vrtx2d;
            GL.VertexArrayAttribBinding(vertexArray, 0, 0);
            GL.EnableVertexArrayAttrib(vertexArray, 0);
            GL.VertexArrayAttribFormat(
                vertexArray,
                0,                      // attribute index, from the shader location = 0
                2,                      // size of attribute, vec2
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                0);                     // relative offset, first item

            // link the vertex array and buffer and provide the stride as size of Vertex
            GL.VertexArrayVertexBuffer(vertexArray, 0, bufferID, IntPtr.Zero, Vertex.Size);
        }

        public override void Render()
        {
            if (!IsEnabled)
                return;

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferID);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * Vertex.Size, data, BufferUsageHint.StaticDraw);

            GL.UseProgram(programID);
            GL.BindVertexArray(vertexArray);
            shader.SetVector4("curveColor", colorTable[color]);

            GL.LineWidth(LineWidth);
            GL.DrawArrays(drawingMode, 0, data.Length);
        }
    }
}
