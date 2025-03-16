using Offsetter.Entities;
using Offsetter.Math;
using System.Collections.Generic;

namespace Offsetter.Solver
{
    using GChainList = List<GChain>;

    // Like duh! null returns are intentional here in these methods
    //   warning CS8600: Converting null literal or possible null value to non-nullable type
#pragma warning disable CS8600

    public class GDegouger
    {
        public GDegouger(int winding)
        {
            this.winding = winding;
        }

        public bool AcceptTangencies { get; set; } = false;

        public void Degouge(in GChain ichain, GChainList results)
        {
            GITable itable = new GITable();

            if (GLogger.Active)
                GLogger.Log("Degouge(in GChain ichain, ...)");

            GPoint origin = new GPoint(0, 0);
            GChain copy = ichain.Clone(origin);
            Split(copy, itable);

            bool bypass = false;
            if (bypass)
            {
                results.Add(copy);
            }
            else
            {
                int minIindex = Stitch(ref copy, in itable);
                Extract(copy, copy, in itable, minIindex, results);  // experiment
            }
        }

        // TODO: Instead of myriad functions, just one IsA(type) (?)
        public void ChainsDegouge(GChainList chains, out GChainList rchains)
        {
            GITable itable = new GITable();

            rchains = new GChainList();

            for (int indx = 0; indx < chains.Count; ++indx)
            {
                for (int jndx = indx+1; jndx < chains.Count; ++jndx)
                {
                    Split(chains[indx], chains[jndx], itable);
                }
            }

            for (int indx = 0; indx < chains.Count; ++indx)
            {
                GChain island = chains[indx];
                if (island.HasSubchains())
                    continue;

                chains.RemoveAt(indx);
                rchains.Add(island);

                --indx;
            }

            int minIindex = Stitch(chains, in itable);

            Extract(chains, in itable, minIindex, rchains);
        }

        public void Split(GChain ichain, GITable itable)
        {
            GCurveIterator forwardIter = ichain.CurveIterator();
            while (true)
            {
                GCurve icurve = forwardIter.Curve();
                if (icurve == null)
                    break;

                if (icurve.CanIgnore)
                {
                    forwardIter.Next();
                    continue;
                }

                GCurve jcurve = ichain.TerminalCurve();
                while (jcurve != icurve)
                {
                    if ((jcurve.PrevCurve() == icurve.NextCurve()) && jcurve.PrevCurve().IsA(T.SUBCHN))
                        break;  // we've come full circle

                    if (jcurve.CanIgnore)
                    {
                        jcurve = jcurve.PrevCurve();  // we've encounter a blend or subchn
                        continue;
                    }

                    if (PreviouslyIntersected(icurve, jcurve))
                    {
                        jcurve = jcurve.PrevCurve();
                        continue;
                    }

                    if (Fencepost(icurve, jcurve))
                    {
                        jcurve = jcurve.PrevCurve();  // The chain start and end points are coincident.
                        continue;
                    }

                    bool contiguous = (jcurve == icurve.Next);
                    GIntersections results = GIntersect.CurveCurve(icurve, jcurve, contiguous, true);

                    GSubchn subchn;
                    if (results.Count == 1)
                    {
                        if (!CanIgnore(icurve, jcurve, results[0]))
                        {
                            subchn = ichain.SplitAt(jcurve, results[0].pt);
                            itable.Record(subchn);

                            subchn = ichain.SplitAt(icurve, results[0].pt);
                            itable.Record(subchn);
                        }
                    }
                    else if (results.Count == 2)
                    {
                        if (results[1].uB > results[0].uB)
                        {
                            subchn = ichain.SplitAt(jcurve, results[1].pt);
                            itable.Record(subchn);

                            subchn = ichain.SplitAt(jcurve, results[0].pt);
                            itable.Record(subchn);
                        }
                        else
                        {
                            subchn = ichain.SplitAt(jcurve, results[0].pt);
                            itable.Record(subchn);

                            subchn = ichain.SplitAt(jcurve, results[1].pt);
                            itable.Record(subchn);
                        }

                        if (results[1].uA > results[0].uA)
                        {
                            subchn = ichain.SplitAt(icurve, results[1].pt);
                            itable.Record(subchn);

                            subchn = ichain.SplitAt(icurve, results[0].pt);
                            itable.Record(subchn);
                        }
                        else
                        {
                            subchn = ichain.SplitAt(icurve, results[0].pt);
                            itable.Record(subchn);

                            subchn = ichain.SplitAt(icurve, results[1].pt);
                            itable.Record(subchn);
                        }
                    }

                    jcurve = jcurve.PrevCurve();
                }

                forwardIter.Next();
            }

            itable.Clear();
            ichain.SubchainsRecord(ref itable);

            if (GLogger.Active)
            {
                itable.Log(null);
                ichain.Log("ichain", null);
            }
        }

