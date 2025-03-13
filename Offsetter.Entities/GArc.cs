using Offsetter.Graphics;
using Offsetter.Math;
using System.Collections.Generic;
using System.Diagnostics;

namespace Offsetter.Entities
{
    public class GArc : GCurve
    {
        public static GArc ArcCreate(GPoint start, GPoint end, GPoint center, int dir)
        {
            if (System.Math.Abs(dir) != 1)
                return null;

            double radA = GProperty.ArcLen(center, start);
            double radB = GProperty.ArcLen(center, end);
            if (System.Math.Abs(radA - radB) >= GConst.SMALL)
                return null;

            double arclen = GProperty.ArcLen(start, end, center, dir);
            if (arclen < GConst.SMALL)
                return null;

            GArc arc = new GArc(start, end, center, dir);
            arc.len = arclen;

            return arc;
        }

        public static GArc ArcCreate(GPoint start, GPoint end, double radians)
        {
            int dir = GProperty.Winding(radians);
            if (dir == 0)
                return null;

            GVec chord = end - start;
            if (chord.len < GConst.SMALL)
                return null;

            double lenHalfChord = chord.len / 2;
            GVec unitVec = chord.UnitVec();
            if (unitVec == null)
                return null;

            // distToCenter is the distance from chordMidPt to pc
            double distToCenter = lenHalfChord / System.Math.Tan(radians / 2);
            GPoint chordMidPt = start + (unitVec * lenHalfChord);

            unitVec += GConst.HALF_PI;
            GPoint pc = chordMidPt + (unitVec * distToCenter)!;

            double arclen = GProperty.ArcLen(start, end, pc, dir);
            if (arclen < GConst.SMALL)
                return null;

            GArc arc = new GArc(start, end, pc, dir);
            arc.len = arclen;

            return arc;
        }

        public static GArc ArcCreate(GPoint pc, double rad, double sa, double ea)
        {
            GPoint ps = pc + (new GVec(sa) * rad);
            GPoint pe = pc + (new GVec(ea) * rad);
            int dir = ((ea > sa) ? GConst.CCW : GConst.CW);

            return ArcCreate(ps, pe, pc, dir);
        }

        public static GArc CircleCreate(GPoint center, double radius, int dir)
        {
            if ((System.Math.Abs(dir) != 1) || (radius < GConst.SMALL))
                return null;

            GPoint pt = new GPoint(center.x + radius, center.y);
            GArc circle = new GArc(pt, pt, center, dir);
            circle.IsCircle = true;

            circle.sa = ((dir == GConst.CW) ? GConst.TWO_PI : 0);
            circle.len = 2 * GConst.PI * radius;

            circle.AnglesCalculate(dir);

            return circle;
        }

        private GArc(GPoint start, GPoint end, GPoint center, int winding)
        {
            ps.Assign(start);
            pe.Assign(end);
            pc.Assign(center);
            rad = GProperty.ArcLen(center, start) * winding;
            
            this.AnglesCalculate(winding);
        }

        public sealed override bool CanIgnore { get { return IsBlend; } }

        public sealed override bool IsValid { get { return (len >= GConst.SMALL); } }
        public sealed override bool IsBlend { get; set; } = false;
        public bool IsCircle { get; set; } = false;

        /// <summary>The arc center point.</summary>
        public GPoint pc { get; protected set; } = new GPoint(GPoint.UNDEFINED);

        /// <summary>The arc radius.</summary>
        public double rad { private set { radius = value; } get { return System.Math.Abs(radius); } }

        /// <summary>The arc winding direction [(-1) CW / (+1) CCW].</summary>
        public int dir { get { return System.Math.Sign(radius); } }

        /// <summary>The arc starting angle (radians).</summary>
        public double sa { private set; get; }

        /// <summary>The arc ending angle (radians).</summary>
        public double ea { private set; get; }

        /// <summary>The arc included angle (radians).</summary>
        public double IncludedAngle() { return System.Math.Abs(ea - sa); }

