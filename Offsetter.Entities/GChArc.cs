using Offsetter.Graphics;
using Offsetter.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Offsetter.Entities
{
    public class GChArc : GCurve
    {
        // ASSUMPTION: sa --> ea does not cross a quadrant boundary.
        public GChArc(GPoint pc, double radius, double sa, double ea, int dir)
        {
            if (dir == GConst.CCW)
            {
                Debug.Assert((sa <= ea), "GChArc(bad CCW angles");

                if ((ea > GConst.TWO_PI) && (sa >= GConst.TWO_PI))
                {
                    ea -= GConst.TWO_PI;
                    sa -= GConst.TWO_PI;
                }
            }
            else if (dir == GConst.CW)
            {
                Debug.Assert((sa >= ea), "GChArc(bad CW angles");

                if ((sa > GConst.TWO_PI) && (ea >= GConst.TWO_PI))
                {
                    sa -= GConst.TWO_PI;
                    ea -= GConst.TWO_PI;
                }
            }

            this.pc = pc;
            this.IsBlend = (radius < 0);
            this.rad = (this.IsBlend ? 0 : radius);
            this.sa = sa;
            this.ea = ea;
            this.dir = dir;
        }

        public sealed override bool CanIgnore { get; } = false;
        public sealed override bool IsValid { get { return false; } }

        public GPoint pc { get; private set; } = new GPoint(GPoint.UNDEFINED);
        public double rad { get; private set; } = 0;
        public double sa { get; private set; } = 0;
        public double ea { get; private set; } = 0;
        public int dir { get; private set; } = 0;
        public int ordinal { get; set; } = -1;

        public void Relocate(GPoint pc)
        {
            this.pc = pc;
        }

        public sealed override void Reverse()
        {
            double tmp = sa;
            sa = ea;
            ea = tmp;
            dir = -dir;
        }

        public sealed override GChArc Clone(GPoint at)
        {
            return new GChArc(this.pc + at, this.rad, this.sa, this.ea, this.dir);
        }

        public sealed override GChArc SplitAt(GPoint pt)
        {
            GVec vec = pt - this.pc;

            double diff = this.rad - vec.len;
            if (System.Math.Abs(diff) >= GConst.SMALL)
                return null;

            // TODO: May need to use dot and cross products instead.
            double radians = vec.Radians();
            if (this.dir == GConst.CCW)
            {
                if ((radians < this.sa) || (radians > this.ea))
                    return null;
            }
            else
            {
                if ((radians > this.sa) || (radians < this.ea))
                    return null;
            }

            GChArc post = new GChArc(this.pc, this.rad, radians, this.ea, this.dir);
            this.ea = radians;

            return post;
        }

        public void SplitAt(double radians)
        {
            GChArc post = new GChArc(this.pc, this.rad, radians, this.ea, this.dir);
            post.Next = this.Next;

            this.ea = radians;
            this.Next = post;
        }

        public sealed override GPoint PointAtUparam(double uparam)
        {
            Debug.Assert(false, "not implemented");
            return (ps + (new GVec(ps, pe) * uparam));
        }

        // Based upon fab's -- double CGeoPoly::sub_area( const CGeoCurve& curve ) const
        public sealed override double area
        {
            get
            {
                Debug.Assert(false, "not implemented");
                return ((ps.x * pe.y) - (pe.x * ps.y));
            }
        }

        public sealed override GBox box
        {
            get
            {
                Debug.Assert(false, "not implemented");
                return new GBox(ps, pe);
            }
        }

        // ASSUMPTION: The parent chain has CCW winding.
        public sealed override GVec TangentAt(double uparam)
        {
            double da = ea - sa;
            GVec radial = new GVec(sa + (uparam * da));
            return (radial + ((dir == GConst.CCW) ? GConst.HALF_PI : -GConst.HALF_PI));
        }

        public override sealed Closest Closest(GPoint pt, double pickTol)
        { throw new NotSupportedException(); }

        public void Dump()
        { Debug.WriteLine(PropertiesForm()); }

        public override sealed string PropertiesForm()
        {
            return string.Format("charc: ord:{{{0}}} id{{{1}}} pc{{{2}}} rad{{{3}}} sa{{{4}}} ea{{{5}}} dir{{{6}}}",
                this.ordinal, this.id, pc.Format(), string.Format("{0:0.000000}", rad), sa, ea, dir);
        }

        public override void Tabulate(VertexList verts, double chordalTol)
        { throw new NotSupportedException(); }

        public override void Digitize(VertexList verts, double delta)
        { throw new NotSupportedException(); }
    }
}
