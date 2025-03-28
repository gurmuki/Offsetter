using Offsetter.Dialogs;
using Offsetter.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Offsetter
{
    public partial class Offsetter : Form
    {
        private VertexList verts = null!;
        private bool IsAnimationRunning { get; set; } = false;
        private int vndx = -1;

        private VertexList AnimationPathDigitize(GChain path)
        {
            VertexList v = new VertexList();

            GChainIterator iter = new GChainIterator(path);
            GCurve curve = iter.FirstCurve();
            while (curve != null)
            {
                if (curve.box.Overlaps(viewBox))
                {
                    double delta = ViewDist(curve);
                    curve.Digitize(v, delta);
                }

                curve = iter.NextCurve();
            }

            return v;
        }

        private async void AnimateAction(object? sender, AnimationData data)
        {
            if (data == null)
                throw new ArgumentNullException();

            if (data.Action == AnimationData.ActionType.INITIALIZE)
            {
                cancelTokenSource = new CancellationTokenSource();
                cancelToken = cancelTokenSource.Token;

                IsAnimationRunning = false;
                ToolRendererCreate(data.Path);
                verts = AnimationPathDigitize(data.Path);

                vndx = 0;
                Vertex startPt = verts[vndx];
                toolOrigin.X = startPt.x;
                toolOrigin.Y = startPt.y;

                Invoke((Action)(() =>
                {
                    Render();
                }));
            }
            else if (data.Action == AnimationData.ActionType.STEP)
            {
                ++vndx;
                if (vndx >= verts.Count)
                    return;

                Vertex startPt = verts[vndx];
                toolOrigin.X = startPt.x;
                toolOrigin.Y = startPt.y;

                Invoke((Action)(() =>
                {
                    Render();
                }));
            }
            else if (data.Action == AnimationData.ActionType.START)
            {
                cancelTokenSource = new CancellationTokenSource();
                cancelToken = cancelTokenSource.Token;

                var task = Task.Run(() =>
                {
                    IsAnimationRunning = true;
                    Animate(data.Path);
                }, cancelToken);

                try
                {
                    await task;

                    IsAnimationRunning = false;
                    ToolRendererDispose();
                }
                catch (OperationCanceledException)
                {
                    // Debug.WriteLine($"AnimateDialogAction: {nameof(OperationCanceledException)} thrown\n");
                }
            }
            else if (data.Action == AnimationData.ActionType.STOP)
            {
                cancelTokenSource.Cancel();
                IsAnimationRunning = false;
            }
            else if (data.Action == AnimationData.ActionType.TERMINATE)
            {
                cancelTokenSource.Cancel();
                IsAnimationRunning = false;

                ToolRendererDispose();
            }
        }

        private void Animate(GChain path)
        {
            while (vndx < verts.Count)
            {
                if (cancelToken.IsCancellationRequested)
                    cancelToken.ThrowIfCancellationRequested();

                Vertex pt = verts[vndx];
                toolOrigin.X = pt.x;
                toolOrigin.Y = pt.y;

                Invoke((Action)(() =>
                {
                    Render();
                }));

                AnimationPause();
                ++vndx;
            }
        }

        private void AnimationPause()
        {
            AnimateDialog dialog = (AnimateDialog)modelessDialog;
            if (dialog == null)
                return;

            if (dialog.Delay == 0)
            {
                while (dialog.Delay == 0)
                {
                    if (cancelToken.IsCancellationRequested)
                        cancelToken.ThrowIfCancellationRequested();

                    Thread.Sleep(10);
                }

                return;
            }

            // The resolution of Thread.Sleep() is approximately 16 ms and
            // so is good enough to use for times >= 16 ms. A tight loop is
            // used for shorter delay times. When using a tight loop for
            // longer delays, I hear the cpu fan whirring.
            if (dialog.Delay < 16)
            {
                int delay = dialog.Delay;

                DateTime start = DateTime.Now;
                while (true)
                {
                    TimeSpan elapsed = DateTime.Now - start;
                    if (elapsed.TotalMilliseconds >= delay)
                        return;
                }
            }
            else
            {
                Thread.Sleep(dialog.Delay);
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
