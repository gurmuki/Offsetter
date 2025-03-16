using Offsetter.Graphics;
using Offsetter.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;  // This is required to expose ElementA()

namespace Offsetter.Entities
{
    using SubchnMap = Dictionary<GSubchn, GSubchn>;
    using GCurveList = List<GCurve>;

    public enum GRelativePosition { PRE, POST }
    public enum GChainType { UNKNOWN, POCKET, ISLAND, PART, TOOL, PATH }
    public enum GContainment { IGNORE, ON, INSIDE, OUTSIDE }

    // TODO: GChain isA GEntity & GCurve isA GEntity & GEntity isA GDNode so, indirectly,
    // GChain isA GDNode but there is no need for GChain to inherit from GDNode.
    public class GChain : GEntity
    {
        private GSubchn firstSubchn = null;
        private GSubchn terminalSubchn = null;

        private double signedArea = 0;
        private SubchnMap map;

        private GBox _box = new GBox();

        private bool hasArcs = false;

        public GChain(GChainType type)
        {
            this.layer = Layer.UNASSIGNED;
            this.ChainType = type;

            firstSubchn = new GSubchn(GPoint.UNDEFINED, this);
            terminalSubchn = new GSubchn(GPoint.UNDEFINED, this);
            firstSubchn.Append(terminalSubchn);

            // TODO: ASSUMPTION: chain is closed (it might not be so).
            map = new Dictionary<GSubchn, GSubchn>();
            map[firstSubchn] = terminalSubchn;
            map[terminalSubchn] = firstSubchn;

            IsDirty = true;

            hasArcs = false;
            origin = new GPoint(0, 0);
        }

        public Layer layer { get; set; }

        public GPoint origin { get; set; } = new GPoint(0, 0);

        public sealed override bool IsValid { get { return !IsEmpty; } }

        public bool IsEmpty { get { return (firstSubchn.Next == terminalSubchn); } }

        public bool IsClosed
        {
            get
            {
                if (firstSubchn == null)
                    return false;

                return (GProperty.ArcLen(ps, pe) < GConst.SMALL);
            }
        }

        // TODO: This shouldn't be public and using a private variable false (whatever the reason).
        public bool IsDirty { get; set; } = true;

        public GChainType ChainType { get; set; } = GChainType.UNKNOWN;

        public bool Append(GCurve? curve)
        {
            GLogger.Assert((curve != null), "GChain.Append(entity is null)");
            GLogger.Assert((curve.Prev == null), "GChain.Append(entity.Prev is not null)");
            GLogger.Assert((curve.Next == null), "GChain.Append(entity.Next is not null)");

            if (firstSubchn == null)
            {
                firstSubchn = new GSubchn(curve.ps, this);
                terminalSubchn = new GSubchn(curve.pe, this);

                // TODO: ASSUMPTION: chain is closed (it might not be so).
                map[firstSubchn] = terminalSubchn;
                map[terminalSubchn] = firstSubchn;

                firstSubchn.Append(terminalSubchn);
                ps.Assign(curve.ps);
            }
            else if (firstSubchn.NextCurve() == terminalSubchn)
            {
                firstSubchn.ipt = curve.ps;
                ps.Assign(curve.ps);
            }

            bool modified = false;
            if (curve.IsA(T.ARC))
            {
                GCurve prev = terminalSubchn.PrevCurve();
                if (prev.IsA(T.ARC))
                {
                    GArc arcA = (GArc)prev;
                    GArc arcB = (GArc)curve;
                    modified = arcA.Merge(arcB);
                }

                hasArcs = true;
            }

            if (!modified)
                terminalSubchn.Prepend(curve);

            terminalSubchn.ipt = curve.pe;

            pe.Assign(curve.pe);

            len += curve.len;
            IsDirty = true;

            return true;
        }


