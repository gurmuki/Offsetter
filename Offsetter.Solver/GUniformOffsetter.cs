using Offsetter.Entities;
using Offsetter.Math;
using System;
using System.Collections.Generic;

namespace Offsetter.Solver
{
    using GChainList = List<GChain>;

    public class GUniformOffsetter
    {
        public GUniformOffsetter() { }

        public void Offset(GChain? ichain, GChainType chainType, int side, double dist, GChainList results)
        {
            if ((ichain == null) || ichain.IsEmpty)
                return;

            GChain ochain = OffsetCore(ichain, side, dist);
            if (ochain == null)
                return;

            // ASSUMPTION: chains have CCW winding direction; this affects the
            // iindexes depending on whether offsetting to inside or outside.
            if (side < 0)
                ochain.Reverse();

            GDegouger degouger = new GDegouger((side == GConst.LEFT) ? GConst.CCW : GConst.CW);
            degouger.Degouge(ochain, results);

            foreach (GChain chain in results)
            { chain.ChainType = chainType; }
        }

        public void Offset(GChainList ichains, int side, double dist, GChainList results)
        {
            GChainList pockets = new GChainList();
            GChainList islands = new GChainList();
            for (int indx = 0; indx < ichains.Count; ++indx)
            {
                GChain ichain = ichains[indx];
                if (indx == 0)
                {
                    Offset(ichain, GChainType.POCKET, side, dist, pockets);
                }
                else
                {
                    // Ignore any island wholly external to the pocket.
                    if (ichain.box.Overlaps(ichains[0].box))
                        Offset(ichain, GChainType.ISLAND, -side, dist, islands);
                }
            }

            GChainList rislands;
            IslandsResolve(islands, out rislands);

            bool debugging = false;
            if (debugging)
            {
                ChainMove(pockets, results);
                ChainMove(rislands, results);
            }
            else
            {
                GChainList temp;
                Resolve(pockets, rislands, out temp);
                ChainMove(temp, results);
            }
        }

        private void Resolve(GChainList pockets, GChainList islands, out GChainList results)
        {
            GDegouger degouger = new GDegouger(GConst.CCW);

            GChainList input = new GChainList();
            ChainMove(pockets, input);
            ChainMove(islands, input);

            degouger.ChainsDegouge(input, out results);

            // Sort the results on bounding box (smallest to largest).
            // From https://stackoverflow.com/questions/3309188/how-to-sort-a-listt-by-a-property-in-the-object
            results.Sort((x, y) => Nullable.Compare<double>(System.Math.Abs(x.area), System.Math.Abs(y.area)));

            // We can discard any island at the end of results because (being the chain having the greatest area)
            // it will likely be outside any pocket. However, it is possible for an offset distance to be large
            // enough the island will contain the pocket (e.g. a004.dxf - L20).
            int indx = results.Count - 1;
            while (indx >= 0)
            {
                if (results[indx].ChainType != GChainType.ISLAND)
                    break;

                GChain island = results[indx];
                results.RemoveAt(indx);

                // Discard any pocket contained by this island.
                int jndx = results.Count - 1;
                while (jndx >= 0)
                {
                    // No island should ever be contained within another island.
                    GLogger.Assert((results[jndx].ChainType != GChainType.ISLAND), "Resolve(island in island)");
                    if (island.Contains(results[jndx].ps, GContainment.OUTSIDE, true) == GContainment.INSIDE)
                        results.RemoveAt(jndx);

                    --jndx;
                }

                indx = results.Count - 1;
            }

            for (indx = 0; indx < results.Count; ++indx)
            {
                GLogger.Assert((results[indx].ChainType != GChainType.UNKNOWN), "Resolve(chain not Typed)");
            }
        }