        private void Split(GChain pocket, GChain island, GITable itable)
        {
            GLogger.Assert((pocket != island), "GDegouger.Split(pocket, island, ...)");
            if (pocket == island)
                return;

            GCurve pcurve = pocket.FirstSubchain();
            while (pcurve != null)
            {
                if (pcurve == null)
                    break;

                if (pcurve.CanIgnore)
                {
                    if (pcurve == pocket.TerminalSubchain())
                        break;

                    pcurve = pcurve.NextCurve();
                    continue;
                }

                GCurve icurve = island.TerminalCurve();
                while (icurve != null)
                {
                    if (icurve.CanIgnore)
                    {
                        if (icurve == island.FirstSubchain())
                            break;

                        icurve = icurve.PrevCurve();  // we've encounter a blend or subchn
                        continue;
                    }

                    if (PreviouslyIntersected(pcurve, icurve))
                    {
                        icurve = icurve.PrevCurve();
                        continue;
                    }

                    bool contiguous = false;
                    GIntersections results = GIntersect.CurveCurve(pcurve, icurve, contiguous, true);

                    GSubchn subchn;
                    if (results.Count == 1)
                    {
                        if (!CanIgnore(pcurve, icurve, results[0]))
                        {
                            subchn = island.SplitAt(icurve, results[0].pt);
                            itable.Record(subchn);

                            subchn = pocket.SplitAt(pcurve, results[0].pt);
                            itable.Record(subchn);
                        }
                    }
                    else if (results.Count == 2)
                    {
                        if (results[1].uB > results[0].uB)
                        {
                            subchn = island.SplitAt(icurve, results[1].pt);
                            itable.Record(subchn);

                            subchn = island.SplitAt(icurve, results[0].pt);
                            itable.Record(subchn);
                        }
                        else
                        {
                            subchn = island.SplitAt(icurve, results[0].pt);
                            itable.Record(subchn);

                            subchn = island.SplitAt(icurve, results[1].pt);
                            itable.Record(subchn);
                        }

                        if (results[1].uA > results[0].uA)
                        {
                            subchn = pocket.SplitAt(pcurve, results[1].pt);
                            itable.Record(subchn);

                            subchn = pocket.SplitAt(pcurve, results[0].pt);
                            itable.Record(subchn);
                        }
                        else
                        {
                            subchn = pocket.SplitAt(pcurve, results[0].pt);
                            itable.Record(subchn);

                            subchn = pocket.SplitAt(pcurve, results[1].pt);
                            itable.Record(subchn);
                        }
                    }

                    icurve = icurve.PrevCurve();
                }

                pcurve = pcurve.NextCurve();
            }

            pocket.SubchainsRecord(ref itable);
            island.SubchainsRecord(ref itable);

            if (GLogger.Active)
            {
                itable.Log(null);
                pocket.Log("pocket", null);
                island.Log("island", null);
            }
        }