        public bool Prepend(GCurve? curve)
        {
            GLogger.Assert((curve != null), "GChain.Prepend(entity is null)");
            GLogger.Assert((curve.Prev == null), "GChain.Prepend(entity.Prev is not null)");
            GLogger.Assert((curve.Next == null), "GChain.Prepend(entity.Next is not null)");

            GLogger.Assert((firstSubchn == null), "GChain.Prepend(firstSubchn is not null)");
            if (firstSubchn == null)
            {
                firstSubchn = new GSubchn(curve.ps, this);
                terminalSubchn = new GSubchn(curve.pe, this);

                // TODO: ASSUMPTION: chain is closed (it might not be so).
                map[firstSubchn] = terminalSubchn;
                map[terminalSubchn] = firstSubchn;

                firstSubchn.Append(terminalSubchn);

                ps.Assign(curve.ps);
                pe.Assign(curve.pe);
            }

            firstSubchn.Append(curve);
            firstSubchn.ipt = curve.pe;

            pe.Assign(curve.pe);

            len += curve.len;
            IsDirty = true;

            if (curve.IsA(T.ARC))
                hasArcs = true;

            return true;
        }

        public void Reverse()
        {
            Stack<GCurve> stack = new Stack<GCurve>();

            GCurveIterator iter = new GCurveIterator(this, false);
            while (true)
            {
                GCurve curve = iter.Curve();
                if (curve == null)
                    break;

                iter.Next();

                curve.Unlink();
                stack.Push(curve);
            }

            if (stack.Count > 0)
            {
                while (stack.Count > 0)
                {
                    GCurve curve = stack.Pop();
                    if (curve.IsA(T.SUBCHN))
                        continue;

                    curve.Reverse();
                    Append(curve);
                }
            }
        }

        // Based upon fab's -- double CGeoPoly::sub_area( const CGeoCurve& entity ) const
        public sealed override double area
        {
            get
            {
                PropertiesUpdate();
                return signedArea;
            }
        }

        public sealed override GBox box
        {
            get
            {
                PropertiesUpdate();
                return new GBox(_box);
            }
        }

        public void PropertiesUpdate()
        {
            if (IsDirty)
            {
                signedArea = 0;
                _box.Invalidate();

                GCurve curve = firstSubchn;
                while (true)
                {
                    if (curve == null)
                        break;

                    if (!curve.IsA(T.SUBCHN))
                    {
                        signedArea += curve.area;
                        _box += curve.box;
                    }

                    curve = curve.NextCurve();
                }

                signedArea /= 2;

                IsDirty = false;
            }
        }

        public sealed override GVec TangentAt(double uparam)  // TODO: Implement fully
        {
            if (IsEmpty)
                return new GVec(0, 0);

            if (uparam == 0)
                return FirstCurve().TangentAt(0);

            if (uparam == 1)
                return TerminalCurve().TangentAt(1);

            return new GVec(0, 0);
        }

        public override sealed Closest Closest(GPoint pt, double pickTol = double.MaxValue)
        {
            Closest closest = new NotClosest();

            double minDist = GConst.UNDEFINED;
            GCurve curve = this.FirstCurve();
            while (true)
            {
                if (curve == null)
                    break;

                Closest candidate = curve.Closest(pt, pickTol);
                if ((candidate.Distance < closest.Distance) && GProperty.InRange(candidate.Uparam, 0, 1))
                    closest = candidate;

                curve = curve.NextCurve();
            }

            return closest;
        }

        public int Winding { get { return System.Math.Sign(this.area); } }

        public bool Grow(GCurve? curve)
        {
            if (curve == null)
                return false;

            if (!curve.IsValid)
                return false;

            bool success = true;
            if (this.IsEmpty)
            {
                this.Append(curve);
            }
            else if (this.pe == curve.ps)
            {
                this.Append(curve);
            }
            else if (this.pe == curve.pe)
            {
                curve.Reverse();
                this.Append(curve);
            }
            else if (this.ps == curve.pe)
            {
                this.Prepend(curve);
            }
            else if (this.ps == curve.ps)
            {
                this.Reverse();
                this.Prepend(curve);
            }
            else
            {
                success = false;
            }

            return success;
        }

