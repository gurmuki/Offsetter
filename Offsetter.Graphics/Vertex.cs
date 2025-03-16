using Offsetter.Math;
using System.Collections.Generic;
using System;

namespace Offsetter
{
    public struct Vertex
    {
        public const int Size = sizeof(float) * 2;

        // public readonly float x;
        // public readonly float y;
        public float x;
        public float y;

        public Vertex(GPoint pt)
        {
            this.x = (float)pt.x;
            this.y = (float)pt.y;
        }

        public Vertex(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class VertexList : List<Vertex>
    {
        private GPoint tmp = new GPoint(0, 0);

        public VertexList() { }

        public void PointAdd(GPoint pt)
        {
            if (pt == null!)
                throw new ArgumentNullException("VertexList.PointAdd()");

            if (this.Count == 0)
            {
                this.Add(new Vertex(pt));
                return;
            }

            int indx = this.Count - 1;
            Vertex v = this[indx];

            if ((v.x == (float)pt.x) && (v.y == (float)pt.y))
                return;

            this.Add(new Vertex(pt));
        }
    }
}