        private bool CanIgnore(GCurve curveA, GCurve curveB, GIntersection intersection)
        {
            if (intersection.tangent && !AcceptTangencies)
                return true;

            if (CanIgnoreBlend(curveA, intersection.pt))
                return true;
            
            if (CanIgnoreBlend(curveB, intersection.pt))
                return true;

            double dot = 1;

            if ((intersection.uA == 0) && (intersection.uB == 1))
            {
                GVec tanAp = curveA.PrevCurve().TangentAt(1);
                GVec tanBn = curveB.NextCurve().TangentAt(0);
                dot = tanAp * tanBn;
                return (dot < -GConst.VECTOR_SMALL);
            }

            if ((intersection.uA == 1) && (intersection.uB == 0))
            {
                GVec tanAn = curveA.NextCurve().TangentAt(0);
                GVec tanBp = curveB.PrevCurve().TangentAt(1);
                dot = tanAn * tanBp;
                return (dot < -GConst.VECTOR_SMALL);
            }

            if ((intersection.uA == 0) && (intersection.uB == 0))
            {
                GVec tanA = curveA.TangentAt(0);
                GVec tanBp = curveB.PrevCurve().TangentAt(0);
                dot = tanA * tanBp;
                if (dot < -GConst.VECTOR_SMALL)
                    return true;

                GVec tanAp = curveA.PrevCurve().TangentAt(1);
                GVec tanB = curveB.TangentAt(0);
                dot = tanAp * tanB;
                if (dot < -GConst.VECTOR_SMALL)
                    return true;
            }

            if ((intersection.uA == 1) && (intersection.uB == 1))
            {
                GVec tanAn = curveA.NextCurve().TangentAt(0);
                GVec tanB = curveB.TangentAt(1);
                dot = tanAn * tanB;
                if (dot < -GConst.VECTOR_SMALL)
                    return true;

                GVec tanA = curveA.TangentAt(1);
                GVec tanBn = curveB.NextCurve().TangentAt(0);
                dot = tanA * tanBn;
                if (dot < -GConst.VECTOR_SMALL)
                    return true;
            }

            return false;
        }

        private bool CanIgnoreBlend(GCurve curve, GPoint pt)
        {
            bool canIgnore = false;

            if (curve.ps == pt)
                canIgnore = curve.PrevCurve().IsBlend;
            else if (curve.pe == pt)
                canIgnore = curve.NextCurve().IsBlend;

            return canIgnore;
        }

        private int Stitch(ref GChain copy, in GITable itable)
        {
            int minIindex = GConst.IUNDEFINED;

            int iindex = 0;

            GSubchn curr = copy.FirstSubchain();
            while (true)
            {
                if (curr == null)
                    break;

                if (curr.IsAssigned())
                    iindex = curr.iindex;
                else
                    Assign(curr, itable, ref iindex);

                if (iindex < minIindex)
                    minIindex = iindex;

                curr = copy.NextSubchain(curr);
            }

            if (GLogger.Active)
                copy.Log("copy", null);

            return minIindex;
        }

        private int Stitch(GChain pocket, GChain island, in GITable itable)
        {
            int minIindex = GConst.IUNDEFINED;

            int iindex = 0;

            GSubchn curr = pocket.FirstSubchain();
            while (true)
            {
                if (curr == null)
                    break;

                if (!curr.IsAssigned())
                    Assign(curr, itable, ref iindex);

                if (iindex < minIindex)
                    minIindex = iindex;

                curr = pocket.NextSubchain(curr);
            }

            pocket.TerminalSubchain().iindex = pocket.FirstSubchain().iindex;
            island.TerminalSubchain().iindex = island.FirstSubchain().iindex;

            if (GLogger.Active)
            {
                pocket.Log("pocket", null);
                if (pocket.id != island.id)
                    island.Log("island", null);
            }

            return minIindex;
        }

        private int Stitch(GChainList chains, in GITable itable)
        {
            int minIindex = GConst.IUNDEFINED;

            int iindex = 0;

            for (int indx = 0; indx < chains.Count; ++indx)
            {
                GChain chain = chains[indx];
                chain.SubchainsReorder(itable);

                GSubchn start = chain.FirstAssignedSubchain();
                if (start == null)
                    start = chain.FirstSubchain();
                
                GSubchn curr = start;
                while (true)
                {
                    if (curr.IsAssigned())
                        iindex = curr.iindex;
                    else
                        Assign(curr, itable, ref iindex);

                    if (iindex < minIindex)
                        minIindex = iindex;

                    curr = chain.NextSubchain(curr);
                    if (curr == null)
                        curr = chain.FirstSubchain();  // We've reached the end of the chain.

                    if (curr == start)
                        break;
                }
            }

            return minIindex;
        }