        public GSubchn SplitAt(GCurve curve, GPoint pt)
        {
            GSubchn subchn = null;

            if (curve.ps == pt)
            {
                GCurve prev = curve.PrevCurve();
                if (prev.CanIgnore)
                {
                    if (prev.IsA(T.SUBCHN))
                        subchn = (GSubchn)prev;
                }
                else
                {
                    subchn = new GSubchn(pt, curve.Owner);
                    curve.Prepend(subchn);
                    IsDirty = true;
                }
            }
            else if (curve.pe == pt)
            {
                GCurve next = curve.NextCurve();
                if (next.CanIgnore)
                {
                    if (next.IsA(T.SUBCHN) && (next.NextCurve() == null))  // TODO: Need method GChain.IsTerminalSubchn(e.g. next) (?)
                    {
                        subchn = next.Owner.FirstSubchain();
                    }
                }
                else
                {
                    subchn = new GSubchn(pt, curve.Owner);
                    curve.Append(subchn);
                    IsDirty = true;
                }
            }
            else
            {
                GCurve sibling = curve.SplitAt(pt);

                subchn = new GSubchn(pt, curve.Owner);
                curve.Append(subchn);

                subchn.Append(sibling);
                IsDirty = true;
            }

            if (subchn != null)
                subchn.Owner = this;

            return subchn;
        }

        public GChain Clone(GPoint at)
        {
            GChain ochain = new GChain(this.ChainType);

            GCurve curve = firstSubchn;
            while (true)
            {
                if (curve == null)
                    break;

                GCurve copy = curve.Clone(at);
                if (copy != null)
                    ochain.Append(copy);

                curve = curve.NextCurve();
            }

            return ochain;
        }

        public void Reorder(int seedCurveId)
        {
            GLogger.Assert((map.Count == 2), "GChain.Reorder() -- can only modify virgin chains.");
            if (map.Count != 2)
                return;

            GCurve seedCurve = CurveFind(seedCurveId);
            if (seedCurve == null)
                return;

            if (seedCurve.id == this.FirstCurve().id)
                return;  // no need to reorder

            GCurve curve = this.FirstCurve();
            while (true)
            {
                if (curve.id == seedCurveId)
                    break;

                curve.Unlink();
                this.Append(curve);

                curve = this.FirstCurve();
            }

            this.ps.Assign(curve.ps);
        }

        private GCurve CurveFind(int id)
        {
            GCurve curve = this.FirstCurve();
            while (true)
            {
                if (curve == null)
                    break;

                if (curve.id == id)
                    break;

                curve = curve.NextCurve();
            }

            GLogger.Assert((curve != null), "GChain.CurveFind() -- cannot find seed seedCurve.");
            return curve;
        }

        public bool HasSubchains()
        {
            return (map[firstSubchn] != terminalSubchn);
        }

        // Reorder the curves of the island such that it
        // starts at the first intersection point. 
        public bool SubchainsReorder(GITable itable)
        {
            if (itable.Value(firstSubchn.ipt) != null)
                return false;  // The start of the chain represents an intersection point

            if (map[firstSubchn] == terminalSubchn)
                return false;  // The chain has no subchains.

            // a b c d e f g h i       e f g h b c d i
            // 0 - - - 0 - - - 0  == > 0 - - - - - - 0
            GSubchn a = firstSubchn;
            GSubchn e = map[firstSubchn];
            GSubchn i = terminalSubchn;

            GCurve b = a.NextCurve();
            GCurve d = e.PrevCurve();
            GCurve h = i.PrevCurve();

            h.Next = b;
            b.Prev = h;

            i.Prev = d;
            d.Next = i;

            i.ipt = d.pe;
            i.iindex = e.iindex;

            a.Next = null;
            e.Prev = null;
            firstSubchn = e;

            map.Remove(a);
            a = null;

            map[i] = e;

            return true;
        }