        public bool Merge(GArc rhs)
        {
            bool merged = false;

            if (rhs.pc.WithinTol(this.pc) && rhs.ps.WithinTol(this.pe))
            {
                this.pe = rhs.pe;
                this.IsCircle = this.ps.WithinTol(this.pe);

                this.AnglesCalculate(this.dir);
                merged = true;
            }

            return merged;
        }

        public sealed override void Reverse()
        {
            GPoint ptmp = ps;
            ps = pe;
            pe = ptmp;

            double dtmp = sa;
            sa = ea;
            ea = dtmp;

            radius = -radius;
        }

        public sealed override GArc Clone(GPoint at)
        {
            GArc copy;
            if (this.IsCircle)
                copy = CircleCreate(this.pc + at, this.rad, this.dir);
            else
                copy = ArcCreate(this.ps + at, this.pe + at, this.pc + at, this.dir);

            copy.IsBlend = this.IsBlend;
            return copy;
        }

        public sealed override GArc SplitAt(GPoint pt)
        {
            if ((pt == this.pe) || (pt == this.ps))
                return null;

            GArc pre = new GArc(this.ps, pt, this.pc, this.dir);
            GArc post = new GArc(pt, this.pe, this.pc, this.dir);

            this.pe = pt;
            this.len = pre.len;
            this.sa = pre.sa;
            this.ea = pre.ea;
            this.IsCircle = false;

            return post;
        }

        public sealed override GPoint PointAtUparam(double uparam)
        {
            double ua = this.sa + (uparam * (ea - sa));
            return (pc + (new GVec(ua) * rad));
        }

        // Based upon fab's -- double CGeoPoly::sub_area( const CGeoCurve& curve ) const
        public sealed override double area
        {
            get
            {
                double r = System.Math.Abs(radius);
                double rSqrd = r * r;

                double area;
                if (ps == pe)
                {
                    // ASSUMPTION: 'this' represents a circle.
                    area = dir * GConst.TWO_PI * rSqrd;
                }
                else
                {
                    // Triangular area, across arc's chord
                    area = (ps.x * pe.y) - (pe.x * ps.y);

                    // Adjust for curved portion...
                    double da = IncludedAngle();
                    double arcArea = rSqrd * (da - System.Math.Sin(da));

                    area += (dir * arcArea);
                }

                return area;
            }
        }

        public sealed override GBox box
        {
            get
            {
                GBox box = new GBox(ps, pe);

                double[] angles = { 0, GConst.HALF_PI, GConst.PI, 3 * GConst.HALF_PI };
                double[] dx = { 1, 0, -1, 0 };
                double[] dy = { 0, 1, 0, -1 };

                if (dir > 0)
                {
                    for (int indx = 0; indx < 4; ++indx)
                    {
                        double ia = Adjust(angles[indx]);
                        if ((ea > ia) && (ia > sa))
                            box += (pc + new GVec(rad * dx[indx], rad * dy[indx]));
                    }
                }
                else if (dir < 0)
                {
                    for (int indx = 3; indx >= 0; --indx)
                    {
                        double ia = Adjust(angles[indx]);
                        if ((sa > ia) && (ia > ea))
                            box += (pc + new GVec(rad * dx[indx], rad * dy[indx]));
                    }
                }

                return box;
            }
        }

        public sealed override GVec TangentAt(double uparam)
        {
            double radians = sa + (uparam * (this.ea - this.sa));
            radians += ((this.dir == GConst.CW) ? -GConst.HALF_PI : GConst.HALF_PI);
            return new GVec(radians);
        }

        // closest.dist -- (<0) inside / (0) on / (>0) outside
        public override sealed Closest Closest(GPoint pt, double pickTol = double.MaxValue)
        {
            double dx = pt.x - pc.x;
            double dy = pt.y - pc.y;

            GVec vec = new GVec(dx, dy);
            if (!vec.IsValid)
                return new NotClosest();

            double dist = vec.len;
            GPoint nearest = ((dist >= GConst.SMALL) ? pc + (vec.UnitVec()! * rad) : pc);

            double uparam = double.MaxValue;
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
            else
            {
                double pa = vec.Radians();
                Adjust(ref pa, ref uparam);
            }

            dist -= rad;
            if (System.Math.Abs(dist) > pickTol)
                return new NotClosest();

            return new Closest(this, nearest, dist, uparam);
        }

