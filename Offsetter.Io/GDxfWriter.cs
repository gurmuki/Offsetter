using Offsetter.Entities;
using Offsetter.Math;
using System.Collections.Generic;
using System.IO;

namespace Offsetter.Io
{
    public class GDxfWriter
    {
        private const int RED = 1;
        private const int GREEN = 3;
        private const int BLUE = 5;
        private const int BLACK = 7;

        private const string R12 = "AC1009";
        private string acadVersion = string.Empty;

        StreamWriter fs = null!;
        GBox box = new GBox();

        public GDxfWriter()
        { }

        // Based on C:\_dev\v20_main\_fab\Java\Macros\DXFOUT.java
        public void Save(string path, DXFData dxfData, List<GChain> ichains, List<GChain> ochains)
		{
            if (File.Exists(path))
                File.Delete(path);

            // The version determines output of POLYLINE/LWPOLYLINE.
            int indx = dxfData.index["$ACADVER"];
            acadVersion = dxfData.info[indx];

            foreach (var chain in ichains)
            { box += chain.box; }

            foreach (var chain in ochains)
            { box += chain.box; }

            using (fs = new StreamWriter(path))
			{
                HeaderWrite(dxfData);

                EntitiesWrite(ichains, ochains);

                Terminate(dxfData);
            }
        }

        // private void TablesWrite()
        // NOTE: It's much easier to regurgitate the tables from the original
        // file than it is to calculate and format new values (since Autocad
        // has some peculiar ways of representing things).
        private void HeaderWrite(DXFData dxfData)
        {
            int indx, terminal;

            indx = 0;
            terminal = dxfData.index["LAYER"];
            for (; indx < terminal; ++indx)
            { fs.WriteLine(dxfData.info[indx]); }

            LayersWrite();

            indx = dxfData.index["STYLE"];
            terminal = dxfData.index["ENTITIES"];
            for (; indx < terminal; ++indx)
            { fs.WriteLine(dxfData.info[indx]); }
        }

        private void LayersWrite()
        {
            fs.WriteLine("  0");
            fs.WriteLine("TABLE");
            fs.WriteLine("  2");
            fs.WriteLine("LAYER");
            fs.WriteLine(" 70");
            fs.WriteLine("    1");  // layer visibility?

            LayerDump("UNASSIGNED", BLACK);
            LayerDump("PART", GREEN);
            LayerDump("PATH", RED);
            LayerDump("TOOL", BLUE);

            fs.WriteLine("  0");
            fs.WriteLine("ENDTAB");
        }

        private void LayerDump(string layerName, int colorNumber)
        {
            fs.WriteLine("  0");
            fs.WriteLine("LAYER");
            fs.WriteLine("  2");
            fs.WriteLine(layerName);
            fs.WriteLine(" 70");
            fs.WriteLine("    0");
            fs.WriteLine(" 62");
            fs.WriteLine(string.Format("    {0}", colorNumber));
            fs.WriteLine("  6");
            fs.WriteLine("CONTINUOUS");
        }

        private void EntitiesWrite(List<GChain> ichains, List<GChain> ochains)
        {
            fs.WriteLine("  0");
            fs.WriteLine("SECTION");

            fs.WriteLine("  2");
            fs.WriteLine("ENTITIES");

            LinesDump();

            ArcsDump();

            ChainsDump(ichains);
            ChainsDump(ochains);

            fs.WriteLine("  0");
            fs.WriteLine("ENDSEC");
        }

        private void Terminate(DXFData dxfData)
        {
            int indx, terminal;

            if (dxfData.index.ContainsKey("OBJECTS"))
            {
                indx = dxfData.index["OBJECTS"];
                terminal = dxfData.index["EOF"];
                for (; indx < terminal; ++indx)
                { fs.WriteLine(dxfData.info[indx]); }
            }

            indx = dxfData.index["EOF"];
            terminal = indx + 2;
            for (; indx < terminal; ++indx)
            { fs.WriteLine(dxfData.info[indx]); }
        }

