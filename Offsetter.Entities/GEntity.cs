using Offsetter.Graphics;
using Offsetter.Math;
using System;
using System.Collections.Generic;

namespace Offsetter.Entities
{
    public abstract class GEntity : GDNode
    {
        private static int eid = 0;

        public static void EntityIdsReset() { eid = 0; }

        public int id { get; } = (++GEntity.eid);

        public virtual bool IsValid { get; set; } = false;

        public bool IsA(Type type) { return (this.GetType() == type); }

        public virtual GPoint ps { get; set; } = new GPoint(GPoint.UNDEFINED);
        public virtual GPoint pe { get; set; } = new GPoint(GPoint.UNDEFINED);

        public double len { get; protected set; } = 0;

        public abstract double area { get; }

        public abstract GBox box { get; }

        public abstract GVec TangentAt(double uparam);
        public abstract Closest Closest(GPoint pt, double pickTol = double.MaxValue);

        public abstract void Tabulate(VertexList verts, double chordalTol);
    }

    public class T
    {
        public static Type CHAIN = typeof(GChain);
        public static Type SUBCHN = typeof(GSubchn);
        public static Type LINE = typeof(GLine);
        public static Type ARC = typeof(GArc);
    }
}