using Offsetter.Entities;
using Offsetter.Io;
using Offsetter.Math;
using Offsetter.Solver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Offsetter
{
    using GChainList = List<GChain>;

    public partial class Offsetter : Form
    {
        private void DxfOpen(string dxfPath)
        {
            string knownPath = dxfPath;

            if (knownPath == null)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    string path = Properties.Settings.Default.inputPath;
                    if (path == string.Empty)
                        path = @"c:\";

                    if (Path.GetExtension(path) == ".dxf")
                        dialog.InitialDirectory = Path.GetDirectoryName(path);
                    else
                        dialog.InitialDirectory = path;

                    dialog.Filter = "dxf files (*.dxf)|*.dxf";
                    dialog.AddExtension = true;

                    dialog.FileName = Path.GetFileName(path);
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Properties.Settings.Default.inputPath = dialog.FileName;
                        Properties.Settings.Default.Save();
                        knownPath = dialog.FileName;
                    }
                }
            }

            if (knownPath != null)
            {
                DxfRead(knownPath);
                ViewsClear();
                ViewBase();
                Render();
            }
        }

        private void DxfSave(string dxfPath)
        {
            string knownPath = dxfPath;

            if (knownPath == null)
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    if (Properties.Settings.Default.outputPath == string.Empty)
                        Properties.Settings.Default.outputPath = Properties.Settings.Default.inputPath;

                    dialog.InitialDirectory = Properties.Settings.Default.outputPath;
                    dialog.Filter = "dxf files (*.dxf)|*.dxf";
                    dialog.AddExtension = true;

                    if (dialog.ShowDialog() == DialogResult.OK)
                        knownPath = dialog.FileName;
                }
            }

            if (knownPath != null)
            {
                GDxfWriter writer = new GDxfWriter();
                writer.Save(knownPath, dxfReader.dxfData, ichains, ochains);
            }
        }

        private void ResultSave()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            int indx = path.IndexOf(@"\bin\");
            if (indx > 0)
            {
                path = path.Substring(0, indx);
                indx = path.LastIndexOf(@"\");
                if (indx > 0)
                {
                    path = path.Substring(0, indx) + @"\data\_results.dxf";
                    DxfSave(path);
                }
            }
        }

        private void DxfRead(string path)
        {
            this.Text = "Offsetter";

            // MenusEnable(false);

            boxMap.Clear();
            RendererMapClear();

            ichains.Clear();
            ochains.Clear();
            tchains.Clear();

            modelBox = new GBox();

            GChainList chains = new GChainList();
            dxfReader = new GDxfReader();
            if (dxfReader.Read(path, chains))
            {
                foreach (var chain in chains)
                { modelBox += chain.box; }

                double delta = ViewHalfDelta(modelBox);
                double chordalTol = ViewChordTol();

                foreach (var chain in chains)
                { Collate(chain, chordalTol); }

                double xc = modelBox.xc;
                double yc = modelBox.yc;
                double dx = modelBox.dx / 2;
                double dy = modelBox.dy / 2;

                // Ensure modelBox is square.
                delta = ((dx > dy) ? dx : dy);
                modelBox = new GBox(
                    xc - delta, yc - delta,
                    xc + delta, yc + delta);

                // Resize so everything fits nicely in the window.
                modelBox = modelBox.Resize(1.05);

                selectedCurve = null!;
                ViewBase();

                MenusItemsEnable(true);

                this.Text = string.Format("Offsetter ({0})", Path.GetFileName(path));
            }
        }

        private void UniformOffset()
        {
            OffsetDialog dialog = new OffsetDialog(geoMenuLocation, "Uniform Offset", offsetSide, offsetDist);
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            offsetSide = dialog.OffsetSide;
            offsetDist = dialog.OffsetDist;

            GChainList results = new GChainList();
            GUniformOffsetter offsetter = new GUniformOffsetter();
            offsetter.Offset(ichains, offsetSide, offsetDist, results);
            if (results.Count <= 0)
                return;

            ResultsCollate(results);
            Render();

            ToolingMenuItemEnable(true);
        }

        private void NonUniformOffset()
        {
            OffsetDialog dialog = new OffsetDialog(geoMenuLocation, "Non-uniform Offset", offsetSide);
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            offsetSide = dialog.OffsetSide;

            GChainList results = new GChainList();
            GNonUniformOffseter ch = new GNonUniformOffseter(false);
            ch.Offset(Part, Tool, offsetSide, 0, results);
            if (results.Count <= 0)
                return;

            ResultsCollate(results);
            Render();

            ToolingMenuItemEnable(true);
        }

        private void Nest()
        {
            OffsetDialog dialog = new OffsetDialog(geoMenuLocation, "Nest", offsetSide);
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            offsetSide = dialog.OffsetSide;

            GChainList results = new GChainList();
            GNonUniformOffseter ch = new GNonUniformOffseter(true);
            ch.Offset(Part, Tool, offsetSide, 0, results);
            if (results.Count <= 0)
                return;

            ResultsCollate(results);
            Render();

            ToolingMenuItemEnable(false);
        }

        private void ResultsCollate(GChainList chains)
        {
            GBox tempA = modelBox.Resize(0.95);
            GBox tempB = new GBox(tempA);

            for (int indx = 0; indx < chains.Count; ++indx)
            {
                GChain chain = chains[indx];
                if (chain.ChainType == GChainType.PART)
                    chain.layer = Layer.PART;
                else
                    tempB += chain.box;
            }

            if ((tempB.dx != tempA.dx) || (tempB.dy != tempA.dy))
                modelBox = tempB.Resize(1.05);  // The new chains extended the bounding box.

            double chordalTol = ViewChordTol();
            for (int indx = 0; indx < chains.Count; ++indx)
            {
                GChain chain = chains[indx];
                Collate(chain, chordalTol);
            }
        }

        private void Decompose()
        {
            GDecomposer decomposer = new GDecomposer();
            decomposer.Decompose(ichains[0], ochains);

            for (int indx = 0; indx < ochains.Count; ++indx)
            { modelBox += ochains[indx].box; }

            ToolingMenuItemEnable(ochains.Count > 0);
        }

        private void Tooling()
        {
            toolingEnabled = !toolingEnabled;
            if (toolingEnabled)
                ToolingAdd(offsetDist, ochains);
            else
                tchains.Clear();
        }

        private void ToolingAdd(double dist, GChainList chains)
        {
            int nchains = chains.Count;

            for (int indx = 0; indx < nchains; ++indx)
            {
                GChain ichain = chains[indx];

                GCurve curve = ichain.FirstCurve();
                while (true)
                {
                    if (curve == null)
                        break;

                    if (!curve.IsA(T.SUBCHN))
                    {
                        if (curve == ichain.FirstCurve())
                            tchains.Add(ToolCreate(curve, dist, 0));

                        if (curve.IsA(T.ARC) && ((GArc)curve).IsCircle)
                            tchains.Add(ToolCreate(curve, dist, 0.25));

                        tchains.Add(ToolCreate(curve, dist, 0.5));

                        if (curve.IsA(T.ARC) && ((GArc)curve).IsCircle)
                            tchains.Add(ToolCreate(curve, dist, 0.75));

                        tchains.Add(ToolCreate(curve, dist, 1));
                    }

                    curve = curve.NextCurve();
                }
            }
        }

        private GChain ToolCreate(GCurve curve, double radius, double u)
        {
            GChain tool = new GChain(GChainType.TOOL);
            tool.Append(GArc.CircleCreate(curve.PointAtUparam(u), radius, GConst.CW));
            return tool;
        }

        private void Reorder()
        {
            int seedCurveId = 0;
            if (seedCurveId > 0)
                ichains[0].Reorder(seedCurveId);
        }

        private void ChainTabulate(GChain chain, double chordalTol)
        {
            // TODO: A better way to do this? I'm not real happy exposing chain traversal.
            GCurve curve = chain.FirstCurve();
            while (curve != null)
            {
                if (!curve.IsA(T.SUBCHN))
                {
                    GBox box = curve.box;
                    modelBox += box;
                    boxMap[curve] = box;

                    VertexList verts = new VertexList();
                    curve.Tabulate(verts, chordalTol);
                    rendererMap[curve] = new WireframeRenderer(wireframeShader, verts, colorMap[chain.ChainType]);
                }

                curve = curve.NextCurve();
            }
        }

        private void Collate(GChain chain, double chordalTol)
        {
            ChainTabulate(chain, chordalTol);

            switch (chain.ChainType)
            {
                case GChainType.PART:
                    chain.layer = Layer.PART;
                    ichains.Add(chain);
                    break;

                case GChainType.POCKET:
                case GChainType.ISLAND:
                case GChainType.PATH:
                    chain.layer = Layer.PATH;
                    ochains.Add(chain);
                    break;

                case GChainType.TOOL:
                    chain.layer = Layer.TOOL;
                    ochains.Add(chain);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private void CurveHilight(GCurve curve, bool hilight)
        {
            if (curve == null)
                return;

            WireframeRenderer wr = (WireframeRenderer)rendererMap[curve];
            wr.LineWidth = (hilight ? 3 : 1);
        }

        private void PropertiesClosedAction()
        {
            if (selectedCurve != null)
            {
                CurveHilight(selectedCurve, false);
                selectedCurve = null!;
                Render();
            }

            viewMode = ViewMode.Static;
            propertiesDialog = null!;

            MenusEnable(true);
        }

        private void PropertiesDialogShow()
        {
            if (propertiesDialog == null)
            {
                MenusEnable(false);

                propertiesDialog = new PropertiesDialog(geoMenuLocation, PropertiesClosedAction);
                propertiesDialog.ShowDegrees = showDegrees;
            }

            if (!propertiesDialog.Visible)
                propertiesDialog.Show(glControl);
        }

        private void PropertiesDialogUpdate(Point mouseLocation)
        {
            GCurve curve = ViewPick(mouseLocation);
            if (curve == null)
                return;

            CurveHilight(selectedCurve, false);

            selectedCurve = curve;
            CurveHilight(selectedCurve, true);
            Render();

            Point screenLocation = glControl.PointToScreen(mouseLocation);
            propertiesDialog.PropertiesUpdate(screenLocation, curve);
        }
    }
}