        private void Extract(GChainList chains, in GITable itable, int minIindex, GChainList results)
        {
            while (chains.Count > 0)
            {
                GChain chain = chains[0];

                // Start the chain at an intersection (if one exists).
                if (chain.SubchainsReorder(itable))
                {
                    // The chain is free of intersections.
                    chains.RemoveAt(0);
                    results.Add(chain);
                }
                else
                {
                    GChain result = new GChain(GChainType.UNKNOWN);

                    while (true)
                    {
                        GSubchn subchn = chain.FirstMinSubchain(minIindex);
                        if (subchn == null)
                            break;

                        while (true)
                        {
                            if (subchn.Owner.ChainType == GChainType.POCKET)
                                result.ChainType = GChainType.POCKET;

                            // TODO: At every use of SubchainReparent() the subchn object should be
                            // removed from its parent chain(?) Doing this will require reordering
                            // some operations, allowing subsequently used data to obtain via the
                            // subchn before it is destroyed. This may turn out to be problematic.
                            result.SubchainReparent(chain, subchn);
                            subchn.IsValid = false;

                            GSubchn next = itable.Sister(chain, subchn);
                            chain.Unlink(ref subchn);

                            // if (next == null)
                            if ((next == null) || (next.Owner == null))
                            {
                                if (result.IsClosed)
                                {
                                    // Recalculate the area and box.
                                    result.PropertiesUpdate();
                                    results.Add(result);
                                }

                                result = new GChain(GChainType.UNKNOWN);
                                chain = chains[0];
                                
                                subchn = chain.FirstMinSubchain(minIindex);
                                if (subchn == null)
                                    break;

                                continue;
                            }

                            subchn = next;
                            chain = subchn.Owner;
                        }
                    }

                    chains.RemoveAt(0);
                }
            }
        }

        // NOTE: When used with a pocket sans chains -- Extract(pocket, pocket, ...);
        private void Extract(GChain pocket, GChain island, in GITable itable, int minIindex, GChainList results)
        {
            GChain ochain = null;

            while (true)
            {
                // Get the first non-empty subchain having a minIindex.
                GSubchn curr = pocket.FirstMinSubchain(minIindex);
                if (curr == null)
                    break;

                if (ochain == null)
                    ochain = new GChain(GChainType.UNKNOWN);

                while (true)
                {
                    ochain.SubchainReparent(pocket, curr);
                    curr.IsValid = false;

                    curr = itable.Sister(pocket, curr);
                    if (curr != null)
                    {
                        if (curr.ipt.WithinTol(ochain.pe))
                            ochain.SubchainReparent(island, curr);
                        else
                            ochain = new GChain(GChainType.UNKNOWN);

                        curr.IsValid = false;

                        GSubchn next = itable.Sister(island, curr);
                        island.Unlink(ref curr);

                        curr = next;
                    }

                    if (curr == null)
                    {
                        if (ochain.IsClosed)
                        {
                            // Recalculate the area and box.
                            ochain.PropertiesUpdate();

                            if (ochain.Winding == winding)
                                results.Add(ochain);
                        }

                        ochain = null;
                        break;
                    }
                }
            }

            for (int indx = 0; indx < results.Count; ++indx)
            {
                string name = string.Format("results[{0}]", indx);
                results[indx].Log(name, null);
            }
        }

