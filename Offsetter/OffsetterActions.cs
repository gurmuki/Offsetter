using Offsetter.Dialogs;
using Offsetter.Entities;
using Offsetter.Io;
using Offsetter.Math;
using Offsetter.Solver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.TextFormatting;

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

                inputMask = false;
                pathMask = false;
                intermediateMask = false;
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
                selectedCurves.Clear();

                foreach (var chain in chains)
                { modelBox += chain.box; }

                double xc = modelBox.xc;
                double yc = modelBox.yc;
                double dx = modelBox.dx / 2;
                double dy = modelBox.dy / 2;

                // Ensure modelBox is square.
                double delta = ViewHalfDelta(modelBox);
                delta = ((dx > dy) ? dx : dy);
                modelBox = new GBox(
                    xc - delta, yc - delta,
                    xc + delta, yc + delta);

                // Resize so everything fits nicely in the window.
                modelBox = modelBox.Resize(1.05);

                ViewBase();

                double chordalTol = ViewChordTol();
                foreach (var chain in chains)
                { Collate(chain, null, chordalTol); }

                MenusItemsEnable(true);

                this.Text = string.Format("Offsetter ({0})", Path.GetFileName(path));
            }
        }

        private void ResultsCollate(GChainList chains, GChain tool)
        {
            GBox tempA = modelBox.Resize(0.95);
            GBox tempB = new GBox(tempA);

            foreach (var chain in chains)
            { tempB += chain.box; }

            if ((tempB.dx != tempA.dx) || (tempB.dy != tempA.dy))
                modelBox = tempB.Resize(1.05);  // The new chains extended the bounding box.

            double chordalTol = ViewChordTol();
            foreach (var chain in chains)
            { Collate(chain, tool, chordalTol); }
        }

        private void Decompose()
        {
            GDecomposer decomposer = new GDecomposer();
            decomposer.Decompose(ichains[0], ochains);

            for (int indx = 0; indx < ochains.Count; ++indx)
            { modelBox += ochains[indx].box; }
        }

        private GChain ToolCreate(double radius)
        {
            GChain tool = new GChain(GChainType.TOOL);
            tool.Append(GArc.CircleCreate(new GPoint(0, 0), radius, GConst.CW));
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
            GChainIterator iter = new GChainIterator(chain);

            GCurve curve = iter.FirstCurve();
            while (curve != null)
            {
                GBox box = curve.box;
                modelBox += box;
                boxMap[curve] = box;

                VertexList verts = new VertexList();
                curve.Tabulate(verts, chordalTol);
                rendererMap[curve] = new WireframeRenderer(wireframeShader, verts, colorMap[chain.ChainType]);

                curve = iter.NextCurve();
            }
        }

        private void Collate(GChain chain, GChain? tool, double chordalTol)
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

                    if (tool != null)
                        tchains[chain] = tool;

                    break;

                case GChainType.TOOL:
                    chain.layer = Layer.TOOL;
                    ochains.Add(chain);
                    break;

                case GChainType.INTERMEDIATE:
                    chain.layer = Layer.INTERMEDIATE;
                    ochains.Add(chain);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private void UniformDialogAction(object? sender, EventArgs e)
        {
            UniformOffsetDialog dialog = (UniformOffsetDialog)modelessDialog;

            offsetSide = dialog.OffsetSide;
            offsetDist = dialog.OffsetDist;

            GChainList results = new GChainList();
            GUniformOffsetter offsetter = new GUniformOffsetter();
            offsetter.Offset(dialog.Chains, offsetSide, offsetDist, results);
            if (results.Count <= 0)
                return;

            GChain tool = ToolCreate(offsetDist);

            ResultsCollate(results, tool);
            Render();
        }

        private void NonUniformDialogAction(object? sender, EventArgs e)
        {
            NonUniformOffsetDialog dialog = (NonUniformOffsetDialog)modelessDialog;
            bool nesting = (dialog.Text == NESTING);

            offsetSide = dialog.OffsetSide;

            GChainList results = new GChainList();
            GNonUniformOffseter ch = new GNonUniformOffseter(nesting);
            ch.Offset(dialog.Shape, dialog.Tool, offsetSide, 0, results);
            if (results.Count <= 0)
                return;

            ResultsCollate(results, dialog.Tool);
            Render();
        }

        private async void AnimateDialogAction(object? sender, GChain path)
        {
            if (path == null)
            {
                // The AnimateDialog "Stop" button was clicked.
                cancelTokenSource.Cancel();

                ToolRendererDispose();
            }
            else
            {
                ToolRendererCreate(path);

                // The AnimateDialog "Start" button was clicked.
                cancelTokenSource = new CancellationTokenSource();
                cancelToken = cancelTokenSource.Token;
                var task = Task.Run(() =>
                {
                    Animate(path, cancelToken);
                }, cancelToken);

                try
                {
                    await task;

                    ToolRendererDispose();
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine($"AnimateDialogAction: {nameof(OperationCanceledException)} thrown\n");
                }
            }
        }

        private void Animate(GChain path, CancellationToken cancellationToken)
        {
            GChainIterator iter = new GChainIterator(path);

            GCurve curve = iter.FirstCurve();
            while (curve != null)
            {
                if (cancelToken.IsCancellationRequested)
                    cancelToken.ThrowIfCancellationRequested();

                if (curve.box.Overlaps(viewBox))
                    AnimateAlongCurve(curve, cancellationToken);

                curve = iter.NextCurve();
            }
        }

        // Creates the toolRenderer for the tool associated with this path.
        private void ToolRendererCreate(GChain path)
        {
            GChain tool = tchains[path];
            double chordalTol = ViewChordTol();

            VertexList verts = new VertexList();
            tool.Tabulate(verts, chordalTol);
            toolRenderer = new WireframeRenderer(wireframeShader, verts, colorMap[GChainType.TOOL]);
        }

        private void ToolRendererDispose()
        {
            if ((modelessDialog != null) && (modelessDialog.GetType() == typeof(AnimateDialog)))
                ((AnimateDialog)modelessDialog).Reset();

            if (toolRenderer == null)
                return;

            toolRenderer = null!;
            Render();
        }

        private void AnimateAlongCurve(GCurve curve, CancellationToken cancellationToken)
        {
            double delta = ViewDist();
            if (curve.IsA(T.ARC))
                delta /= 2;

            // Calculate the tool display locations.
            VertexList verts = new VertexList();
            curve.Digitize(verts, delta);

            foreach (Vertex v in verts)
            {
                if (cancelToken.IsCancellationRequested)
                    cancelToken.ThrowIfCancellationRequested();

                toolOrigin.X = v.x;
                toolOrigin.Y = v.y;

                Invoke((Action)(() =>
                {
                    Render();
                }));

                DateTime start = DateTime.Now;
                while (true)
                {
                    TimeSpan elapsed = DateTime.Now - start;
                    if (elapsed.TotalMilliseconds >= 2)
                        break;
                }
            }
        }
    }
}
