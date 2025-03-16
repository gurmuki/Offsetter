using Offsetter.Graphics;
using Offsetter.Math;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Offsetter.Entities
{
    public class GLine : GCurve
    {
        public static GLine LineCreate(GPoint start, GPoint end)
        {
            double arclen = GProperty.ArcLen(start, end);
            if (arclen < GConst.SMALL)
                return null;

            GLine line = new GLine(start, end);
            line.len = arclen;

            return line;
        }

        private GLine(GPoint start, GPoint end)
        {
            ps.Assign(start);
            pe.Assign(end);
        }

        public sealed override bool CanIgnore { get; } = false;
        public sealed override bool IsValid { get { return (len >= GConst.SMALL); } }

        public sealed override void Reverse()
        {
            GPoint tmp = ps;
            ps = pe;
            pe = tmp;
        }

        public sealed override GLine Clone(GPoint at)
        {
            return new GLine(this.ps + at, this.pe + at);
        }

        public sealed override GLine SplitAt(GPoint pt)
        {
            if ((pt == this.pe) || (pt == this.ps))
                return null;

            GLine post = new GLine(pt, this.pe);
            this.pe = pt;

            return post;
        }

        public sealed override GPoint PointAtUparam(double uparam)
        {
            return (ps + (new GVec(ps, pe) * uparam));
        }

        // Based upon fab's -- double CGeoPoly::sub_area( const CGeoCurve& curve ) const
        public sealed override double area
        {
            get { return ((ps.x * pe.y) - (pe.x * ps.y)); }
        }

        public sealed override GBox box
        {
            get { return new GBox(ps, pe); }
        }

        public sealed override GVec TangentAt(double uparam)
        {
            return GVec.UnitVec(ps, pe, true);
        }

        public override sealed Closest Closest(GPoint pt, double pickTol = double.MaxValue)
        {
            GVec vecA = pt - this.ps;
            GVec vecB = this.pe - this.ps;

            double magSqrd = vecB.len * vecB.len;
            if (magSqrd < GConst.SMALL_SQRD)
                return new NotClosest();  // We've encountered a zero-length line (shouldn't happen)

            double dot = vecA * vecB;
            double uparam = dot / magSqrd;

            GPoint nearest = this.ps + (vecB * uparam);

            if (ps == nearest)
            {
                nearest = ps;
                uparam = 0;
            }
            else if (pe == nearest)
            {
                nearest = pe;
                uparam = 1;
            }

            double dist = GProperty.ArcLen(nearest, pt);
            if (dist > pickTol)
                return new NotClosest();

            return new Closest(this, nearest, dist, uparam);
        }

        public override sealed string CanonicalForm()
        {
            return string.Format(" line: id{{{0}}} ps{{{1}}} pe{{{2}}}", this.id, ps.Format(), pe.Format());
        }

        public override void Tabulate(VertexList verts, double chordalTol)
        {
            verts.PointAdd(this.ps);
            verts.PointAdd(this.pe);
        }
    }
}