        private void Assign(GSubchn curr, in GITable itable, ref int iindex)
        {
            if (curr.Next == null)
                return;  // Terminal subchain.

            IntList list = itable.Value(curr.ipt);
            if (list == null)
            {
                // ASSUMPTION: We're visiting a head/tail node.
                // Otherwise, something is drastically wrong.
                curr.iindex = iindex;
            }
            else if (list.Count == 2)
            {
                // We're visiting a trivial intersection.
                // By convention, pockets are CCW and chains are CW.
                int side = GConst.LEFT;

                GVec vecA = null;
                GVec vecB = null;
                GVec vecC = null;
                GVec vecD = null;
                if (curr.id == list[0].id)
                {
                    if (list[0].PrevCurve() == null)  // TODO: Put this logic into a function
                        vecA = list[0].Owner.TerminalCurve().TangentAt(1);
                    else
                        vecA = list[0].PrevCurve().TangentAt(1);

                    vecB = list[0].NextCurve().TangentAt(0);

                    if (list[1].PrevCurve() == null)
                        vecC = list[1].Owner.TerminalCurve().TangentAt(1) + (GConst.HALF_PI * side);
                    else
                        vecC = list[1].PrevCurve().TangentAt(1) + (GConst.HALF_PI * side);

                    vecD = list[1].NextCurve().TangentAt(0) + (GConst.HALF_PI * side);

                    list[1].iindex = iindex;
                    if (Opposite(vecA, vecC) || Opposite(vecB, vecD))
                        ++iindex;
                    else
                        --iindex;

                    list[0].iindex = iindex;
                }
                else if (curr.id == list[1].id)
                {
                    if (list[1].PrevCurve() == null)  // TODO: Put this logic into a function
                        vecA = list[1].Owner.TerminalCurve().TangentAt(1);
                    else
                        vecA = list[1].PrevCurve().TangentAt(1);

                    vecB = list[1].NextCurve().TangentAt(0);

                    if (list[0].PrevCurve() == null)
                        vecC = list[0].Owner.TerminalCurve().TangentAt(1) + (GConst.HALF_PI * side);
                    else
                        vecC = list[0].PrevCurve().TangentAt(1) + (GConst.HALF_PI * side);

                    vecD = list[0].NextCurve().TangentAt(0) + (GConst.HALF_PI * side);

                    list[0].iindex = iindex;
                    if (Opposite(vecA, vecC) || Opposite(vecB, vecD))
                        ++iindex;
                    else
                        --iindex;

                    list[1].iindex = iindex;
                }
                else
                {
                    GLogger.Assert(false, "GDegouger.Assign() -- something is wrong.");
                }
            }
            else if (list.Count == 3)
            {
                // Special case where the tool fits down a blind slot.
                // file:///C:/_sandbox/Offsetter/docs/SpecialCaseSlot.png
                //    int side = GConst.LEFT;
                int currIindex = iindex;

                int id = curr.id;
                int indx = list.FindIndex(x => (x.id == id));

                if (curr.id == list[0].id)
                {
                    GVec tanA = list[0].PrevCurve().TangentAt(1);
                    GVec tanC = list[1].PrevCurve().TangentAt(1);
                    GVec tanE = list[2].PrevCurve().TangentAt(1);

                    double dotAC = tanA * tanC;
                    double dotCE = tanC * tanE;
                    double dotEA = tanE * tanA;

                    if ((dotAC < dotCE) && (dotAC < dotEA))
                    {
                        double crossAE = tanA ^ tanE;
                        if (crossAE < 0)
                        {
                            --iindex;
                            list[0].iindex = iindex;
                            list[1].iindex = iindex + 1;
                            list[2].iindex = iindex + 1;
                        }
                        else
                        {
                            GLogger.Assert(false, "GDegouger.Assign(#1) -- ooops.");
                        }
                    }
                    else if ((dotCE < dotEA) && (dotCE < dotAC))
                    {
                        double crossAE = tanA ^ tanE;
                        if (crossAE < 0)
                        {
                            --iindex;
                            list[0].iindex = iindex + 1;
                            list[1].iindex = iindex;
                            list[2].iindex = iindex + 1;
                        }
                        else
                        {
                            GLogger.Assert(false, "GDegouger.Assign(#2) -- ooops.");
                        }
                    }
                    else if ((dotEA < dotAC) && (dotEA < dotCE))
                    {
                        double crossAC = tanA ^ tanC;
                        if (crossAC > 0)
                        {
                            ++iindex;
                            list[0].iindex = iindex;
                            list[1].iindex = iindex;
                            list[2].iindex = iindex - 1;
                        }
                        else
                        {
                            GLogger.Assert(false, "GDegouger.Assign(#3) -- ooops.");
                        }
                    }
                    else
                    {
                        GLogger.Assert(false, "GDegouger.Assign(#4) -- ooops.");
                    }
                }
                else
                {
                    GLogger.Assert(false, "GDegouger.Assign() -- ooops.");
                }
            }
            else if (list.Count == 4)
            {
                // file:///C:/_sandbox/Offsetter/docs/SpecialCase01.png
                // ASSUMPTION: all curves intersect as depicted in .png
                // TODO: May need to analyze relationship of tangencies(?)
                list[0].iindex = iindex;
                list[0].PrevSubchain().iindex = iindex;

                list[1].iindex = iindex;
                list[1].PrevSubchain().iindex = iindex;

                list[2].iindex = iindex;
                list[2].PrevSubchain().iindex = iindex;

                list[3].iindex = iindex;
                list[3].PrevSubchain().iindex = iindex;
            }
            else if (list.Count == 1)
            {
                // TODO: Determine how this happens (e.g. pocket06i.dxf -L20) and fix it!
                list[0].iindex = iindex;
            }
            else
            {

                // TODO: Implement this!
                GVec vecA = null;
                GVec vecB = null;
                foreach (var entry in list)
                {
                    vecA = entry.PrevCurve().TangentAt(1);
                    vecB = entry.NextCurve().TangentAt(0);
                }
                GLogger.Assert(false, "GDegouger.Assign() -- not implemented.");
            }
        }