        public void SubchainsRecord(ref GITable itable)
        {
            map = new Dictionary<GSubchn, GSubchn>();

            GSubchn subchnA = null;
            GSubchn subchnB = null;

            GCurveIterator iter = new GCurveIterator(this, true);
            if (iter.Curve() != null)
            {
                GSubchn terminalSubchn = null;
                if (iter.Curve().IsA(T.SUBCHN))
                {
                    if (subchnA == null)
                        subchnA = (GSubchn)iter.Curve();

                    terminalSubchn = (GSubchn)iter.Curve();
                }

                while (true)
                {
                    GCurve curve = iter.Next();
                    if (curve == null)
                        break;

                    if (iter.Curve().IsA(T.SUBCHN))
                    {
                        GSubchn subchn = (GSubchn)iter.Curve();
                        subchnB = subchn;

                        if (subchn.NextCurve() != null)
                            itable.Record(subchn);

                        map[subchnA] = subchnB;
                        subchnA = subchnB;
                    }
                }
            }

            if ((this.firstSubchn != null) && (this.firstSubchn.ipt == subchnB.ipt))
                map[subchnB] = this.firstSubchn;
        }

        public void SubchainReparent(GChain copy, GSubchn subchn)
        {
            GCurve curr = subchn.NextCurve();
            while (true)
            {
                if ((curr == null) || curr.CanIgnore)
                    break;

                GCurve next = curr.NextCurve();

                curr.Unlink();
                this.Append(curr);

                copy.IsDirty = true;

                curr = next;
            }

            if (subchn == copy.FirstSubchain() && (curr != null) && curr.IsA(T.SUBCHN))
            {
                // We've reparented the first subchain of copy.
                copy.FirstSubchain().ipt = ((GSubchn)curr).ipt;
                copy.pe.Assign(((GSubchn)curr).ipt);
            }

            if (subchn.NextCurve() == copy.TerminalSubchain())
            {
                // We've reparented the terminal subchain of copy.
                copy.TerminalSubchain().ipt = subchn.ipt;
                copy.pe.Assign(subchn.ipt);
            }
        }

        public bool Unlink(ref GSubchn subchn)
        {
            Debug.Assert(subchn.Owner == this, "GChain.Unlink() - chain not owner of subchn.");
            if (subchn.Owner != this)
                return false;

            GCurve prev = subchn.PrevCurve();
            GCurve next = subchn.NextCurve();

            if (subchn.id == this.terminalSubchn.id)
            {
                if (prev == null)
                {
                    // Unlikely we'll ever arrive here.
                    subchn.Unlink();
                    subchn = null;

                    this.terminalSubchn = null;

                    map.Clear();
                    return true;
                }
                
                if (prev.id == this.firstSubchn.id)
                {
                    // Unlikely we'll ever arrive here.
                    subchn.Unlink();
                    subchn = null;
                    this.terminalSubchn = null;

                    prev.Unlink();
                    prev = null;
                    this.firstSubchn = null;

                    map.Clear();
                    return true;
                }

                Debug.Assert(prev.IsA(T.SUBCHN), "GChain.Unlink(#1) - invalid operation.");
                if (prev.IsA(T.SUBCHN))
                {
                    this.terminalSubchn = (GSubchn)prev;
                    subchn.Unlink();
                    subchn = null;
                    return true;
                }

                return false;
            }

            if (subchn.id == this.firstSubchn.id)
            {
                if (next == null)
                {
                    // Unlikely we'll ever arrive here.
                    subchn.Unlink();
                    subchn = null;
                    this.firstSubchn = null;
                    return true;
                }

                if (next.id == this.terminalSubchn.id)
                {
                    // Unlikely we'll ever arrive here.
                    subchn.Unlink();
                    subchn = null;
                    this.terminalSubchn = null;

                    next.Unlink();
                    next = null;
                    this.firstSubchn = null;
                    return true;
                }

                Debug.Assert(next.IsA(T.SUBCHN), "GChain.Unlink(#2) - invalid operation.");
                if (next.IsA(T.SUBCHN))
                {
                    map[terminalSubchn] = map[subchn];
                    map.Remove(subchn);

                    this.firstSubchn = (GSubchn)next;
                    subchn.Unlink();
                    subchn = null;
                    return true;
                }

                return false;
            }

            if (next == null)
                return false;

            Debug.Assert(next.IsA(T.SUBCHN), "GChain.Unlink(#3) - invalid operation.");
            if (next.IsA(T.SUBCHN))
            {
                GSubchn prevSubchn = this.PrevSubchain(subchn);

                map[prevSubchn] = map[subchn];
                map.Remove(subchn);

                subchn.Unlink();
                subchn = null;
            }

            return next.IsA(T.SUBCHN);
        }

