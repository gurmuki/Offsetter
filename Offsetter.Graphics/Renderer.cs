using System;
using System.Collections.Generic;
using Offsetter.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Offsetter
{
    public enum VColor { BLACK, RED, GREEN, BLUE }

    public abstract class Renderer : IDisposable
    {
        protected static Dictionary<VColor, Vector4> colorTable = null!;

        protected Shader shader;
        protected readonly int bufferID;
        protected readonly int vertexArray;  // VAO (vertext array object)

        public readonly int vertexCount;
        public readonly VColor color;

        public readonly int programID;

        private static void ColorTableInitialize()
        {
            colorTable = new Dictionary<VColor, Vector4>();
            colorTable[VColor.BLACK] = new Vector4(0f, 0f, 0f, 1f);
            colorTable[VColor.RED] = new Vector4(1f, 0f, 0f, 1f);
            colorTable[VColor.GREEN] = new Vector4(0f, 0.8f, 0f, 1f);
            colorTable[VColor.BLUE] = new Vector4(0f, 0f, 1f, 1f);
        }

        protected Renderer(Shader shader, int vertexCount, VColor color)
        {
            this.shader = shader;
            this.programID = shader.Handle;
            this.vertexCount = vertexCount;
            this.color = color;
            vertexArray = GL.GenVertexArray();
            bufferID = GL.GenBuffer();

            GL.BindVertexArray(vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferID);

            if (colorTable == null)
                ColorTableInitialize();
        }

        public bool IsEnabled { get; set; } = true;

        public void SetMatrix4(string name, Matrix4 data)
        {
            shader.SetMatrix4(name, data);
        }

        public void SetOrigin(string name, Vector2 origin)
        {
            shader.SetVector2(name, origin);
        }

        public virtual void Render()
        {
            if (!IsEnabled)
                return;

            GL.BindVertexArray(vertexArray);
            GL.UseProgram(programID);
            shader.SetVector4("curveColor", colorTable[color]);
            GL.LineWidth(3);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, vertexCount);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteVertexArray(vertexArray);
                GL.DeleteBuffer(bufferID);
            }
        }
    }

    public class Renderers : List<Renderer>
    {
        public Renderers() { }

        public new void Clear()
        {
            foreach (var obj in this)
            { obj.Dispose(); }

            base.Clear();
        }
    }
}