        private bool Opposite(GVec vecA, GVec vecB)
        {
            return ((vecA * vecB) < 0);
        }

        private bool PreviouslyIntersected(GCurve icurve, GCurve jcurve)
        {
#pragma warning disable CS8604
            if (jcurve.PrevCurve().IsA(T.SUBCHN))
            {
                GSubchn jsubchn = (GSubchn)jcurve.PrevCurve();

                GSubchn isubchn = (icurve.PrevCurve().IsA(T.SUBCHN) ? (GSubchn)icurve.PrevCurve() : null);
                if ((isubchn != null) && (isubchn.ipt == jsubchn.ipt))
                    return true;

                isubchn = (icurve.NextCurve().IsA(T.SUBCHN) ? (GSubchn)icurve.NextCurve() : null);
                if ((isubchn != null) && (isubchn.ipt == jsubchn.ipt))
                    return true;
            }
            else if (jcurve.NextCurve().IsA(T.SUBCHN))
            {
                GSubchn jsubchn = (GSubchn)jcurve.NextCurve();

                GSubchn isubchn = (icurve.PrevCurve().IsA(T.SUBCHN) ? (GSubchn)icurve.PrevCurve() : null);
                if ((isubchn != null) && (isubchn.ipt == jsubchn.ipt))
                    return true;

                isubchn = (icurve.NextCurve().IsA(T.SUBCHN) ? (GSubchn)icurve.NextCurve() : null);
                if ((isubchn != null) && (isubchn.ipt == jsubchn.ipt))
                    return true;
            }
#pragma warning restore CS8604

            return false;
        }

        private bool Fencepost(GCurve icurve, GCurve jcurve)
        {
            return (IsFirstCurve(icurve) && IsTerminalCurve(jcurve) && (icurve.ps == jcurve.pe));
        }

        private bool IsFirstCurve(GCurve curve)
        {
            GCurve prev = curve.PrevCurve();
            if (!prev.IsA(T.SUBCHN))
                return false;

            return (prev.Prev == null);
        }

        private bool IsTerminalCurve(GCurve curve)
        {
            GCurve next = curve.NextCurve();
            if (!next.IsA(T.SUBCHN))
                return false;

            return (next.Next == null);
        }

        private int winding;
    }
#pragma warning restore CS8600

}