        // NOTE: GChain.ContainmentAt() is really more general than determining the
        // containment relationship between a tool and another chain. The 'tool' is
        // named such only to simplify understanding in the context of non-uniform
        // offsetting and nesting. In the case of UniformOffsetting, 'this' might
        // represent a pocket and 'tool' an island (or visa versa)
        public bool ContainmentAt(GChain tool, GContainment desired)
        {
            Dictionary<GCurve, bool> ons = new Dictionary<GCurve, bool>();
            GContainment status;

            GCurve curr = tool.FirstCurve();
            while (true)
            {
                if (curr == tool.terminalSubchn)
                    break;

                if (!curr.IsA(T.SUBCHN))
                {
                    GPoint pt = curr.ps + tool.origin;

                    status = this.Contains(pt, desired, true);
                    if (status != desired)
                    {
                        if (status == GContainment.ON)
                            ons[curr] = true;
                        else
                            return false;
                    }

                    if (this.hasArcs && curr.IsA(T.ARC))
                    {
                        GArc shiftedArc = ((GArc)curr).Clone(tool.origin);
                        if (ArcsIntersect(shiftedArc))
                            return false;
                    }
                }

                curr = curr.NextCurve();
            }

            // TODO: Move this block just before this.hasArcs (?)
            if (ons.Count > 0)
            {
                foreach (var entry in ons)
                {
                    status = this.Scrutinize(entry.Key, desired);
                    if (status != desired)
                        return false;
                }
            }

            return true;
        }

        // ASSUMPTION: CCW windings.
        private GContainment Scrutinize(GCurve toolCurve, GContainment desired)
        {
            GContainment status;

            GChain tool = toolCurve.Owner;
            GPoint pc = toolCurve.ps + tool.origin;
            GArc circle = GArc.CircleCreate(pc, 1.5 * GConst.SMALL, GConst.CCW);

            GCurve curr = toolCurve.Clone(tool.origin);
            status = Check(curr, circle, desired);
            if (status != desired)
                return status;

            GCurve prev = ((toolCurve == tool.FirstCurve()) ? tool.TerminalCurve() : toolCurve.PrevCurve()).Clone(tool.origin);
            status = Check(prev, circle, desired);
            if (status != desired)
                return status;

            return desired;  // So as not to adversely affect the result.
        }

        private GContainment Check(GCurve toolCurve, GCurve circle, GContainment desired)
        {
            GIntersections intersections = GIntersect.CurveCurve(toolCurve, circle, true, true);
            foreach (var entry in intersections)
            {
                GContainment status = this.Contains(intersections[0].pt, desired, true);

                if ((desired == GContainment.OUTSIDE) && (status == GContainment.INSIDE))
                    return status;

                if ((desired == GContainment.INSIDE) && (status == GContainment.OUTSIDE))
                    return status;
            }

            return desired;  // So as not to adversely affect the result.
        }

        private bool ArcsIntersect(GArc shiftedArc)
        {
            Debug.Assert(hasArcs, "Invalid call to GChain.ArcsIntersect()");

            GContainment polyStatus = GContainment.IGNORE;

            GCurve curr = this.FirstCurve();
            while (true)
            {
                if (curr == terminalSubchn)
                    break;

                if (curr.IsA(T.ARC))
                {
                    GArc thisArc = ((GArc)curr).Clone(this.origin);
                    GIntersections results = GIntersect.ArcArc(thisArc, shiftedArc, true, 9.999999E-7);
                    if (results.Count == 2)
                        return true;

                    if (results.Count == 1)
                    {
                        if (!GProperty.InRange(results[0].uA, 0, 1) && !GProperty.InRange(results[0].uB, 0, 1))
                            return true;
                    }
                }

                curr = curr.NextCurve();
            }

            return false;
        }