        private void IslandsResolve(GChainList islands, out GChainList results)
        {
            GDegouger degouger = new GDegouger(GConst.CW);
            degouger.ChainsDegouge(islands, out results);

            // Sort the results on bounding box (smallest to largest).
            // From https://stackoverflow.com/questions/3309188/how-to-sort-a-listt-by-a-property-in-the-object
            results.Sort((x,y) => Nullable.Compare<double>(x.box.area, y.box.area));

            // Identify as pocket/island.
            for (int indx = 0; indx < results.Count; ++indx)
            {
                bool contained = false;

                for (int jndx = indx + 1; jndx < results.Count; ++jndx)
                {
                    if (results[indx].box.Overlaps(results[jndx].box))
                    {
                        if (results[jndx].Contains(results[indx].ps, GContainment.INSIDE, true) == GContainment.INSIDE)
                        {
                            contained = true;
                            break;
                        }
                    }
                }

                results[indx].ChainType = (contained ? GChainType.POCKET : GChainType.ISLAND);
            }
        }

        private void ChainMove(GChainList chains, GChainList results)
        {
            foreach (var chain in chains)
            { results.Add(chain); }
        }

        private GChain OffsetCore(GChain ichain, int side, double dist)
        {
            GChain ochain = new GChain(GChainType.UNKNOWN);

            GCurveIterator iter = ichain.CurveIterator();
            while (true)
            {
                GCurve icurve = iter.Curve();
                if (icurve == null)
                    break;

                iter.Next();

                GCurve? ocurve = Offset(icurve, side, dist);
                if (ocurve == null)
                    continue;

                if (!ochain.IsEmpty && (ochain.pe != ocurve.ps))
                    Blend(ref ochain, ocurve, icurve.ps, side);

                ochain.Append(ocurve);
            }

            if (ichain.IsClosed && (ochain.pe != ochain.ps))
                Blend(ref ochain, ochain.FirstCurve(), ichain.pe, side);

            return ochain;
        }

        // ochain - offset chain, post - offset pre to be appended, icurve - input reference pre
        private void Blend(ref GChain ochain, GCurve post, GPoint pc, int side)
        {
            GCurve pre = ochain.TerminalCurve();
            if (pre == null)
                return;

            GVec vecA = pre.TangentAt(1);
            GVec vecB = post.TangentAt(0);

            double cross = vecA ^ vecB;
            if (System.Math.Abs(cross) >= GConst.VECTOR_SMALL)
            {
                GArc blend = GArc.ArcCreate(pre.pe, post.ps, pc, System.Math.Sign(cross));
                if (blend != null)
                {
                    blend.IsBlend = ((cross * side) > 0);
                    ochain.Append(blend);
                }
            }
        }

        private GCurve? Offset(GCurve? icurve, int side, double dist)
        {
            if (icurve == null)
                return null;

            GCurve? ocurve = null;
#pragma warning disable CS8604  // Possible null reference argument 'uvec, svec, evec'
            if (icurve.IsA(T.LINE))
            {
                GVec uvec = icurve.TangentAt(0) + (GConst.HALF_PI * side);
                GPoint ps = icurve.ps + (uvec * dist);
                GPoint pe = icurve.pe + (uvec * dist);
                ocurve = GLine.LineCreate(ps, pe);
            }
            else if (icurve.IsA(T.ARC))
            {
                GArc arc = (GArc)icurve;

                if (arc.IsCircle)
                {
                    if ((arc.dir * side) > 0)
                        dist = -dist;

                    ocurve = GArc.CircleCreate(arc.pc, arc.rad + dist, arc.dir);
                }
                else
                {
                    GVec svec = new GVec(arc.sa);
                    GVec evec = new GVec(arc.ea);
                    if ((side * arc.dir) > 0)
                    {
                        svec += GConst.PI;
                        evec += GConst.PI;
                    }

                    GPoint ps = arc.ps + (svec * dist);
                    GPoint pe = arc.pe + (evec * dist);
                    ocurve = GArc.ArcCreate(ps, pe, arc.pc, arc.dir);

                    if ((ocurve != null) && (arc.dir * side > 0) && (dist > arc.rad))
                        ((GArc)ocurve).IsBlend = true;
                }
            }
#pragma warning restore CS8604

            return ocurve;
        }
    }
}