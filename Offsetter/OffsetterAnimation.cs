using Offsetter.Dialogs;
using Offsetter.Entities;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Offsetter
{
    public partial class Offsetter : Form
    {
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

        private void AnimateAlongCurve(GCurve curve, CancellationToken cancellationToken)
        {
            double delta = ViewDist(curve);

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

                AnimationPause();
            }
        }

        private void AnimationPause()
        {
            AnimateDialog dialog = (AnimateDialog)modelessDialog;
            if (dialog == null)
                return;

            int delay = dialog.Delay;

            // The resolution of Thread.Sleep() is approximately 16 ms and
            // so is good enough to use for times >= 16 ms. A tight loop is
            // used for shorter delay times. When using a tight loop for
            // longer delays, I hear the cpu fan whirring.
            if (delay < 16)
            {
                DateTime start = DateTime.Now;
                while (true)
                {
                    TimeSpan elapsed = DateTime.Now - start;
                    if (elapsed.TotalMilliseconds >= delay)
                        break;
                }
            }
            else
            {
                Thread.Sleep(delay);
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
            if (IsAnimateDialog(modelessDialog))
                ((AnimateDialog)modelessDialog).Reset();

            if (toolRenderer == null)
                return;

            toolRenderer = null!;
            Render();
        }
    }
}