        public bool OnPath(GPoint pt)
        {
            GCurve curr = TerminalCurve();
            int terminalId = ((curr == null) ? 0 : curr.id);

            while (true)
            {
                if ((curr == null) || curr.IsA(T.SUBCHN))
                    break;

                // TODO: Extend to check on arc (?)
                if (curr.IsA(T.LINE))
                {
                    if (OnSegment((GLine)curr, pt))
                        return (curr.id != terminalId);
                }

                curr = curr.PrevCurve();
            }

            return false;
        }

        // Based on: https://www.csharphelper.com/howtos/howto_polygon_geometry_point_inside.html
        public GContainment Contains(GPoint pt, GContainment desired, bool reportOnSegment)
        {
            double INSIDE_TOL = GConst.TWO_PI - GConst.VECTOR_SMALL;

            GCurve firstCurve = FirstCurve();
            Debug.Assert((firstCurve != null), "GChain.Contains(firstCurve is null)");

            GCurve terminalCurve = TerminalCurve();
            Debug.Assert((terminalCurve != null), "GChain.Contains(terminalCurve is null)");

            double total = 0;

            // Add the angles from the point to each other pair of vertices.
            GCurve curr = firstCurve;
            while (true)
            {
                if (curr == terminalSubchn)
                    break;

                if (!curr.IsA(T.SUBCHN))
                {
                    bool accept = true;
                    if (curr.IsA(T.LINE))
                    {
                        if (OnSegment((GLine)curr, pt))
                        {
                            if (reportOnSegment)
                                return GContainment.ON;

                            accept = false;
                        }
                    }

                    // NOTE: In the event curr is an arc, its end points
                    // represent the arc chord (e.g. the edge of a polygon).
                    if (accept)
                        total += Angle(curr.ps, pt, curr.pe);
                }

                curr = curr.NextCurve();
            }

            // (inside) |total angle| ~= 2PI / (outside) |total angle| ~= 0
            GContainment polyStatus = ((System.Math.Abs(total) > INSIDE_TOL) ? GContainment.INSIDE : GContainment.OUTSIDE);
            if (!hasArcs)
                return polyStatus;

            // ASSUMPTION: 'this' has a CCW winding. TODO: when 'this' has CW winding(?)
            GContainment arcStatus;
            curr = firstCurve;
            while (true)
            {
                if (curr == terminalSubchn)
                    break;

                if (curr.IsA(T.ARC))
                {
                    GArc arc = (GArc)curr;

                    arcStatus = ArcContains(arc, pt);
                    if (arcStatus == GContainment.ON)
                        return GContainment.ON;

                    if (arcStatus != GContainment.IGNORE)
                    {
                        // NOTE: GChain.Contains() was originally implemented for NonUniformOffsetter,
                        // in which case, all chains (are assumed) to have CCW winding. However, in the
                        // case of Uniform offsetting, islands will have a CW winding. Hence, we must
                        // consider winding direction, lest we get wrong answers.
                        if (this.Winding == GConst.CCW)
                        {
                            if (arcStatus == GContainment.INSIDE)
                                return ((arc.dir == GConst.CCW) ? GContainment.INSIDE : GContainment.OUTSIDE);
                        }
                        else
                        {
                            if (arcStatus == GContainment.INSIDE)
                                return ((arc.dir == GConst.CCW) ? GContainment.OUTSIDE : GContainment.INSIDE);
                        }
                    }
                }

                curr = curr.NextCurve();
            }

            return polyStatus;
        }