        /// <summary>Get the number of vertices to be generated using the given choral tolerance.</summary>
        public int TabulationCount(double chordalTol)
        {
            double sweep = System.Math.Abs(ea - sa);
            double halfda = System.Math.Acos((this.rad - chordalTol) / this.rad);
            return ((int)(sweep / halfda) + 2);
        }

        /// <summary>Generate the display list of vertices representing the graphical arc.</summary>
        public override void Tabulate(VertexList verts, double chordalTol)
        {
            int nChords = TabulationCount(chordalTol);

            double sweep = ea - sa;
            double halfda = sweep / nChords;

            // The number of generated vertices is nChords+1.
            for (int indx = 0; indx <= nChords; ++indx)
            {
                GVec vec = new GVec(sa + (indx * halfda));
                verts.Add(new Vertex(pc + (vec * rad)));
            }
        }

        public override sealed string CanonicalForm()
        {
            return string.Format("{0}: id{{{1}}} ps{{{2}}} pc{{{3}}} pe{{{4}}} rad{{{5}}} dir{{{6}}}",
                What(), this.id, ps.Format(), pc.Format(), pe.Format(),
                string.Format("{0:0.000000}", rad), dir);
        }

        private string What()
        {
            return (IsBlend
                ? "blend"
                : (IsCircle ? "circle" : "  arc"));
        }

        private void AnglesCalculate(int winding)
        {
            if (this.IsCircle)
            {
                if (winding > 0)
                {
                    if (sa < 0)
                        sa += GConst.TWO_PI;

                    ea = sa + GConst.TWO_PI;
                }
                else
                {
                    if (sa > GConst.TWO_PI)
                        sa -= GConst.TWO_PI;

                    ea = sa - GConst.TWO_PI;
                }
            }
            else
            {
                GVec uvec;

                uvec = GVec.UnitVec((ps.x - pc.x), (ps.y - pc.y), true);
                sa = uvec.Radians();

                uvec = GVec.UnitVec((pe.x - pc.x), (pe.y - pc.y), true);
                ea = uvec.Radians();

                if (winding > 0)
                {
                    if (ea < sa)
                        ea += GConst.TWO_PI;
                }
                else
                {
                    if (sa < ea)
                        sa += GConst.TWO_PI;
                }
            }
        }

        // Based on fab's -- CGeoArc::Adjust( double as, double ae, double* ap, double* uParam ) const
        private double Adjust(double ia)
        {
            // Adjust the angles to reflect any crossing of the 0/360 boundary.
            // NOTE: sa & ea are always within 2PI of each other.  We therefore
            // attempt to move ia into the same range.
            //
            // Note:  If we hit the TWOPI case, treat it sa over...
            // Otherwise, we can have a start of TWOPI to an end of 4.7 
            // (in dir -1) and an intermediate
            //	of 0.0... which gives a uparam of 4.0 instead of 0.0!!!
            // Found during poly boolean tests
            // 23 June 02 Edwin 
            if (dir < 0)
            {
                if ((sa >= (GConst.TWO_PI - GConst.SMALL)) && (ia < sa) && (ia < ea))  // ia < PI)
                    ia += GConst.TWO_PI;
            }
            else
            {
                if ((ea >= (GConst.TWO_PI - GConst.SMALL)) && (ia < ea) && (ia < sa))  // ia < PI)
                    ia += GConst.TWO_PI;
            }

            return ia;
        }

        // Based on fab's -- CGeoArc::Adjust( double as, double ae, double* ap, double* uParam ) const
        private void Adjust(ref double pa, ref double uparam)
        {
            // Adjust the angles to reflect any crossing of the 0/360 boundary.
            double ia = Adjust(pa);

            double adiff0, adiff1;

            if (dir < 0)
            {
                adiff1 = sa - ea;
                adiff0 = sa - ia;
            }
            else
            {
                adiff1 = ea - sa;
                adiff0 = ia - sa;
            }
            
            try { uparam = adiff0 / adiff1; }
            catch { uparam = GConst.UNDEFINED; }

            pa = ia;
        }

        private double radius;  // Sign indicates winding winding
    }
}
