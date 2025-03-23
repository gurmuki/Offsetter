using Offsetter.Entities;
using Offsetter.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Offsetter.Solver
{
    using GChainList = List<GChain>;

    public class GNonUniformOffsetter
    {
        private bool nesting;

        public GNonUniformOffsetter(bool nesting) { this.nesting = nesting; }

        /// <summary>
        /// When true, intermediate chains are returned in the results
        /// from Offset(). Said chains show how the part and tool are split.
        /// </summary>
        public bool ResultsAugment { get; set; } = false;

        // PREREQUISITE: The tool must be drawn about (0, 0).
        public void Offset(GChain part, GChain tool, int side, double stockAllowance, GChainList results)
        {
            GConfig.ConfigLoad();

            if (stockAllowance > 0)
            {
                // TODO: Decide whether to implement this.
                // Uniformly offset either the part or the tool (which one?)
                // Offsetting the part could result in multiple part chains.
                // Makes more sense to offset the tool.
            }

            // Oooops. We shouldn't be modifying the input chains because
            // doing so causes problems downstream (ie. to graphics).
            GPoint origin = new GPoint(0, 0);
            GChain splitPart = part.Clone(origin);
            GChain splitTool = tool.Clone(origin);

            ArcsSplit(splitPart);
            ArcsSplit(splitTool);

            DoIt(splitPart, splitTool, side, results);

#if NESTING_BOOLEAN
            if (nesting)
            {
                // This is a workaround for a limitation of LibreCAD (and likely any
                // DXF-based CAD software) which fails when a chain (polyline) contains
                // an arc whose span is very close to 2PI (e.g. a square having
                // and internal arc touching one edge at near-tangent condition).
                for (int indx = 2; indx < results.Count; ++indx)
                {
                    ArcsSplit(results[indx]);
                }
            }
#endif
        }

        private void SplitAnglesGet(GChChain chain, out List<double> radians)
        {
            radians = new List<double>();

            GChArc curr = chain.FirstCurve();
            while (true)
            {
                GVec radial = new GVec(curr.sa);
                double dval = System.Math.Abs(radial.x) + System.Math.Abs(radial.y);
                if (dval != 1)
                {
                    // The radial vector is not orthogonal to the coordinate system.
                    double ang = curr.sa;
                    if (curr.dir == GConst.CW)
                    {
                        ang += GConst.PI;
                        if (ang > GConst.TWO_PI)
                            ang -= GConst.TWO_PI;
                    }

                    // Conditionally add the vector.
                    double tmp = System.Math.Round(ang, 12);
                    if (radians.BinarySearch(tmp) < 0)
                    {
                        radians.Add(tmp);
                        radians.Sort();
                    }
                }

                radial = new GVec(curr.ea);
                dval = System.Math.Abs(radial.x) + System.Math.Abs(radial.y);
                if (dval != 1)
                {
                    // The radial vector is not orthogonal to the coordinate system.
                    double ang = curr.ea;
                    if (curr.dir == GConst.CW)
                    {
                        ang += GConst.PI;
                        if (ang > GConst.TWO_PI)
                            ang -= GConst.TWO_PI;
                    }

                    // Conditionally add the vector.
                    double tmp = System.Math.Round(ang, 12);
                    if (radians.BinarySearch(tmp) < 0)
                    {
                        radians.Add(tmp);
                        radians.Sort();
                    }
                }

                // TODO: Clean this up. Perhaps GChArc.NextCurve() should override GCurve.NextCurve()?
                GCurve next = curr.NextCurve();
                if (next.IsA(T.SUBCHN))
                    break;

                curr = (GChArc)next;
            }
        }

        private void OrdinalsAssign(GChChain chain)
        {
            int ord = 0;

            GCurve curr = chain.FirstCurve();
            while (true)
            {
                if (curr.IsA(T.SUBCHN))
                    break;

                ((GChArc)curr).ordinal = ord;
                ++ord;

                curr = curr.NextCurve();
            }
        }

        // TODO: Improve efficiency and robustness.
        private void SplitAt(GChChain chain, int side, List<double> radians)
        {
            GChArc curr = chain.FirstCurve();
            while (true)
            {
                GCurve next = curr.NextCurve();

                foreach (double angle in radians)
                {
                    double ang = angle;
                    if (side == GConst.RIGHT)
                    {
                        if (!chain.IsTool || (chain.IsTool && (curr.dir == GConst.CCW)))
                        {
                            ang += GConst.PI;
                            if ((ang >= GConst.TWO_PI) && (curr.sa < GConst.TWO_PI) && (curr.ea < GConst.TWO_PI))
                                ang -= GConst.TWO_PI;
                        }
                    }

                    if (InRange(curr.sa, ang, curr.ea))
                        curr.SplitAt(ang);
                }

                // TODO: Clean this up. Perhaps GChArc.NextCurve() should override GCurve.NextCurve()?
                if (next.IsA(T.SUBCHN))
                    break;

                curr = (GChArc)next;
            }
        }

        private bool InRange(double sa, double ang, double ea)
        {
            double sdiff = ang - sa;
            double ediff = ea - ang;

            // i.e.  sa < ang < ea
            if ((sdiff >= GConst.VECTOR_SMALL) && (ediff >= GConst.VECTOR_SMALL))
                return true;

            // i.e.  ea < ang < sa
            if ((ediff <= -GConst.VECTOR_SMALL) && (sdiff <= -GConst.VECTOR_SMALL))
                return true;

            return false;
        }

        private void DoIt(GChain part, GChain tool, int side, GChainList results)
        {
            GChChain chPart = new GChChain(part, GChainType.PART);
            GChChain chTool = new GChChain(tool, GChainType.TOOL);

            GChain path;
            if (chTool.IsConvex)
                ConvexToolOffset(chPart, chTool, side, out path);
            else
                NonConvexToolOffset(part, tool, chPart, chTool, side, out path);

            if (!path.IsEmpty)
            {
                GContainment desired = ((side == GConst.LEFT)
                    ? GContainment.INSIDE : GContainment.OUTSIDE);

                Degouge(path, part, tool, desired);

                path.ChainType = GChainType.PATH;
                path.layer = Layer.PATH;

                if (side == GConst.RIGHT)
                    path.Reverse();

                results.Add(path);

                if (nesting && !path.IsEmpty)
                {
                    GPoint bestPt = null!;
                    GBox bestBox = null!;
                    GBox partBox = chPart.box;

                    // ASSUMPTION: Everything worked as expected and so
                    // the result of degouging is a single closed chain.

                    GCurve crv = path.FirstSubchain();
                    while (true)
                    {
                        crv = crv.NextCurve();
                        if (crv.IsA(T.SUBCHN))
                            break;

                        GBox toolBox = chTool.BoxAt(crv.ps);
                        GBox candidate = partBox + toolBox;
                        if ((bestBox == null) || (candidate.area < bestBox.area))
                        {
                            bestBox = candidate;
                            bestPt = crv.ps;
                        }

                        GPoint mid = crv.PointAtUparam(0.5);
                        toolBox = chTool.BoxAt(mid);
                        candidate = partBox + toolBox;
                        if ((bestBox == null) || (candidate.area < bestBox.area))
                        {
                            bestBox = candidate;
                            bestPt = mid;
                        }
                    }

                    chTool.MoveTo(bestPt);
                }
            }

            if (GConfig.Values.Augment)
                results.Add(ChainCreate(chPart, GChainType.INTERMEDIATE));

            if (nesting || GConfig.Values.Augment)
                results.Add(ChainCreate(chTool, GChainType.INTERMEDIATE));

#if NESTING_BOOLEAN
            if (nesting)
            {
                GDegouger degouger = new GDegouger(GConst.CCW);
                degouger.AcceptTangencies = true;

                if (results[2].Winding == GConst.CCW)
                    results[2].Reverse();

                GChainList chains = new GChainList();
                chains.Add(results[1]);
                chains.Add(results[2]);
                GChainList rchains;
                degouger.ChainsDegouge(chains, out rchains);

                foreach (var ch in rchains)
                { results.Add(ch); }
            }
#endif
        }

        private void ConvexToolOffset(GChChain chPart, GChChain chTool, int side, out GChain path)
        {
            path = new GChain(GChainType.UNKNOWN);
            path.layer = Layer.PATH;

            //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
            // This block of code is difficult to explain but is required, allowing
            // the angle domains of part and tool arcs to overlap. In the trivial
            // case, where part and tool edges are all orthogonal to the coordinate
            // system, this code is not relevant because the chArc domains align
            // with quadrant boundaries. In the non-trivial case, splitting the
            // chArcs ensures domain overlap.
            //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

            List<double> partRadians, toolRadians;
            SplitAnglesGet(chPart, out partRadians);
            SplitAnglesGet(chTool, out toolRadians);
            SplitAt(chPart, side, toolRadians);
            SplitAt(chTool, side, partRadians);

            OrdinalsAssign(chPart);
            OrdinalsAssign(chTool);

            if (GConfig.Values.DumpChChains)
            {
                chPart.Dump();
                chTool.Dump();
            }

            //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

            GPoint pe = path.pe;

            SortedDictionary<string, bool> dict = new SortedDictionary<string, bool>();

            bool advanceOnPart = true;
            GCurve curr = (GCurve)chPart.FirstCurve();
            while (true)
            {
                if (curr.IsA(T.SUBCHN))
                    break;

                GChArc partArc = (GChArc)curr;

                GPoint ms = chTool.PositionAt(partArc, 0, side, advanceOnPart, false);  // move start
                if (ms == GPoint.UNDEFINED)
                {
                    curr = (GCurve)curr.NextCurve();
                    continue;
                }

                GPoint me = chTool.PositionAt(partArc, 1, side, advanceOnPart, false);  // move end
                if (me == GPoint.UNDEFINED)
                {
                    curr = (GCurve)curr.NextCurve();
                    continue;
                }

                string key = string.Format("PART {0}, TOOL {1}", partArc.id, chTool.curr.id);
                if (!dict.ContainsKey(key))
                {
                    dict[key] = true;

                    Debug.Assert(!ms.WithinTol(GPoint.UNDEFINED) && !me.WithinTol(GPoint.UNDEFINED));

                    if (ms.WithinTol(me))
                    {
                        // ASSUMPTION: We arrive here only when both the part and tool radii are zero.
                        if (path.IsEmpty)
                        {
                            if (!pe.WithinTol(GPoint.UNDEFINED))  // TODO: Is this necessary?
                                LineAppend(path, GLine.LineCreate(pe, ms));

                            pe = ms;
                        }
                        else if (!path.pe.WithinTol(me))
                        {
                            LineAppend(path, GLine.LineCreate(pe, me));
                            pe = me;
                        }
                    }
                    else
                    {
                        if (!path.IsEmpty && !path.pe.WithinTol(ms))
                            LineAppend(path, GLine.LineCreate(path.pe, ms));

                        bool isBlend = false;
                        if (partArc.rad < GConst.SMALL)
                        {
                            if ((side < 0) && (partArc.dir < 0))
                                isBlend = true;
                        }

                        ArcAppend(path, isBlend, GArc.ArcCreate(ms, me, partArc.ea - partArc.sa));
                        pe = me;
                    }
                }

                curr = (GCurve)curr.NextCurve();
            }

            if (!path.IsEmpty)
            {
                if (!path.IsClosed)
                    LineAppend(path, GLine.LineCreate(path.pe, path.ps));
            }
        }

        private void NonConvexToolOffset(GChain part, GChain tool, GChChain chPart, GChChain chTool, int side, out GChain path)
        {
            path = new GChain(GChainType.UNKNOWN);
            path.layer = Layer.PATH;

            //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
            // This block of code is difficult to explain but is required, allowing
            // the angle domains of part and tool arcs to overlap. In the trivial
            // case, where part and tool edges are all orthogonal to the coordinate
            // system, this code is not relevant because the chArc domains align
            // with quadrant boundaries. In the non-trivial case, splitting the
            // chArcs ensures domain overlap.
            //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

            List<double> partRadians, toolRadians;
            SplitAnglesGet(chPart, out partRadians);
            SplitAnglesGet(chTool, out toolRadians);
            SplitAt(chPart, side, toolRadians);
            SplitAt(chTool, side, partRadians);

            OrdinalsAssign(chPart);
            OrdinalsAssign(chTool);

            if (GConfig.Values.DumpChChains)
            {
                chPart.Dump();
                chTool.Dump();
            }

            //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

            GPoint ps = GPoint.UNDEFINED;

            SortedDictionary<string, bool> dict = new SortedDictionary<string, bool>();
            // SortedList<double, int> candidates = new SortedList<double, int>();
            List<GChArc> candidates = new List<GChArc>();
            List<GChArc> niCandidates = new List<GChArc>();  // non-interfering
            SortedDictionary<GApt, int> pts = new SortedDictionary<GApt, int>(new GPointComparer());

            GContainment desired = ((side == GConst.LEFT)
                ? GContainment.INSIDE : GContainment.OUTSIDE);

            chTool.ArcMapInit();

            bool okay = true;

            int prevToolOrdinal = -1;
            GCurve curr = (GCurve)chPart.FirstCurve();
            while (true)
            {
                if (curr.IsA(T.SUBCHN))
                    break;

                GChArc partArc = (GChArc)curr;
                chTool.ContactCandidatesGet(partArc, tool, side, out candidates);

                ContactCandidatesDump("INITIAL", partArc, candidates);

                Experiment(partArc, part, tool, path, chTool, side, candidates, out niCandidates);

                if (niCandidates.Count > 0)
                {
                    candidates.Clear();
                    foreach (GChArc arc in niCandidates)
                    {
                        candidates.Add(arc);
                    }

                    ContactCandidatesDump("NON-INTERFERING", partArc, candidates);
                }

                int cndx = -1;
                if ((prevToolOrdinal >= 0) && (partArc.dir == GConst.CW) && (partArc.rad > 0))
                {
                    for (cndx = 0; cndx < candidates.Count; ++cndx)
                    {
                        if (candidates[cndx].ordinal == prevToolOrdinal)
                            break;
                    }

                    Debug.WriteLine("prevToolOrdinal:{{{0}}} cndx:{{{1}}}", prevToolOrdinal, cndx);
                    Debug.Assert(cndx < candidates.Count);
                }

                // A special case. See Filter().
                if ((partArc.rad == 0) && (ps != GPoint.UNDEFINED))
                    Filter(partArc, chTool, side, ps, candidates);

                bool firstPoint = true;

                int count = candidates.Count;
                int indx = -1;
                while (true)
                {
                    indx = IterIndex(indx, cndx, count);
                    if (indx < 0)
                        break;

                    chTool.CurrSet(candidates[indx].id);

                    GPoint ms = chTool.PositionAt(partArc, 0, side, chTool.curr.dir == GConst.CCW, true);  // move start
                    if (ms == GPoint.UNDEFINED)
                        continue;

                    GPoint me = chTool.PositionAt(partArc, 1, side, chTool.curr.dir == GConst.CCW, true);  // move end
                    if (me == GPoint.UNDEFINED)
                        continue;

                    if (GConfig.Values.DumpPositionAtClient)
                    {
                        Debug.WriteLine("part:{0} tool:{1} ms:{2} me:{3}",
                            partArc.ordinal, chTool.curr.ordinal, ms.Format(), me.Format());
                    }

                    GApt pt;
                    if (ms.WithinTol(me))
                    {
                        // ASSUMPTION: We arrive here only when both the part and tool radii are zero.
                        pt = new GApt(ms, partArc.ordinal, chTool.curr.ordinal);

                        okay = (partArc.NextCurve().IsA(T.SUBCHN));
                        if (!okay)
                        {
                            okay = !pts.ContainsKey(pt);
                            if (okay)
                                pts.Add(pt, pts.Count + 1);
                        }

                        if (okay)
                        {
                            okay = !path.OnPath(pt);
                            if (okay)
                            {
                                if (path.IsEmpty)
                                {
                                    if (ps == GPoint.UNDEFINED)
                                        ps = ms;
                                }

                                if (!ps.WithinTol(ms))
                                {
                                    if (firstPoint)
                                    {
                                        LineAppend(path, GLine.LineCreate(ps, ms));
                                        ps = ms;

                                        prevToolOrdinal = candidates[indx].ordinal;
                                        firstPoint = false;
                                    }
                                    else
                                    {
                                        // Move the tool to the current location on the path.
                                        tool.origin = ms;
                                        okay = ContainmentAt(part, tool, desired);
                                        if (okay)
                                        {
                                            LineAppend(path, GLine.LineCreate(ps, ms));
                                            ps = ms;

                                            prevToolOrdinal = candidates[indx].ordinal;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        pt = new GApt(me, partArc.ordinal, chTool.curr.ordinal);

                        okay = (partArc.NextCurve().IsA(T.SUBCHN));
                        if (!okay)
                        {
                            okay = !pts.ContainsKey(pt);
                            if (okay)
                                pts.Add(pt, pts.Count + 1);
                        }

                        if (okay)
                        {
                            if (path.IsEmpty)
                            {
                                if (ps == GPoint.UNDEFINED)
                                    ps = ms;
                            }

                            if (!ps.WithinTol(ms))
                            {
                                LineAppend(path, GLine.LineCreate(ps, ms));
                                ps = ms;
                            }

                            bool isBlend = false;  // TODO
                            ArcAppend(path, isBlend, GArc.ArcCreate(ms, me, partArc.ea - partArc.sa));
                            ps = me;

                            prevToolOrdinal = candidates[indx].ordinal;

                            if ((desired == GContainment.OUTSIDE) && (partArc.dir == GConst.CW))
                                break;
                        }
                    }
                }

                curr = (GCurve)curr.NextCurve();
            }

            RptsDump(pts);

            if (!path.IsEmpty && !path.IsClosed)
                LineAppend(path, GLine.LineCreate(path.pe, path.ps));
        }

        // Lacking a formal vocabulary to describe this issue, I can only say
        // the purpose of Filter() is to prevent a "spike" from appearing at
        // the start of the resulting path. In essence, the tool moves away
        // from (and then immediately returns to) the same part location.
        //
        // The following .png files illustrate the difference in behavior
        // with/without using Filter().
        //
        //    file:///C:/_sandbox/Offsetter/docs/nest02c-sans-filter.png
        //    file:///C:/_sandbox/Offsetter/docs/nest02c-with-filter.png
        //
        private void Filter(GChArc partArc, GChChain chTool, int side, GPoint ps, List<GChArc> candidates)
        {
            if (partArc.rad > 0)
                return;  // ASSUMPTION: This issue occurs only at sharp corners.

            int count = candidates.Count;
            if (count < 3)
                return;  // At least three candidates are required to create a spike.

            List<GPoint> pts = new List<GPoint>();
            pts.Add(ps);  // The part "corner" point at which the issue might manifest.

            int indx = -1;
            while (true)
            {
                indx = IterIndex(indx, -1, count);
                if (indx < 0)
                    break;

                chTool.CurrSet(candidates[indx].id);

                GPoint ms = chTool.PositionAt(partArc, 0, side, chTool.curr.dir == GConst.CCW, true);  // move start
                if (ms == GPoint.UNDEFINED)
                    continue;

                GPoint me = chTool.PositionAt(partArc, 1, side, chTool.curr.dir == GConst.CCW, true);  // move end
                if (me == GPoint.UNDEFINED)
                    continue;

                if (ms != pts[pts.Count-1])
                    pts.Add(ms);

                if (me != pts[pts.Count - 1])
                    pts.Add(me);
            }

            if (pts.Count > 2)
            {
                if (pts[2] == pts[0])
                    candidates.RemoveAt(0);  // Remove the candidate causing the spike.
            }
        }

        private int IterIndex(int indx, int cndx, int count)
        {
            if (indx < 0)
            {
                return ((cndx < 0) ? 0 : cndx);
            }
            else
            {
                if (cndx < 0)
                {
                    ++indx;
                    if (indx >= count)
                        return -1;

                    return indx;
                }
                else
                {
                    if (indx < cndx)
                        return -1;

                    return (cndx - 1);
                }
            }
        }

        private void Experiment(GChArc partArc, GChain part, GChain tool, GChain path, GChChain chTool, int side, List<GChArc> candidates, out List<GChArc> niCandidates)
        {
            SortedDictionary<GApt, int> pts = new SortedDictionary<GApt, int>(new GPointComparer());
            List<int> tords = new List<int>();

            GContainment desired = ((side == GConst.LEFT)
                ? GContainment.INSIDE : GContainment.OUTSIDE);

            niCandidates = new List<GChArc>();
            if (partArc.rad > 0)
                return;

            List<GChArc> ilist = new List<GChArc>();
            List<GChArc> nlist = new List<GChArc>();

            GCurve terminal = path.TerminalCurve();
            GVec tanA = partArc.TangentAt(0);

            bool okay;
            int count = candidates.Count;
            for (int indx = 0; indx < count; ++indx)
            {
                chTool.CurrSet(candidates[indx].id);

                GPoint ms = chTool.PositionAt(partArc, 0, side, chTool.curr.dir == GConst.CCW, true);  // move start
                if (ms == GPoint.UNDEFINED)
                    continue;

                GPoint me = chTool.PositionAt(partArc, 1, side, chTool.curr.dir == GConst.CCW, true);  // move end
                if (me == GPoint.UNDEFINED)
                    continue;

                if (GConfig.Values.DumpPositionAtClient)
                {
                    Debug.WriteLine("part:{0} tool:{1} ms:{2} me:{3}",
                        partArc.ordinal, chTool.curr.ordinal, ms.Format(), me.Format());
                }

                GApt pt;
                okay = ms.WithinTol(me);
                Debug.Assert(okay, "Experiment()");
                if (okay)
                {
                    // ASSUMPTION: We arrive here only when both the part and tool radii are zero.
                    pt = new GApt(ms, partArc.ordinal, chTool.curr.ordinal);
                    if (!pts.ContainsKey(pt))
                    {
                        pts.Add(pt, pts.Count + 1);  // TODO: Move this statement inside next conditional block(?)

                        tool.origin = ms;
                        okay = ContainmentAt(part, tool, desired);
                        if (okay)
                        {
                            nlist.Add(chTool.curr);
                        }
                        else
                        {
                            if (terminal == null)
                            {
                                ilist.Add(chTool.curr);
                            }
                            else
                            {
                                GVec tanB = GVec.UnitVec(terminal.pe, ms);
                                tanB.Normalize();

                                double dot = tanA * tanB;
                                if (System.Math.Abs(dot) >= (1 - GConst.VECTOR_SMALL))
                                    ilist.Add(chTool.curr);
                            }
                        }
                    }
                }
            }

            if (nlist.Count > 0)
            {
                foreach(var n in nlist)
                {
                    niCandidates.Add(n);
                }
            }
            else if (ilist.Count > 0)
            {
                niCandidates.Add(ilist.Last());
            }
        }

        private void ContactCandidatesDump(string label, GChArc partArc, List<GChArc> candidates)
        {
            if (GConfig.Values.DumpContactCandidatesClient)
            {
                Debug.WriteLine("{0} candidates for part: {{{1}}}", label, partArc.PropertiesForm());
                for (int i = 0; i < candidates.Count; i++)
                {
                    Debug.WriteLine(candidates[i].PropertiesForm());
                }
                Debug.WriteLine("");
            }
        }

        private void RptsDump(SortedDictionary<GApt, int> pts)
        {
            if (GConfig.Values.DumpRpts)
            {
                SortedDictionary<int, GApt> rpts = new SortedDictionary<int, GApt>();

                foreach (var entry in pts)
                {
                    rpts.Add(entry.Value, entry.Key);
                }

                Debug.WriteLine("=-=-=-= Rpts =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
                foreach (var entry in rpts)
                {
                    Debug.WriteLine("{0}) part:{1}, tool:{2} {{{3}, {4}}}", entry.Key, entry.Value.pid, entry.Value.tid, entry.Value.x, entry.Value.y);
                }
                Debug.WriteLine("");
            }
        }

        private void Degouge(GChain path, GChain part, GChain tool, GContainment desired)
        {
            ZigZagsRemove(path, part, tool, desired);

            // Create intersection points along the path (e.g. at T-like intersections).
            Split(path);

            if (GConfig.Values.DumpPath)
            { path.Dump(); }

            GCurve curr = path.FirstCurve();
            while (true)
            {
                if (curr.IsA(T.SUBCHN))
                    break;

                GCurve next = curr.NextCurve();

                // Move the tool to the current location on the path.
                tool.origin = curr.ps;

                bool okay = part.ContainmentAt(tool, desired);  // TODO: Rename ContainmentAt() (?)
                if (!okay)
                {
                    GCurve prev = curr.PrevCurve();

                    if (prev.IsA(T.SUBCHN))
                        prev = path.TerminalCurve();

                    if (prev.IsA(T.LINE) && curr.IsA(T.LINE))
                    {
                        GContainment status = path.Contains(tool.origin, desired, false);
                        if (status != desired)
                        {
                            if (prev.pe.WithinTol(curr.ps))
                                prev.Unlink();
                        }
                        else
                        {
                            // A special case.
                            //
                            // Without this code block:
                            //    file:///C:/_sandbox/Offsetter/docs/nest02c-interference.png
                            //
                            // With this code block:
                            //    file:///C:/_sandbox/Offsetter/docs/nest02c-no-interference.png
                            //
                            GVec vec = prev.TangentAt(0) * prev.len;
                            GPoint pt = curr.ps - vec;
                            prev.pe = curr.pe - vec;
                            next = curr.NextCurve();
                            next.ps = prev.pe;
                        }
                    }
                    else
                    {
                        if (prev.pe.WithinTol(curr.ps))
                            prev.Unlink();
                    }

                    curr.Unlink();

                    curr = next.PrevCurve();
                    if (!curr.IsA(T.SUBCHN) && !next.IsA(T.SUBCHN) && curr.ps.WithinTol(next.ps))
                    {
                        // We've encountered a special case where the path crosses itself.
                        // The #ref(s) correlate with file:///C:/_sandbox/Offsetter/docs/nest05a.png
                        prev = curr.PrevCurve();  // #ref:1 (prev)
                        prev.pe = curr.pe;        // #ref:2 (curr)
                        next.ps = prev.pe;        // #ref:5 (next)
                        curr = prev;
                        next = curr.NextCurve();
                        continue;
                    }
                }
                else if (curr == path.TerminalCurve())
                {
                    // Move the tool to the current location on the path.
                    tool.origin = curr.pe;

                    // TODO: Unlink() should cause the chain to be dirty and,
                    // in turn, cause chain end point update so IsClosed
                    // returns the correct answer.
                    okay = part.ContainmentAt(tool, desired);
                    if (!okay)
                    {
                        curr.Unlink();
                        break;
                    }
                }

                curr = next;
            }

            if (!path.IsClosed)
            {
                // Move the tool to the current location on the path.
                tool.origin = curr.ps;

                curr = path.TerminalCurve();
                bool okay = part.ContainmentAt(tool, desired);
                if (!okay)
                    curr.Unlink();
            }
        }

        private void ZigZagsRemove(GChain path, GChain part, GChain tool, GContainment desired)
        {
            GCurve curveA, curveB, curveC;
            GVec tanA, tanB, tanC;
            double dot;

            GCurve iter = path.FirstCurve();
            while (true)
            {
                if (!ThreeLines(iter, out curveA, out curveB, out curveC))
                    break;

                tanA = curveA.TangentAt(1);
                tanB = curveB.TangentAt(0);

                dot = tanA * tanB;
                if (dot < 0)
                {
                    // Found a zig.
                    tanB = curveB.TangentAt(1);
                    tanC = curveC.TangentAt(0);

                    dot = tanB * tanC;
                    if (dot < 0)
                    {
                        // Found a zag.

                        // Move the tool to the current location on the path.
                        tool.origin = curveA.pe;
                        bool okA = part.ContainmentAt(tool, desired);

                        tool.origin = curveB.pe;
                        bool okB = part.ContainmentAt(tool, desired);

                        if (okA)
                        {
                            Debug.Assert(!okB, "ZigZagsRemove(okA==true, okB==false)");
                            if (!okB)
                                curveC.ps.Assign(curveA.pe);  // e.g. nest10a.dxf
                        }
                        else if (okB)
                        {
                            Debug.Assert(!okA, "ZigZagsRemove(okA==false, okB==true)");
                            if (!okA)
                                curveA.pe.Assign(curveC.ps);
                        }
                        else
                        {
                            // e.g. nest16d.dxf
                            GIntersections intersections = GIntersect.CurveCurve(curveA, curveC, false, true);
                            Debug.Assert((intersections.Count > 0), "ZigZagsRemove(no intersections)");
                            if (intersections.Count == 0)
                            {
                                // The zigzag is a spike (a non-intersecting 'Z' as opposed to being triangular).
                                intersections = GIntersect.CurveCurve(curveA, curveC, false, false);
                                Debug.Assert((intersections.Count > 0), "ZigZagsRemove(spike)");
                            }
                            else
                            {
                                Debug.Assert((intersections.Count == 1), "ZigZagsRemove(multiple intersections)");
                                curveA.pe.Assign(intersections[0].pt);
                                curveC.ps.Assign(intersections[0].pt);
                            }
                        }

                        curveB.Unlink();
                        curveB = null!;
                    }
                }

                iter = ((curveB == null) ? curveC : curveB);
            }
        }

        private bool ThreeLines(GCurve seed, out GCurve curveA, out GCurve curveB, out GCurve curveC)
        {
            List<GCurve> curves = new List<GCurve>();

            curveA = null!;
            curveB = null!;
            curveC = null!;

            GCurve iter = seed;
            while (true)
            {
                if (iter.IsA(T.SUBCHN))
                    return false;

                if (iter.IsA(T.LINE))
                    curves.Add(iter);
                else
                    curves.Clear();

                if (curves.Count == 3)
                    break;

                iter = iter.NextCurve();
            }

            if (curves.Count == 3)
            {
                curveA = curves[0];
                curveB = curves[1];
                curveC = curves[2];
                return true;
            }

            return false;
        }

        private void Cleanup(GChain path)
        {
            GCurve curveA = null!;
            GCurve curveB = null!;
            GCurve curveC = null!;
            GCurve curveD = null!;
            GCurve iter = null!;

            /*
             * 1. Find a gap at curveA
             * 2. Forward search for curveB whose start point is coincident with the end point of curveA
             * 3. Search backwards for curveC whose start point is coincident with the end point of curveB
             * 4. Starting at curveB, search forward for curveD whose start point is coincident with the end point of curveC
             * 5. Unlink all unused curves
             */
            GCurve curr = path.FirstCurve();
            while (true)
            {
                GCurve next = curr.NextCurve();
                if (next.IsA(T.SUBCHN))
                    break;

                if (curr.pe.WithinTol(next.ps))
                {
                    curr = next;
                    continue;
                }

                // 1. We've encountered a gap. ASSUMPTION: The gap was created by Degouge() deleting curves.
                curveA = curr;

                // 2. Forward search for curveB whose start point is coincident with the end point of curveA.
                iter = next;
                while (true)
                {
                    if (iter.IsA(T.SUBCHN))
                        break;  // We've reached the end of the chain.

                    if (iter.ps.WithinTol(curveA.pe))
                        break;  // We've found curveB.

                    iter = iter.NextCurve();
                }

                if (!iter.IsA(T.SUBCHN))
                {
                    curveB = iter;

                    // 3. Search backwards for curveC whose start point is coincident with the end point of curveB.
                    iter = iter.PrevCurve();
                    while (true)
                    {
                        if (iter == curveA)
                            break;  // We've failed to find curveC.

                        if (iter.ps.WithinTol(curveB.pe))
                            break;  // We've found curveC.

                        iter = iter.PrevCurve();
                    }

                    if (iter != curveA)
                    {
                        curveC = iter;

                        // 4. Starting at curveB, search forward for curveD whose start point is coincident with the end point of curveC
                        iter = curveB;
                        while (true)
                        {
                            if (iter.IsA(T.SUBCHN))
                                break;  // We've reached the end of the chain.

                            if (iter.ps.WithinTol(curveC.pe))
                                break;  // We've found curveD.

                            iter = iter.NextCurve();
                        }
                    }

                    if (!iter.IsA(T.SUBCHN))
                    {
                        curveD = iter;

                        // 5. Unlink all unused curves.
                        iter = curveA;
                        while (true)
                        {
                            iter = iter.NextCurve();
                            if (iter == curveD)
                                break;

                            if ((iter == curveB) || (iter == curveC))
                                continue;

                            GCurve unused = iter;
                            iter = iter.PrevCurve();
                            unused.Unlink();
                        }

                        // 6. Change the curve order to ABCD.
                        curveC.Unlink();
                        curveB.Append(curveC);
                    }
                }

                curr = next;
            }
        }

        // NOTE: GChain.ContainmentAt() is really more general than determining the
        // containment relationship between a tool and another chain. The 'tool' is
        // named such only to simplify understanding in the context of NonUniformOffsetter
        // offsetting and nesting. In the case of UniformOffsetter, 'this' might
        // represent a pocket and 'tool' an island (or visa versa)
        public bool ContainmentAt(GChain part, GChain tool, GContainment desired)
        {
            GChain tclone = tool.Clone(tool.origin);
            if (Intersect(part, tclone, desired))
                return false;

            // Does the tool origin have the correct containment relationship to the part?
            GContainment status = part.Contains(tool.origin, desired, true);
            return ((status == GContainment.ON) || (status == desired));
        }

        private bool Intersect(GChain part, GChain tool, GContainment desired)
        {
            GLogger.Assert((part != tool), "GNonUniformOffseter.Intersect(part, tool, desired)");
            if (part == tool)
                return false;

            bool contiguous = false;
            GCurve pcurve = part.FirstSubchain();
            while (pcurve != null)
            {
                if (pcurve == null)
                    break;

                if (pcurve.CanIgnore)
                {
                    if (pcurve == part.TerminalSubchain())
                        break;

                    pcurve = pcurve.NextCurve();
                    continue;
                }

                GCurve tcurve = tool.TerminalCurve();
                while (tcurve != null)
                {
                    if (tcurve.CanIgnore)
                    {
                        if (tcurve == tool.FirstSubchain())
                            break;

                        tcurve = tcurve.PrevCurve();  // we've encounter a blend or subchn
                        continue;
                    }

                    GIntersections results = GIntersect.CurveCurve(pcurve, tcurve, contiguous, true);
                    Debug.Assert(results.Count < 2);  // TODO: What to do if 2?
                    if (results.Count > 0)
                    {
                        if (!NearEndPoint(results[0].pt, pcurve, tcurve))
                            return true;  // We found an "obvious" intersection.

                        GContainment status = Scrutinize(part, tcurve, desired);
                        if (status != desired)
                            return true;
                    }

                    tcurve = tcurve.PrevCurve();
                }

                pcurve = pcurve.NextCurve();
            }

            return false;
        }

        private bool NearEndPoint(GPoint pt, GCurve pcurve, GCurve tcurve)
        {
            bool near = (pt.WithinTol(pcurve.ps) || pt.WithinTol(pcurve.pe) || pt.WithinTol(tcurve.ps) || pt.WithinTol(tcurve.pe));
            return near;
        }

        // ASSUMPTION: CCW windings.
        private GContainment Scrutinize(GChain part, GCurve toolCurve, GContainment desired)
        {
            GContainment status;

            GChain tool = toolCurve.Owner;
            GPoint pc = toolCurve.ps + tool.origin;
            GArc circle = GArc.CircleCreate(pc, 1.5 * GConst.SMALL, GConst.CCW);

            GCurve curr = toolCurve.Clone(tool.origin);
            status = Check(part, curr, circle, desired);
            if (status != desired)
                return status;

            GCurve prev = ((toolCurve == tool.FirstCurve()) ? tool.TerminalCurve() : toolCurve.PrevCurve()).Clone(tool.origin);
            status = Check(part, prev, circle, desired);
            if (status != desired)
                return status;

            return desired;  // So as not to adversely affect the result.
        }

        private GContainment Check(GChain part, GCurve toolCurve, GCurve circle, GContainment desired)
        {
            GIntersections intersections = GIntersect.CurveCurve(toolCurve, circle, true, true);
            foreach (var entry in intersections)
            {
                GContainment status = part.Contains(intersections[0].pt, desired, true);

                if ((desired == GContainment.OUTSIDE) && (status == GContainment.INSIDE))
                    return status;

                if ((desired == GContainment.INSIDE) && (status == GContainment.OUTSIDE))
                    return status;
            }

            return desired;  // So as not to adversely affect the result.
        }

        private void Split(GChain path)
        {
            GITable itable = new GITable();
            GChain garbage = new GChain(GChainType.UNKNOWN);

            // NOTE: The provided winding direction here is irrelevant.
            GDegouger degouger = new GDegouger(GConst.CCW);
            degouger.Split(path, itable);

            foreach (var entry in itable)
            {
                foreach (var subchn in entry.Value)
                {
                    if ((subchn == path.FirstSubchain()) || (subchn == path.TerminalSubchain()))
                        continue;

                    subchn.Unlink();
                }
            }
        }

        private GChain ChainCreate(GChChain chChain, GChainType chainType)
        {
            GChain chain = new GChain(GChainType.UNKNOWN);
            chain.ChainType = chainType;
            chain.layer = ((chainType == GChainType.PART) ? Layer.PART : Layer.TOOL);

            GChain refChain = chChain.Chain();
            GChArc curr = (GChArc)refChain.FirstCurve();
            while (curr != null)
            {
                GCurve nextCurve = curr.NextCurve();
                GChArc next = (((nextCurve == null) || nextCurve.IsA(T.SUBCHN)) ? null : (GChArc)nextCurve)!;

                GPoint ps = chain.pe;
                if (curr.rad > 0)
                {
                    GArc arc = GArc.ArcCreate(curr.pc, curr.rad, curr.sa, curr.ea);
                    if (arc != null)
                    {
                        if (ps.IsValid() && !arc.ps.WithinTol(ps))
                        {
                            GLine line = GLine.LineCreate(ps, arc.ps);
                            if (line != null)
                                chain.Append(line);
                        }

                        chain.Append(arc);
                    }
                }
                else
                {
                    GPoint pe = curr.pc;
                    if ((next == null) || ps.IsValid())
                    {
                        GLine line = GLine.LineCreate(ps, pe);
                        if (line != null)
                            chain.Append(line);
                    }
                    else
                    {
                        chain.pe = pe;
                    }
                }

                curr = next!;
            }

            if (refChain.IsClosed && !chain.IsClosed)
            {
                GLine line = GLine.LineCreate(chain.pe, chain.ps);
                if (line != null)
                    chain.Append(line);
            }

            return chain;
        }

        private void LineAppend(GChain chain, GLine? line)
        {
            if (line == null)
                return;

            chain.Append(line);
        }

        private void ArcAppend(GChain chain, bool isBlend, GArc arc)
        {
            if (arc == null)
                return;

            arc.IsBlend = isBlend;
            chain.Append(arc);
        }

        private void ArcsSplit(GChain chain)
        {
            GCurve curr = chain.FirstCurve();
            while (!curr.IsA(T.SUBCHN))
            {
                GCurve next = curr.NextCurve();

                if (curr.IsA(T.ARC))
                    ArcSplit((GArc)curr);

                curr = next;
            }
        }

        private void ArcSplit(GArc arc)
        {
            double arcSang = arc.sa;
            double arcEang = arc.ea;

            double sang = arcSang;
            while (true)
            {
                bool split = false;
                double eang = GChSolve.QuadEndAng(sang, arc.dir);
                if (arc.dir == GConst.CCW)
                    split = ((sang >= arcSang) && (eang < arcEang));
                else
                    split = ((sang <= arcSang) && (eang > arcEang));

                if (!split)
                    break;

                GPoint pe = arc.pc + (new GVec(eang) * arc.rad);
                GArc post = arc.SplitAt(pe);

                arc.Append(post);
                arc = post;

                sang = eang;
            }
        }
    }

    internal class GChChain
    {
        private GChain chain;
        public GChArc curr;

        // Experimental
        SortedDictionary<int, GChArc> arcs = new SortedDictionary<int, GChArc>();

        public GChChain(GChain chain, GChainType type)
        {
            Debug.Assert(chain.IsClosed, "GChChain() - requires closed chain");
            Debug.Assert((chain.Winding > 0), "GChChain() - requires CCW winding");

            this.chain = new GChain(GChainType.UNKNOWN);
            this.chain.ChainType = type;
            this.IsClosed = chain.IsClosed;
            this.IsConvex = true;

            double blendRadius = ((type == GChainType.TOOL) ? -1 : 0);

            GCurve prev = chain.TerminalCurve();

            GChArc chArc = null!;
            GCurve curve = chain.FirstCurve();
            while (true)
            {
                if (curve.IsA(T.SUBCHN))
                    break;

                GCurve next = curve.NextCurve();

                GVec svec = prev.TangentAt(1);
                GVec evec = curve.TangentAt(0);
                double sa = svec.Radians();
                double ea = evec.Radians();

                double cross = svec ^ evec;
                int winding = ((System.Math.Abs(cross) <= GConst.VECTOR_SMALL) ? 0 : System.Math.Sign(cross));  // TODO: What if winding == 0 (?)

                if (curve.IsA(T.LINE))
                {
                    GLine line = (GLine)curve;

                    // Debug.Assert((winding != 0), "GChChain() - not implemented");
                    if (winding == 0)
                    {
                        // ASSUMPTION: chain starts with a line
                        sa -= GConst.HALF_PI;
                        if (sa < 0)
                            sa += GConst.TWO_PI;

                        ea -= GConst.HALF_PI;
                        if (ea < sa)
                            ea += GConst.TWO_PI;
                    }
                    else
                    {
                        if (winding < 0)
                        {
                            // The chain has CCW winding but this corner is CW.

                            // Rotate the tangent vectors (angles) so we can treat
                            // them as if they are radial vectors of a blending arc.
                            sa += GConst.HALF_PI;
                            ea += GConst.HALF_PI;

                            if (sa > GConst.TWO_PI)
                                sa -= GConst.TWO_PI;

                            if (ea > sa)
                                ea -= GConst.TWO_PI;

                            this.IsConvex = false;
                        }
                        else
                        {
                            sa -= GConst.HALF_PI;
                            if (sa < 0)
                                sa += GConst.TWO_PI;

                            ea -= GConst.HALF_PI;
                            if (ea < sa)
                                ea += GConst.TWO_PI;
                        }
                    }

                    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
                    // TODO: Not sure this section is correct.
                    // TODO: Both line & arc sections have common blending-arc code.
                    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
                    GChArc terminal = (GChArc)this.chain.TerminalCurve();
                    if (terminal == null)  // i.e. the chain is empty (TODO: Fix FirstCurve() ?)
                    {
                        if (ea != sa)
                            ArcsAppend(line.ps, blendRadius, sa, ea);

                    }
                    else if (terminal.rad == 0)
                    {
                        ArcsAppend(line.ps, blendRadius, sa, ea);
                    }
                    else if (next.IsA(T.SUBCHN))
                    {
                        if (!chain.IsClosed)
                            ArcsAppend(line.pe, blendRadius, sa, ea);
                    }
                    else if (winding != 0)
                    {
                        // Insert a blending arc(s).
                        ArcsAppend(curve.ps, blendRadius, sa, ea);
                    }

                    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
                }
                else if (curve.IsA(T.ARC))
                {
                    GArc arc = (GArc)curve;
                    if (winding != 0)
                    {
                        if (winding < 0)
                        {
                            // The chain has CCW winding but this corner is CW.
                            sa += GConst.HALF_PI;
                            ea += GConst.HALF_PI;
                            if (ea > sa)
                                ea -= GConst.TWO_PI;

                            this.IsConvex = false;
                        }
                        else
                        {
                            sa -= GConst.HALF_PI;
                            if (sa < 0)
                                sa += GConst.TWO_PI;

                            ea -= GConst.HALF_PI;
                            if (ea < sa)
                                ea += GConst.TWO_PI;
                        }

                        // Insert blending arc(s).
                        ArcsAppend(curve.ps, 0, sa, ea);
                    }

                    chArc = new GChArc(arc.pc, arc.rad, arc.sa, arc.ea, arc.dir);
                    this.chain.Append(chArc);
                }

                prev = curve;
                curve = next;
            }

            if (chain.IsClosed)
            {
                GChArc first = FirstCurve();
                GChArc terminal = TerminalCurve();

                GVec svec = new GVec(first.sa);
                GVec evec = new GVec(terminal.ea);

                GPoint ps = first.pc + (svec * first.rad);
                GPoint pe = terminal.pc + (evec * terminal.rad);
                if (!ps.WithinTol(pe) && (terminal.rad > 0))
                {
                    double cross = svec ^ evec;
                    if (System.Math.Abs(cross) > GConst.VECTOR_SMALL)
                    {
                        // TODO: BEWARE, this might not properly handle all cases.
                        //    Was introduced to "fix" issue found with ch005b.dxf
                        double sa = terminal.ea;
                        double ea = first.sa;
                        if (cross > 0)
                        {
                            sa -= GConst.PI;
                            if (sa < 0)
                                sa += GConst.TWO_PI;
                        }

                        if (ea != sa)
                            ArcsAppend(pe, blendRadius, sa, ea);
                    }
                }
            }

            this.curr = FirstCurve();
            this.box = chain.box;
        }

        public bool IsTool { get { return this.chain.ChainType == GChainType.TOOL; } }

        // PREREQUISITE: The tool must be split prior to calling ToolRadius()
        public double ToolRadius()
        {
            double radius = 0;

            // TODO: Account for arcs and not just corners.
            GCurve curr = this.FirstCurve();
            while (true)
            {
                if (curr.IsA(T.SUBCHN))
                    break;

                GChArc chArc = (GChArc)curr;
                double rad = System.Math.Sqrt((chArc.pc.x * chArc.pc.x) + (chArc.pc.y * chArc.pc.y));
                if (rad > radius)
                    radius = rad;

                curr = curr.NextCurve();
            }

            return radius;
        }

        private void ArcsAppend(GPoint pc, double radius, double sa, double ea)
        {
            int winding = System.Math.Sign(ea - sa);

            List<double> angles = new List<double>();
            angles.Add(sa);

            GChArc chArc;
            double quadEnd;
            while (true)
            {
                quadEnd = GChSolve.QuadEndAng(sa, winding);

                if ((winding > 0) && (quadEnd < ea))
                {
                    // Do nothing.
                }
                else if ((winding < 0) && (quadEnd > ea))
                {
                    // Do nothing.
                }
                else
                {
                    break;
                }

                chArc = new GChArc(pc, radius, sa, quadEnd, winding);
                this.chain.Append(chArc);

                sa = quadEnd;
            }

            chArc = new GChArc(pc, radius, sa, ea, winding);
            this.chain.Append(chArc);
        }

        public bool IsClosed { get; private set; } = false;

        public bool IsConvex { get; private set; } = false;

        public bool IsNextDir(int dir)
        {
            GChArc next = null!;
            GChArc iter = curr;

            GPoint pc = curr.pc;
            while (true)
            {
                next = (GChArc)(iter.NextCurve().IsA(T.SUBCHN) ? chain.FirstCurve() : iter.NextCurve());
                if (!next.pc.WithinTol(pc))
                    break;

                iter = next;
            }

            return (((GChArc)next).dir == dir);
        }

        // TODO: don't really want these to be public
        public GChArc FirstCurve() { return (GChArc)this.chain.FirstCurve(); }
        public GChArc TerminalCurve() { return (GChArc)this.chain.TerminalCurve(); }

        // TODO: This is for convenience (don't really want to duplicate the GChain interface right now).
        public GChain Chain() { return this.chain; }

        public void Dump()
        {
            chain.Dump();
        }

        // Must call once before ever using CurrSet().
        public void ArcMapInit()
        {
            arcs.Clear();

            GCurve aiter = this.chain.FirstSubchain();
            while (true)
            {
                aiter = aiter.NextCurve();
                if (aiter.IsA(T.SUBCHN))
                    break;

                GChArc arc = (GChArc)aiter;
                arcs[arc.id] = arc;
            }
        }

        public void ContactCandidatesGet(GChArc partArc, GChain tool, int side, out List<GChArc> candidates)
        {
            SortedList<int, int> experiment = new SortedList<int, int>();
            SortedList<double, GChArc> tmp = new SortedList<double, GChArc>();

            double toolRadius = this.ToolRadius();
            GArc circle = GArc.CircleCreate(new GPoint(0, 0), toolRadius, GConst.CCW);

            double sa = partArc.sa;
            double ea = partArc.ea;

            double ma = (sa + ea) / 2;
            if (partArc.dir == GConst.CW)
                ma += GConst.PI;

            GVec pnorm = new GVec(ma);

            //=-=-=
            GVec flipped = new GVec(ma + GConst.PI);
            GPoint pm = new GPoint(0, 0) + (flipped * toolRadius)!;
            GPoint ps = pm + ((flipped + -GConst.HALF_PI) * toolRadius)!;
            GPoint pe = pm + ((flipped + GConst.HALF_PI) * toolRadius)!;
            GLine line = GLine.LineCreate(ps, pe)!;

            // NOTE: Simply need any arbitrary end points as they will be overwritten.
            GLine ray = GLine.LineCreate(ps, pe)!;
            //=-=-=

            candidates = new List<GChArc>();

            Closest closest = new NotClosest();
            GPoint origin = new GPoint(0, 0);
            GCurve iter = this.chain.FirstCurve();
            while (iter != null)
            {
                if (iter.IsA(T.SUBCHN))
                    break;

                GChArc toolArc = (GChArc)iter;

                bool skip = ((side == GConst.RIGHT)
                    ? (partArc.dir == GConst.CW) && (toolArc.dir == GConst.CW)
                    : (partArc.dir == GConst.CCW) && (toolArc.dir == GConst.CW));

                if (skip)
                {
                    iter = iter.NextCurve();
                    continue;
                }

                // Since the tool always has CCW winding.
                ma = (toolArc.sa + toolArc.ea) / 2;
                if (toolArc.dir == GConst.CW)
                    ma += GConst.PI;

                GVec inorm = new GVec(ma);
                double dot = inorm * pnorm;
                if (dot < -0.5)
                {
                    bool accept = true;

                    if (side == GConst.LEFT)
                    {
                        if (toolArc.dir == partArc.dir)
                        {
                            double rdiff = ((toolArc.dir == GConst.CCW) ? partArc.rad - toolArc.rad : toolArc.rad - partArc.rad);
                            accept = (rdiff >= 0);
                        }
                    }
                    else
                    {
                        if (toolArc.dir != partArc.dir)
                        {
                            double rdiff = ((toolArc.dir == GConst.CCW) ? partArc.rad - toolArc.rad : toolArc.rad - partArc.rad);
                            accept = (rdiff >= 0);
                        }
                    }

                    if (accept)
                    {
                        // Is line actually "visible" from this point?
                        // TODO: Account for the point's radius. Currently, this solution
                        // assumes only sharp corners.

                        closest = line.Closest(toolArc.pc);
                        if (closest.Distance >= GConst.SMALL)
                        {
                            ray.ps = toolArc.pc;
                            ray.pe = ray.ps + (flipped * closest.Distance);

                            GCurve tcurve = tool.FirstCurve();
                            while (tcurve != null)
                            {
                                if (tcurve.IsA(T.SUBCHN))
                                    break;

                                bool contiguous = false;
                                GIntersections results = GIntersect.CurveCurve(ray, tcurve, contiguous, true);
                                foreach(var result in results)
                                {
                                    accept = result.pt.WithinTol(ray.ps);
                                    if (!accept)
                                        break;
                                }

                                if (!accept)
                                    break;

                                tcurve = tcurve.NextCurve();
                            }
                        }
                    }

                    if (accept)
                    {
                        ps = toolArc.pc + (new GVec(toolArc.sa) * toolArc.rad);
                        closest = line.Closest(ps);
                        if (!tmp.ContainsKey(closest.Uparam))
                        {
                            tmp[closest.Uparam] = toolArc;
                            experiment[toolArc.id] = toolArc.ordinal;
                        }
                        else
                        {
                            tmp[closest.Uparam + 1e-12] = toolArc;
                            experiment[toolArc.id] = toolArc.ordinal;
                        }

                        if (toolArc.rad > 0)
                        {
                            pe = toolArc.pc + (new GVec(toolArc.ea) * toolArc.rad);
                            closest = line.Closest(pe);
                            if (!tmp.ContainsKey(closest.Uparam))
                            {
                                tmp[closest.Uparam] = toolArc;
                                experiment[toolArc.id] = toolArc.ordinal;
                            }
                            else
                            {
                                tmp[closest.Uparam + 1e-12] = toolArc;
                                experiment[toolArc.id] = toolArc.ordinal;
                            }
                        }
                    }
                }

                iter = iter.NextCurve();
            }

            if (GConfig.Values.DumpContactCandidatesInternal)
            {
                Debug.WriteLine("");
                Debug.WriteLine("partArc.id: {0}", partArc.id);
                Debug.WriteLine("tmp: {0}", tmp.Count);
                foreach (var entry in tmp)
                {
                    Debug.WriteLine("u: {{{0}}} id: {{{1}}} ord: {{{2}}}", entry.Key, entry.Value.id, experiment[entry.Value.id]);
                }
                Debug.WriteLine("");
            }

            int limit = ((tmp.Count < 3) ? tmp.Count : 2);
            for (int indx = 0; indx < limit; ++indx)
            {
                candidates.Add(tmp.ElementAt(indx).Value);
            }

            if (tmp.Count > 2)
            {
                // min/max assumption!!!
                int minOrd = candidates[0].ordinal;
                int maxOrd = candidates[1].ordinal;
                int maxOrdIndex = 1;

                for (int indx = 2; indx < tmp.Count; ++indx)
                {
                    GChArc arc = tmp.ElementAt(indx).Value;

                    if (arc.ordinal > maxOrd)
                    {
                        maxOrd = arc.ordinal;
                        candidates.Add(arc);
                        maxOrdIndex = candidates.Count - 1;
                    }
                    else if (arc.ordinal < minOrd)
                    {
                        candidates.Add(arc);
                    }
                    else
                    {
                        int jndx;
                        for (jndx = 1; jndx < maxOrdIndex; ++jndx)
                        {
                            if (arc.ordinal > candidates[jndx].ordinal)
                            {
                                candidates.Insert(jndx + 1, arc);
                                break;
                            }
                        }

                        if (jndx >= candidates.Count)
                            candidates.Add(arc);
                    }
                }
            }
        }

        public GPoint PositionAt(GChArc part, double uparam, int side, bool advanceOnPart, bool earlyExit)
        {
            GPoint pt = new GPoint(GPoint.UNDEFINED);
            bool solved = false;

            double sa = part.sa;
            double ea = part.ea;

            double ma = (sa + ea) / 2;
            if ((side * part.dir) < 0)
                ma += GConst.PI;

            GVec pnorm = new GVec(ma);

            // Debug.Write("PART ");
            // part.Dump();

            // When offsetting to the outside of an arc rotate the part angles by PI.
            // The required condition for this action is [(part_winding * side * arc_dir) < 0]
            // Assuming the part_winding is CCW this reduces to [(side * arc_dir) < 0]
            if (advanceOnPart && ((side * part.dir) < 0))
            {
                if (part.dir == GConst.CCW)
                {
                    sa += GConst.PI;
                    ea += GConst.PI;

                    if (sa >= GConst.TWO_PI)
                    {
                        sa -= GConst.TWO_PI;
                        ea -= GConst.TWO_PI;
                    }
                    else if (ea <= 0)
                    {
                        sa += GConst.TWO_PI;
                        ea += GConst.TWO_PI;
                    }
                }
                else
                {
                    if (ea < 0)
                    {
                        sa += GConst.TWO_PI;
                        ea += GConst.TWO_PI;
                    }
                }
            }

            GVec pvec = new GVec(sa + (uparam * (ea - sa)));

            if (curr == null)
                curr = (GChArc)this.chain.FirstCurve();

            GChArc iter = curr;
            while (iter != null)
            {
                // Since the tool always has CCW winding.
                ma = (iter.sa + iter.ea) / 2;
                if (iter.dir == GConst.CW)
                    ma += GConst.PI;

                GVec inorm = new GVec(ma);
                double dot = inorm * pnorm;
                if (dot >= 0)
                {
                    if (part.dir == GConst.CCW)
                    {
                        if (advanceOnPart)
                        {
                            double dsa = System.Math.Abs(sa - iter.sa);
                            double dea = System.Math.Abs(iter.ea - ea);
                            if ((dsa <= GConst.VECTOR_SMALL) || (dea <= GConst.VECTOR_SMALL))
                                solved = true;
                        }
                        else
                        {
                            if ((ea >= iter.sa) && (sa <= iter.ea))
                                solved = true;
                        }
                    }
                    else
                    {
                        double isa = iter.sa;
                        double iea = iter.ea;
                        if (side == GConst.LEFT)
                        {
                            isa += GConst.PI;
                            iea += GConst.PI;
                        }

                        if (isa >= GConst.TWO_PI)
                        {
                            isa -= GConst.TWO_PI;
                            iea -= GConst.TWO_PI;
                        }

                        double tmp = isa;
                        isa = iea;
                        iea = tmp;

                        double dsa = System.Math.Abs(sa - isa);
                        double dea = System.Math.Abs(iea - ea);
                        if ((dsa <= GConst.VECTOR_SMALL) || (dea <= GConst.VECTOR_SMALL))
                            solved = true;
                    }
                }

                if (solved || earlyExit)
                    break;

                GCurve next = iter.NextCurve();
                iter = (GChArc)(next.IsA(T.SUBCHN) ? this.chain.FirstCurve() : next);

                if (iter == curr)
                    iter = null!;  // We've come full circle
            }

            curr = iter!;
            if (solved)
            {
                GVec ovec = new GVec(curr.pc.x, curr.pc.y);

                double toolRadius = (curr.IsBlend ? 0 : curr.rad);

                // NOTE: I don't understand why the solution is asymmetrical.
                if (part.dir == GConst.CCW)
                {
                    double rad = part.rad + (toolRadius * -side);
                    pt = part.pc + (pvec * (rad * side))! - ovec;
                }
                else
                {
                    double rad = part.rad + (toolRadius * side);
                    pt = part.pc + (pvec * rad)! - ovec;
                }
            }

            return pt;
        }

        public GBox box { get; private set; }

        public GBox BoxAt(GPoint pt)
        {
            this.box.MoveTo(pt);
            return this.box;
        }

        public void MoveTo(GPoint pt)
        {
            GCurve curr = this.chain.FirstSubchain();
            while (true)
            {
                curr = curr.NextCurve();
                if (curr.IsA(T.SUBCHN))
                    break;

                GPoint pc = ((GChArc)curr).pc + new GVec(pt.x, pt.y);
                ((GChArc)curr).Relocate(pc);
            }
        }

        // Experimental
        public bool CurrSet(int id)
        {
            if (arcs.Count == 0)
            {
                GCurve iter = this.chain.FirstSubchain();
                while (true)
                {
                    iter = iter.NextCurve();
                    if (iter.IsA(T.SUBCHN))
                        break;

                    GChArc arc = (GChArc)iter;
                    arcs[arc.id] = arc;
                }
            }

            if (arcs.ContainsKey(id))
            {
                curr = arcs[id];
                return true;
            }

            return false;
        }
    }

    internal class GChSolve
    {
        // CCW - 0 .. 2PI
        //  CW - 2PI .. 0
        public static double QuadEndAng(double sang, int dir)
        {
            double sina = System.Math.Sin(sang);
            if ((System.Math.Abs(sina) < GConst.VECTOR_SMALL) || (System.Math.Abs(sina) > (1 - GConst.VECTOR_SMALL)))
                return (sang + ((dir == GConst.CCW) ? GConst.HALF_PI : -GConst.HALF_PI));  // ends on a quadrant boundary

            double cosa = System.Math.Cos(sang);
            if (cosa > 0)
            {
                if (dir == GConst.CCW)
                    return ((sina > 0) ? GConst.HALF_PI : GConst.TWO_PI);
                else
                    return ((sina > 0)
                        ? ((sang > GConst.TWO_PI) ? GConst.TWO_PI : 0)
                        : (3 * GConst.HALF_PI));
            }
            else
            {
                if (dir == GConst.CCW)
                    return ((sina > 0) ? GConst.PI : (3 * GConst.HALF_PI));
                else
                    return ((sina > 0) ? GConst.HALF_PI : GConst.PI);
            }
        }
    }

    internal class GApt : GPoint
    {
        public GApt(GPoint pt, int pid, int tid)
            : base(pt)
        {
            this.pid = pid;
            this.tid = tid;
        }

        public int pid;
        public int tid;
    }

    internal class GPointComparer : IComparer<GApt>
    {
        public int Compare(GApt? lhs, GApt? rhs)
        {
            if ((lhs! == null!) || (rhs! == null!))
                throw new InvalidOperationException("Compare() has null argument.");

            if (lhs.x == rhs.x)
            {
                if (lhs.y == rhs.y)
                    return 0;
                else
                    return System.Math.Sign(lhs.y - rhs.y);
            }
            else
            {
                return System.Math.Sign(lhs.x - rhs.x);
            }
        }
    }
}
