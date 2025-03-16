using System;
using System.Diagnostics;

namespace Offsetter.Math
{
    public class GPoint
    {
        public static readonly GPoint UNDEFINED = new GPoint(GConst.UNDEFINED, GConst.UNDEFINED);

        public GPoint(GPoint pt)
        {
            this.x = pt.x;
            this.y = pt.y;
        }

        public GPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double x { get; set; }
        public double y { get; set; }

        public bool IsValid()
        {
            return ((System.Math.Abs(x) < GConst.UNDEFINED) && (System.Math.Abs(y) < GConst.UNDEFINED));
        }

        public void Assign(GPoint pt)
        {
            this.x = pt.x;
            this.y = pt.y;
        }

        public static bool Equals(GPoint lhs, GPoint rhs)
        {
            if ((Object)lhs == (Object)rhs)
                return true;

            if ((Object)lhs == null || (Object)rhs == null)
                return false;

            return lhs.WithinTol(rhs);
        }

        // TODO: Use the == operator everywhere instead of WithinTol(?)
        public static bool operator ==(GPoint lhs, GPoint rhs)
        {
            return GPoint.Equals(lhs, rhs);
        }

        public static bool operator !=(GPoint lhs, GPoint rhs)
            => !(lhs == rhs);

        public static GPoint operator +(GPoint pt, GPoint vec)
            => new GPoint(pt.x + vec.x, pt.y + vec.y);

        public static GPoint operator +(GPoint pt, GVec vec)
            => new GPoint(pt.x + vec.x, pt.y + vec.y);

        public static GPoint operator -(GPoint pt, GVec vec)
            => new GPoint(pt.x - vec.x, pt.y - vec.y);

        public static GVec operator -(GPoint pe, GPoint ps)
            => new GVec(pe.x - ps.x, pe.y - ps.y);

        public override bool Equals(Object? obj)
        {
            if (this == null!) 
                throw new NullReferenceException();

            GPoint pt = (obj as GPoint)!;
            if (pt == null!)
                return false;

            if (Object.ReferenceEquals(this, obj))
                return true;

            return WithinTol(pt);
        }

        public bool Equals(GPoint pt)
        {
            if (this == null!)
                throw new NullReferenceException();

            if (pt == null!)
                return false;

            if (Object.ReferenceEquals(this, pt))
                return true;

            return WithinTol(pt);
        }

        public override int GetHashCode() => HashCode.Combine(x.GetHashCode(), y.GetHashCode());

        public bool WithinTol(GPoint rhs)
        {
            return WithinTol(rhs, GConst.SMALL);
        }

        private bool WithinTol(GPoint rhs, double tol)
        {
            double dx = System.Math.Abs(this.x - rhs.x);
            if (dx >= tol)
                return false;

            double dy = System.Math.Abs(this.y - rhs.y);
            if (dy >= tol)
                return false;

            // TODO: Compare squared value instead(?)
            double dist = System.Math.Sqrt(dx * dx + dy * dy);
            return (dist < tol);
        }

        public string Format()
        {
            string xstr = ((System.Math.Abs(x) == GConst.UNDEFINED) ? "UNDEFINED" : string.Format("{0:0.000000}", x));
            string ystr = ((System.Math.Abs(y) == GConst.UNDEFINED) ? "UNDEFINED" : string.Format("{0:0.000000}", y));
            return string.Format("{0},{1}", xstr, ystr);
        }

        public void Dump()
        {
            Debug.Write(Format());
        }
    }
}
