using Offsetter.Entities;
using Offsetter.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Offsetter.Io
{
    using GChainList = List<GChain>;
    
    // Like duh! null returns are intentional here in these methods
    //   warning CS8600: Converting null literal or possible null value to non-nullable type
    //   warning CS8603: Possible null reference return
    //   warning CS8604: Possible null reference argument
#pragma warning disable CS8600, CS8603, CS8604

    public class GDxfReader
    {
        public readonly DXFData dxfData = new DXFData();

        public GDxfReader()
        {
        }

        public bool Read(string path, GChainList chains)
        {
            bool success = false;

            chains.Clear();

            if (FileRead(path))
            {
                success = EntitiesProcess(chains); 
            }

            return success;
        }

        // Data blocks have the form
        //       0
        //     <block type> [eg. where <block type> is SECTION]
        //       2
        //     <block name> [eg. where <block name> is ENTITIES]
        //       0
        private bool FileRead(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            dxfData.info = File.ReadAllLines(filePath);

            bool vport = false;

            int indx = 1;  // skip the initial entity code 999
            while (indx < dxfData.info.Length)
            {
                string name = dxfData.info[indx].ToUpper();
                if (name == "$ACADVER")
                {
                    indx += 2;
                    dxfData.index[name] = indx;
                }
                else if (name == "SECTION")
                {
                    indx += 2;
                    name = dxfData.info[indx].ToUpper();
                    dxfData.index[name] = indx - 3;
                }
                else if (name == "TABLE")
                {
                    indx += 2;
                    name = dxfData.info[indx].ToUpper();
                    vport = (name == "VPORT");

                    if ((name == "LAYER") || (name == "STYLE"))
                        dxfData.index[name] = indx - 3;
                }
                else if (name == "$GRIDMODE")
                {
                    // Disable grid-related data.
                    dxfData.info[indx + 2] = "    0";
                }
                else if (name == "EOF")
                {
                    dxfData.index[name] = indx - 1;
                }

                if (vport && (name == " 76"))
                {
                    // Disable grid-related data.
                    dxfData.info[indx + 1] = "    0";
                }

                ++indx;  // skip the entity code
            }

            return true;
        }

        private bool EntitiesProcess(GChainList chains)
        {
            bool success = false;

            GEntity.EntityIdsReset();

            int indx = dxfData.index["ENTITIES"] + 1;

            GChain chain = null;
            while (true)
            {
                GEntity entity = NextEntity(ref indx);
                if (entity == null)
                    break;

                if (entity.IsA(T.CHAIN))
                {
                    // NOTE: Obtained by converting a polyline.
                    GChain existing = (GChain)entity;
                    if (existing.Winding == GConst.CW)
                        existing.Reverse();

                    chains.Add(existing);
                }
                else
                {
                    if (chain == null)
                        chain = new GChain(GChainType.PART);

                    GCurve curve = (GCurve)entity;
                    if (!chain.Grow(curve))
                    {
                        // 'entity' is not adjacent to either end of the chain, so ....
                        if (chain.Winding == GConst.CW)
                            chain.Reverse();

                        chains.Add(chain);

                        // Being a new chain.
                        chain = new GChain(GChainType.PART);
                        chain.Append(curve);
                    }
                }
            }

            if (chain != null)
            {
                if (chain.Winding == GConst.CW)
                    chain.Reverse();

                chains.Add(chain);
            }

            success = true;

            return success;
        }

        private GEntity NextEntity(ref int indx)
        {
            GEntity entity = null;

            while (indx < dxfData.info.Length)
            {
                if (dxfData.info[indx] == "ENDSEC")
                    break;

                if (dxfData.info[indx].Equals("LINE", StringComparison.OrdinalIgnoreCase))
                    entity = LineParse(ref indx);
                else if (dxfData.info[indx].Equals("ARC", StringComparison.OrdinalIgnoreCase))
                    entity = ArcParse(ref indx);
                else if (dxfData.info[indx].Equals("CIRCLE", StringComparison.OrdinalIgnoreCase))
                    entity = CircleParse(ref indx);
                else if (dxfData.info[indx].Equals("POLYLINE", StringComparison.OrdinalIgnoreCase))
                    entity = PolyParse(ref indx);
                else if (dxfData.info[indx].Equals("LWPOLYLINE", StringComparison.OrdinalIgnoreCase))
                    entity = LWPolyParse(ref indx);

                if (entity != null)
                    break;

                ++indx;
            }

            return entity;
        }

        // Entries in the DXF file alternate between CODE & VALUE (in that order).
        private GLine LineParse(ref int indx)
        {
            if (!dxfData.info[indx].Equals("LINE", StringComparison.OrdinalIgnoreCase))
                return null;

            double xs = GConst.UNDEFINED;
            double ys = GConst.UNDEFINED;
            double xe = GConst.UNDEFINED;
            double ye = GConst.UNDEFINED;

            while (indx < dxfData.info.Length)
            {
                ++indx;
                int code = EntityCode(indx);
                if (code == 0)
                    break;  // End of line definition

                ++indx;
                if (code == 10)
                    xs = DoubleParse(indx);
                else if (code == 20)
                    ys = DoubleParse(indx);
                else if (code == 11)
                    xe = DoubleParse(indx);
                else if (code == 21)
                    ye = DoubleParse(indx);
            }

            GLine line = null;
            if ((xs != GConst.UNDEFINED) && (ys != GConst.UNDEFINED) && (xe != GConst.UNDEFINED) && (ye != GConst.UNDEFINED))
                line = GLine.LineCreate(new GPoint(xs, ys), new GPoint(xe, ye));

            return line;
        }

        // Entries in the DXF file alternate between CODE & VALUE (in that order).
        // In R12 DXF, all arcs are CCW, so there is no way to know the actual
        // winding direction without having any context (eg. adjacent curves).
        //
        //          90
        //           |
        //           |
        //   180 ----|---- 0
        //           |
        //           |
        //          270
        //
        private GArc ArcParse(ref int indx)
        {
            if (!dxfData.info[indx].Equals("ARC", StringComparison.OrdinalIgnoreCase))
                return null;

            double xc = GConst.UNDEFINED;
            double yc = GConst.UNDEFINED;
            double rad = GConst.UNDEFINED;
            double sa = GConst.UNDEFINED;
            double ea = GConst.UNDEFINED;

            while (indx < dxfData.info.Length)
            {
                ++indx;
                int code = EntityCode(indx);
                if (code == 0)
                    break;  // End of arc definition

                ++indx;
                if (code == 10)
                    xc = DoubleParse(indx);
                else if (code == 20)
                    yc = DoubleParse(indx);
                else if (code == 40)
                    rad = DoubleParse(indx);
                else if (code == 50)
                    sa = DoubleParse(indx);
                else if (code == 51)
                    ea = DoubleParse(indx);
            }

            GArc arc = null;
            if ((xc != GConst.UNDEFINED) && (yc != GConst.UNDEFINED) && (rad != GConst.UNDEFINED) && (sa != GConst.UNDEFINED) && (ea != GConst.UNDEFINED))
            {
                double sRadians = sa * GConst.DEG2RAD;
                double eRadians = ea * GConst.DEG2RAD;
                if (eRadians < sRadians)
                    eRadians += GConst.TWO_PI;  // e.g. sa = 270 & ea = 0

                double xs = xc + (rad * System.Math.Cos(sRadians));
                double ys = yc + (rad * System.Math.Sin(sRadians));
                double xe = xc + (rad * System.Math.Cos(eRadians));
                double ye = yc + (rad * System.Math.Sin(eRadians));

                arc = GArc.ArcCreate(new GPoint(xs, ys), new GPoint(xe, ye), new GPoint(xc, yc), GConst.CCW);
            }

            return arc;
        }

        private GChain CircleParse(ref int indx)
        {
            if (!dxfData.info[indx].Equals("CIRCLE", StringComparison.OrdinalIgnoreCase))
                return null;

            double xc = GConst.UNDEFINED;
            double yc = GConst.UNDEFINED;
            double rad = GConst.UNDEFINED;

            while (indx < dxfData.info.Length)
            {
                ++indx;
                int code = EntityCode(indx);
                if (code == 0)
                    break;  // End of arc definition

                ++indx;
                if (code == 10)
                    xc = DoubleParse(indx);
                else if (code == 20)
                    yc = DoubleParse(indx);
                else if (code == 40)
                    rad = DoubleParse(indx);
            }

            GChain chain = null;
            if ((xc != GConst.UNDEFINED) && (yc != GConst.UNDEFINED) && (rad != GConst.UNDEFINED))
            {
                GArc arc = GArc.CircleCreate(new GPoint(xc, yc), rad, GConst.CCW);
                if (arc != null)
                {
                    chain = new GChain(GChainType.PART);
                    chain.Append(arc);
                }
            }

            return chain;
        }

        // Entries in the DXF file alternate between CODE & VALUE (in that order).
        private GChain PolyParse(ref int indx)
        {
            if (!dxfData.info[indx].Equals("POLYLINE", StringComparison.OrdinalIgnoreCase))
                return null;

            // Find the first vertex.
            bool isClosed = false;
            while (indx < dxfData.info.Length)
            {
                ++indx;
                int code = EntityCode(indx);
                if ((code == 0) && dxfData.info[indx + 1].Equals("VERTEX", StringComparison.OrdinalIgnoreCase))
                    break;

                ++indx;
                if (code == 70)
                    isClosed = ((EntityCode(indx) & 1) == 1);
            }

            --indx;  // Back up to code = 0

            Queue<GPoint> pts = new Queue<GPoint>();
            Queue<double> bulges = new Queue<double>();
            GPoint pt = null;
            double bulge = 0;

            GChain chain = new GChain(GChainType.PART);
            while (indx < dxfData.info.Length)
            {
                ++indx;
                int code = EntityCode(indx);

                ++indx;
                if (code == 0)
                {
                    if (dxfData.info[indx].Equals("SEQEND", StringComparison.OrdinalIgnoreCase))
                    {
                        // At end of polyline definition.
                        if (isClosed && (pt != null!))
                        {
                            pts.Enqueue(pt);
                            bulges.Enqueue(bulge);

                            if (pts.Count == 2)
                            {
                                // Create curves as necessary.
                                GPoint ps = pts.Dequeue();
                                GPoint pe = pts.Peek();

                                bulge = bulges.Dequeue();
                                if (bulge == 0)
                                    chain.Append(GLine.LineCreate(ps, pe));
                                else
                                    chain.Append(GArc.ArcCreate(ps, pe, System.Math.Atan(bulge) * 4));

                                ps = pts.Dequeue();
                                bulge = bulges.Dequeue();
                                if (ps != chain.ps)
                                {
                                    if (bulge == 0)
                                        chain.Append(GLine.LineCreate(ps, chain.ps));
                                    else
                                        chain.Append(GArc.ArcCreate(ps, chain.ps, System.Math.Atan(bulge) * 4));
                                }
                            }
                        }
                        break;
                    }
                    else if (dxfData.info[indx].Equals("VERTEX", StringComparison.OrdinalIgnoreCase))
                    {
                        if (pt != null!)
                        {
                            pts.Enqueue(pt);
                            bulges.Enqueue(bulge);
                        }

                        if (pts.Count == 2)
                        {
                            // Create curves as necessary.
                            GPoint ps = pts.Dequeue();
                            GPoint pe = pts.Peek();

                            double angle = System.Math.Atan(bulges.Dequeue()) * 4;
                            if (angle == 0)
                                chain.Append(GLine.LineCreate(ps, pe));
                            else
                                chain.Append(GArc.ArcCreate(ps, pe, angle));
                        }

                        pt = new GPoint(GConst.UNDEFINED, GConst.UNDEFINED);
                        bulge = 0;
                    }
                }
                else
                {
                    if ((code == 10) && (pt != null!))
                        pt.x = DoubleParse(indx);
                    else if ((code == 20) && (pt != null!))
                        pt.y = DoubleParse(indx);
                    else if (code == 42)
                        bulge = BulgeParse(indx);
                }
            }

            return chain;
        }

        private GChain LWPolyParse(ref int indx)
        {
            if (!dxfData.info[indx].Equals("LWPOLYLINE", StringComparison.OrdinalIgnoreCase))
                return null;

            bool isClosed = false;

            // Find the start of the first vertex.
            ++indx;
            while (indx < dxfData.info.Length)
            {
                int code = EntityCode(indx);
                if (code == 10)
                    break;

                if (code == 70)
                {
                    code = EntityCode(indx + 1);
                    isClosed = ((code & 1) == 1);
                }

                indx += 2;
            }

            // --indx;  // Back up to code = 0

            Queue<GPoint> pts = new Queue<GPoint>();
            Queue<double> bulges = new Queue<double>();
            double x = Double.MinValue;
            double y = Double.MinValue;
            double bulge = 0;

            GChain chain = new GChain(GChainType.PART);

            while (indx < dxfData.info.Length)
            {
                int code = EntityCode(indx);

                if (code == 0)
                {
                    // At end of polyline definition (eg. ENDSEC or start of next entity).
                    if (isClosed)
                    {
                        GPoint ps = new GPoint(x, y);
                        GPoint pe = chain.ps;

                        if (bulge == 0)
                            chain.Append(GLine.LineCreate(ps, pe));
                        else
                            chain.Append(GArc.ArcCreate(ps, pe, System.Math.Atan(bulge) * 4));
                    }

                    break;
                }

                if (code == 10)
                {
                    y = Double.MinValue;
                    bulge = 0;

                    x = DoubleParse(indx + 1);
                    indx += 2;

                    if (EntityCode(indx) == 20)
                    {
                        y = DoubleParse(indx + 1);
                        indx += 2;

                        if (EntityCode(indx) == 42)
                        {
                            bulge = DoubleParse(indx + 1);
                            indx += 2;
                        }

                        pts.Enqueue(new GPoint(x, y));
                        bulges.Enqueue(bulge);

                        if (pts.Count == 2)
                        {
                            // Create curves as necessary.
                            GPoint ps = pts.Dequeue();
                            GPoint pe = pts.Peek();

                            double angle = System.Math.Atan(bulges.Dequeue()) * 4;
                            if (angle == 0)
                                chain.Append(GLine.LineCreate(ps, pe));
                            else
                                chain.Append(GArc.ArcCreate(ps, pe, angle));
                        }
                    }
                }
                else
                {
                    indx += 2;
                }
            }

            return chain;
        }

        private int EntityCode(int indx)
        {
            if (!int.TryParse(dxfData.info[indx], out int code))
                code = -1;

            return code;
        }

        private double DoubleParse(int indx)
        {
            return Double.Parse(dxfData.info[indx]);
        }

        // 'bulge' is the tangent of one fourth of the included bulge, the sign indicating winding direction.
        private double BulgeParse(int indx)
        {
            return Double.Parse(dxfData.info[indx]);
        }
    }
#pragma warning restore CS8600, CS8603, CS8604

    public class DXFData
    {
        public string[] info = { };
        public Dictionary<string, int> index = new Dictionary<string, int>();

        public DXFData() { }
    }
}