        // As ACAD polyline or circle, depending on chain contents.
        private void ChainsDump(List<GChain> chains)
		{
			// Loop over chains.
            for (int cndx = 0; cndx < chains.Count; ++cndx)
			{
                GChain chain = chains[cndx];
                string layerName = LayerName(chain.layer);

                GCurve curve = chain.FirstCurve();
                if (curve.IsA(T.ARC) && ((GArc)curve).IsCircle)
                    CircleDump((GArc)curve, layerName);
                else if (acadVersion == R12)
                    PolylineDump(chain, layerName);
                else
                    LWPolylineDump(chain, layerName);
            }
        }

        private void PolylineDump(GChain chain, string layerName)
        {
            fs.WriteLine("  0");
            fs.WriteLine("POLYLINE");

            fs.WriteLine("  8");
            fs.WriteLine(layerName);

            fs.WriteLine("  6");  // line type
            fs.WriteLine("BYLAYER");

            fs.WriteLine(" 62");
            fs.WriteLine(256);

            fs.WriteLine(" 66");  // "entities follow" flag
            fs.WriteLine("1");

            fs.WriteLine(" 10");
            fs.WriteLine("0");

            fs.WriteLine(" 20");
            fs.WriteLine("0");

            fs.WriteLine(" 30");
            fs.WriteLine("0");

            fs.WriteLine(" 70");
            fs.WriteLine(string.Format("    {0}", (chain.IsClosed ? "1" : "0")));

            GCurveIterator iter = chain.CurveIterator();
            while (true)
            {
                GCurve curve = iter.Curve();
                if (curve == null)
                    break;

                GCurve next = iter.Next();

                if (!curve.IsA(T.SUBCHN))
                {
                    fs.WriteLine("  0");
                    fs.WriteLine("VERTEX");

                    fs.WriteLine("  8");
                    fs.WriteLine(layerName);

                    fs.WriteLine("  6");  // line type
                    fs.WriteLine("BYLAYER");

                    fs.WriteLine(" 62");  // color number
                    fs.WriteLine("  256");

                    fs.WriteLine(" 10");
                    fs.WriteLine(curve.ps.x);

                    fs.WriteLine(" 20");
                    fs.WriteLine(curve.ps.y);

                    fs.WriteLine(" 30");
                    fs.WriteLine(0);

                    if (curve.IsA(T.ARC))
                    {
                        GArc arc = (GArc)curve;

                        fs.WriteLine(" 42");  // bulge
                        fs.WriteLine(System.Math.Tan((arc.IncludedAngle() / 4) * arc.dir));
                    }
                }

                curve = next;
            }

            if (!chain.IsClosed)
            {
                GCurve curve = chain.TerminalCurve();

                fs.WriteLine("  0");
                fs.WriteLine("VERTEX");

                fs.WriteLine("  8");
                fs.WriteLine(layerName);

                fs.WriteLine("  6");  // line type
                fs.WriteLine("BYLAYER");

                fs.WriteLine(" 62");  // color number
                fs.WriteLine("  256");

                fs.WriteLine(" 10");
                fs.WriteLine(curve.pe.x);

                fs.WriteLine(" 20");
                fs.WriteLine(curve.pe.y);

                fs.WriteLine(" 30");
                fs.WriteLine(0);
            }

            fs.WriteLine("  0");
            fs.WriteLine("SEQEND");
        }