        // Prior to calling ArcContains(), we know if the given pt is inside/outside
        // of the polyline spanning the vertices of the part curves.
        // ASSUMPTIONS: The part has CCW winding and its arcs span (at most) one quadrant.
        //
        // A visual description of the cases is presented by
        // file:///C:/_sandbox/Offsetter/docs/ArcContains.png
        private GContainment ArcContains(GArc arc, GPoint pt)
        {
            GVec sVec = GVec.UnitVec(arc.pc, arc.ps, false);
            GVec eVec = GVec.UnitVec(arc.pc, arc.pe, false);
            GVec mVec = GVec.UnitVec(arc.pc, pt, false);

            double mCs = mVec ^ sVec;
            double mCe = mVec ^ eVec;

            if (arc.dir == GConst.CCW)
            {
                if ((mCs > 0) || (mCe < 0))
                    return GContainment.IGNORE;  // The point is outside the arc "wedge".
            }
            else
            {
                if ((mCs < 0) || (mCe > 0))
                    return GContainment.IGNORE;  // The point is outside the arc "wedge".
            }

            GVec chord = new GVec(arc.ps, arc.pe);
            GVec vec = new GVec(arc.ps, pt);

            double cross = vec ^ chord;
            if (System.Math.Abs(cross) < GConst.VECTOR_SMALL)
                return GContainment.ON;

            if ((arc.dir * cross) <= 0)  // TODO: May need to use signed distance to chord instead.
                return GContainment.IGNORE;  // The point is inside the triangle.

            vec = new GVec(arc.pc, pt);
            double diff = arc.rad - vec.len;
            if (System.Math.Abs(diff) < GConst.SMALL)
                return GContainment.ON;

            // Inside/Outside the region bounded by the arc and its chord.
            return ((System.Math.Sign(diff) < 0) ? GContainment.OUTSIDE : GContainment.INSIDE);
        }

        // Return a value between PI and -PI for the angle ABC.
        // Note that AB x BC = |AB| * |BC| * Cos(theta).
        private double Angle(GPoint ptA, GPoint ptB, GPoint ptC)
        {
            GPoint at = new GPoint(ptB.x, ptB.y);

            GVec vecA = new GVec(ptB, ptA);
            GVec vecB = new GVec(ptB, ptC);

            double dot = vecA * vecB;
            double cross = vecA ^ vecB;

            return System.Math.Atan2(cross, dot);
        }

        private bool OnSegment(GLine line, GPoint pt)
        {
            Closest closest = line.Closest(pt);
            return (GProperty.InRange(closest.Uparam, 0, 1) && (closest.Distance < GConst.SMALL));
        }

        public void Dump()
        {
            List<string> log = new List<string>();
            Log("chain", log);

            foreach (string str in log)
            { Debug.WriteLine(str); }
        }

        public string TextId()
        {
            switch (this.ChainType)
            {
                case GChainType.POCKET: return "POCKET";
                case GChainType.ISLAND: return "ISLAND";
                case GChainType.PART: return "PART";
                case GChainType.TOOL: return "TOOL";
                case GChainType.PATH: return "PATH";
                default: return "UNKNOWN";
            }
        }

        public void Log(string name, List<string>? log)
        {
            string header = string.Format("=-=-=-= {0} id{{{1}}} type{{{2}}} =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=", name, this.id, this.TextId());
            if (log != null)
                log.Add(header);
            else if (GLogger.Active)
                GLogger.Log(header);

            GCurve curve = firstSubchn;
            while (true)
            {
                if (curve == null)
                    break;

                if (log != null)
                    log.Add(curve.CanonicalForm());
                else if (GLogger.Active)
                    GLogger.Log(curve.CanonicalForm());

                curve = curve.NextCurve();
            }

            if (log != null)
                log.Add("");
            else if (GLogger.Active)
                GLogger.Log("");
        }

        public GSubchn FirstAssignedSubchain()
        {
            GSubchn start = map.ElementAt(0).Key;
            GSubchn curr = start;
            while (true)
            {
                if (curr == null)
                    break;

                if (curr.IsValid && curr.IsAssigned())
                    return curr;

                curr = map[curr];
                if (curr == start)
                    break;  // We've come full circle (ASSUMPTION: on a closed chain).
            }

            return null;
        }