        private void LWPolylineDump(GChain chain, string layerName)
        {
            fs.WriteLine("  0");
            fs.WriteLine("LWPOLYLINE");

            fs.WriteLine("  8");
            fs.WriteLine(layerName);

            fs.WriteLine("  6");  // line type
            fs.WriteLine("ByLayer");

            fs.WriteLine(" 62");
            fs.WriteLine(256);

            fs.WriteLine("370");
            fs.WriteLine("   -1");

            fs.WriteLine(" 70");
            fs.WriteLine(string.Format("    {0}", (chain.IsClosed ? "1" : "0")));

            fs.WriteLine(" 43");
            fs.WriteLine("0");

            GCurveIterator iter = chain.CurveIterator();
            while (true)
            {
                GCurve curve = iter.Curve();
                if (curve == null)
                    break;

                GCurve next = iter.Next();

                if (!curve.IsA(T.SUBCHN))
                {
                    fs.WriteLine(" 10");
                    fs.WriteLine(curve.ps.x);

                    fs.WriteLine(" 20");
                    fs.WriteLine(curve.ps.y);

                    if (curve.IsA(T.ARC))
                    {
                        GArc arc = (GArc)curve;

                        fs.WriteLine(" 42");  // bulge
                        fs.WriteLine(System.Math.Tan((arc.IncludedAngle() / 4) * arc.dir));
                    }
                }

                curve = next;
            }

            fs.WriteLine("  0");
            fs.WriteLine("SEQEND");
        }

        private void CircleDump(GArc circle, string layerName)
        {
            fs.WriteLine("  0");
            fs.WriteLine("CIRCLE");

            fs.WriteLine("  8");
            fs.WriteLine(layerName);

            fs.WriteLine("  6");  // line type
            fs.WriteLine("BYLAYER");

            fs.WriteLine(" 62");  // color number
            fs.WriteLine("256");

            fs.WriteLine(" 10");
            fs.WriteLine(circle.pc.x);

            fs.WriteLine(" 20");
            fs.WriteLine(circle.pc.y);

            fs.WriteLine(" 40");
            fs.WriteLine(circle.rad);
        }

        private void LinesDump()
		{
#if false
			// Loop over the lines.
			{
			    LineDump(layerName, line.ps, line.pe);
			}
#endif
        }

        private void LineDump(string layerName, GPoint ps, GPoint pe)
		{
			fs.WriteLine("  0");
			fs.WriteLine("LINE");

			fs.WriteLine("  8");
			fs.WriteLine(layerName);

			fs.WriteLine(" 10");
			fs.WriteLine(ps.x);

			fs.WriteLine(" 20");
			fs.WriteLine(ps.y);

			fs.WriteLine(" 30");
			fs.WriteLine("0");

			fs.WriteLine(" 11");
			fs.WriteLine(pe.x);

			fs.WriteLine(" 21");
			fs.WriteLine(pe.y);

			fs.WriteLine(" 31");
			fs.WriteLine(0);
		}

		// CLEARLY not tested
        private void ArcsDump()
        {
#if false
			// Loop over the lines.
			{
				GArc arc;
                fs.WriteLine("  0");
                fs.WriteLine("ARC");

                fs.WriteLine("  8");
                fs.WriteLine(layerName);

                fs.WriteLine(" 10");
                fs.WriteLine(arc.pc.x);

                fs.WriteLine(" 20");
                fs.WriteLine(arc.pc.y);

                fs.WriteLine(" 30");
                fs.WriteLine(0);

                fs.WriteLine(" 40");
				fs.WriteLine(Math.Abs(arc.rad));

				double sa = arc.sa * GConst.RAD2DEG;
				double ea = arc.ea * GConst.RAD2DEG;

                fs.WriteLine(" 50");
                fs.WriteLine(((arc.dir == GConst.CCW) ? sa : ea));

                fs.WriteLine(" 51");
                fs.WriteLine(((arc.dir == GConst.CCW) ? ea : sa));
            }
#endif
        }

        // NOTE: Layer colors are defined by the CAD system and so the
        // geometry color depends on its associated layer (BYLAYER).
        private string LayerName(Layer layer)
        {
            if (layer == Layer.PART)
                return "PART";

            if (layer == Layer.PATH)
                return "PATH";

            if (layer == Layer.TOOL)
                return "TOOL";

            return "UNASSIGNED";
        }
    }
}