        public GSubchn FirstMinSubchain(int minIindex)
        {
            GSubchn start = map.ElementAt(0).Key;
            GSubchn curr = start;
            while (true)
            {
                if (curr == null)
                    break;

                if (curr.IsValid && (curr.iindex == minIindex))
                {
                    // 'curr' is the head of a non-empty subchain
                    // or it is the terminal subchain.
                    return ((curr.NextCurve() == null) ? null : curr);
                }

                curr = map[curr];
                if (curr == start)
                    break;  // We've come full circle on a closed chain.
            }

            return null;
        }

        public GSubchn NextMinSubchain(GSubchn curr, GITable itable)
        {
            GSubchn next = itable.NextMinSubchain(curr);
            if (next == null)
                return null;

            if (!next.IsValid)
                return null;

            if (curr.iindex != next.iindex)
                return null;

            return next;
        }

        public GSubchn FirstSubchain()
        { return firstSubchn; }

        public GSubchn PrevSubchain(GSubchn subchn)
        {
            GSubchn prevSubchn = this.firstSubchn;
            while (true)
            {
                GSubchn tmp = map[prevSubchn];
                if (tmp == subchn)
                    break;

                prevSubchn = tmp;
            }

            if (prevSubchn == this.terminalSubchn)
                prevSubchn = PrevSubchain(prevSubchn);

            return prevSubchn;
        }

        public GSubchn NextSubchain(GSubchn curr)
        {
            if (!map.ContainsKey(curr))
                return null;

            GSubchn next = map[curr];
            return ((next == map.ElementAt(0).Key) ? null : next);
        }

        public GSubchn TerminalSubchain()
        { return terminalSubchn; }

        public GCurveIterator CurveIterator() { return new GCurveIterator(this, false); }

        // TODO: don't really want these to be public
        public GCurve FirstCurve()
        {
            GCurve firstCurve = firstSubchn.NextCurve();
            return ((firstCurve == terminalSubchn) ? null : firstCurve);
        }

        public GCurve TerminalCurve()
        {
            GLogger.Assert((terminalSubchn != null), "GChain.TerminalCurve() - malformed chain (must end with GSubchn).");
            GCurve terminalCurve = terminalSubchn.PrevCurve();
            return ((terminalCurve == firstSubchn) ? null : terminalCurve);
        }

        public GCurveList ToList()
        {
            GCurveList list = new GCurveList();

            GCurve curve = this.FirstCurve();
            while (curve != null)
            {
                if (!curve.IsA(T.SUBCHN))
                    list.Add(curve);

                curve = curve.NextCurve();
            }

            return list;
        }

        public override void Tabulate(VertexList verts, double chordalTol)
        {
            GCurve curve = this.FirstCurve();
            while (curve != null)
            {
                if (!curve.IsA(T.SUBCHN))
                    curve.Tabulate(verts, chordalTol);

                curve = curve.NextCurve();
            }
        }
    }

    public class GCurveIterator
    {
        private GChain chain;
        private bool includeBoundingSubchns;
        private GCurve curr;

        // TODO: Eliminate use of 'includeBoundingSubchns'.
        public GCurveIterator(GChain chain, bool includeBoundingSubchns)
        {
            this.chain = chain;
            this.includeBoundingSubchns = includeBoundingSubchns;

            curr = chain.FirstSubchain();
            Debug.Assert((curr != null), "GCurveIterator() - malformed chain (firstSubchn is null).");

            if (!includeBoundingSubchns)
                curr = ((curr == chain.TerminalSubchain()) ? null : curr.NextCurve());
        }

        public GCurve Curve() { return curr; }

        public GCurve Next()
        {
            if (curr == null)
                return null;

            GCurve next = curr.NextCurve();
            if (!includeBoundingSubchns && (next == chain.TerminalSubchain()))
                curr = null;
            else
                curr = next;

            return curr;
        }
    }
}
