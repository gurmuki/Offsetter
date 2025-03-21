using Offsetter.Entities;
using Offsetter.Math;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    public partial class Offsetter : Form
    {
        // The limit below which any gl ordinate is considered to be zero.
        const float RESOLUTION = 1E-6F;

        // Clips space (viewport coordinates)
        // -1, 1 -------- 1, 1
        //    |             |
        //    |             |
        //    |      +      |
        //    |             |
        //    |             |
        // -1,-1 -------- 1,-1

        /// <summary>Get the cursor X ordinate as a value in clip space (-1 .. 1)</summary>
        private float XClip(int x)
        {
            float tmp = ((float)x - center.X) / center.X;
            return ((System.Math.Abs(tmp) < RESOLUTION) ? 0f : tmp);
        }

        /// <summary>Get the cursor Y ordinate as a value in clip space (-1 .. 1)</summary>
        private float YClip(int y)
        {
            float tmp = ((float)y - center.Y) / -center.Y;
            return ((System.Math.Abs(tmp) < RESOLUTION) ? 0f : tmp);
        }

        /// <summary>Updates the control points for the windowing rectangle.</summary>
        private void RectUpdate(MouseEvent me, Point pt)
        {
            if (me == MouseEvent.LButtonUp)
            {
                windowingObject.Clear();
            }
            else
            {
                if (me == MouseEvent.LButtonDown)
                    windowingObject.FirstPt(XClip(pt.X), YClip(pt.Y));
                else if (me == MouseEvent.LButtonDownMoving)
                    windowingObject.SecondPt(XClip(pt.X), YClip(pt.Y));
            }
        }

        private GCurve ViewPick(Point pt)
        {
            double wx = (XClip(pt.X) + 1) / 2;
            double wy = (YClip(pt.Y) + 1) / 2;

            double mx = (double)(viewBox.xmin + (wx * viewBox.dx));
            double my = (double)(viewBox.ymin + (wy * viewBox.dy));

            GPoint mpt = new GPoint(mx, my);

            // pickTol -- the modelspace equivalent of 6 pixels.
            double pickTol = viewBox.dx * ((XClip(6) + 1) / 2);

            Closest closest = new NotClosest();
            foreach (GChain chain in ichains)
            {
                Closest candidate = chain.Closest(mpt, pickTol);
                if (candidate.Distance < closest.Distance)
                    closest = candidate;
            }

            foreach (GChain chain in ochains)
            {
                Closest candidate = chain.Closest(mpt, pickTol);
                if (candidate.Distance < closest.Distance)
                    closest = candidate;
            }

            return closest.Curve;
        }

        private void ViewPush()
        {
            if (views.Count < 1)
            {
                views.Push(new GBox(modelBox));
            }
            else
            {
                if (viewBox != views.Peek())
                    views.Push(new GBox(viewBox));
            }
        }

        private void ViewPop()
        {
            GBox box = ((views.Count < 1) ? modelBox : views.Pop());
            ViewUpdate(box);
        }

        private void ViewsClear()
        {
            views.Clear();
        }

        private void ViewBase()
        {
            ViewUpdate(modelBox);
        }

        /// <summary>Given a rectangle in GL coordinates returns the corresponding modelBox coordinates.</summary>
        /// <param name="glcoords [xmin, ymin, xmax, ymax]"></param>
        /// <returns>modelBox coordinates [xmin, ymin, xmax, ymax]</returns>
        // wpts -- GL coordinates in the range [-1..1]
        private void ViewWindow(double[] glcoords)
        {
            Debug.Assert(
                (glcoords[0] >= -1) && (glcoords[0] >= -1)
                && (glcoords[2] <= 1) && (glcoords[3] <= 1));

            double wXmin = (glcoords[0] + 1) / 2;
            double wYmin = (glcoords[1] + 1) / 2;
            double wXmax = (glcoords[2] + 1) / 2;
            double wYmax = (glcoords[3] + 1) / 2;

            double wXc = (wXmin + wXmax) / 2;
            double wYc = (wYmin + wYmax) / 2;

            double mXc = (double)(viewBox.xmin + (wXc * viewBox.dx));
            double mYc = (double)(viewBox.ymin + (wYc * viewBox.dy));

            double dx = wXmax - wXmin;
            double dy = wYmax - wYmin;
            double delta = (double)((dx < dy) ? dy * viewBox.dy : dx * viewBox.dx) / 2;

            GBox box = new GBox(
                mXc - delta, mYc - delta,
                mXc + delta, mYc + delta);

            ViewUpdate(box);
        }

        private void ViewTranslate(double x, double y)
        {
            GBox box = new GBox(
                viewBox.xmin + x, viewBox.ymin + y,
                viewBox.xmax + x, viewBox.ymax + y);

            ViewUpdate(box);
        }

        private void ViewZoom(double glx, double gly, double scale)
        {
            double wx = (glx + 1) / 2;
            double wy = (gly + 1) / 2;

            double mx = (double)(viewBox.xmin + (wx * viewBox.dx));
            double my = (double)(viewBox.ymin + (wy * viewBox.dy));

            GBox box = viewBox.ScaleAbout(mx, my, scale);
            ViewUpdate(box);
        }

        private double ViewHalfDelta(GBox box)
        {
            return ((box.dx > box.dy) ? box.dx : box.dy) / 2;
        }

        private double ViewChordTol()
        {
            double delta = ViewHalfDelta(viewBox);
            return (delta * ((XClip(2) + 1) / 2));
        }

        private double ViewDist()
        {
            double delta = ViewHalfDelta(viewBox);
            return (delta * ((XClip(2) + 1) / 2));
        }

        /// <summary>Regenerate display lists so arcs look smooth.</summary>
        /// <param name="box">The bounds of the model within the view.</param>
        /// <remarks>Optionally, we could do away with 'box' and directly use 'modelBox'</remarks>
        private void ViewUpdate(GBox box)
        {
            double delta = ViewHalfDelta(box);

            viewBox = new GBox(
                (float)(box.xc - delta), (float)(box.yc - delta),
                (float)(box.xc + delta), (float)(box.yc + delta));

            model = Matrix4.CreateOrthographicOffCenter(
                (float)viewBox.xmin, (float)viewBox.xmax,
                (float)viewBox.ymin, (float)viewBox.ymax,
                -1f, 1f);

            // Tabulate only visible entities.
            double chordalTol = ViewChordTol();
            foreach (var pair in boxMap)
            {
                if (pair.Key.GetType() != typeof(GArc))
                    continue;  // Only arcs need to be tabulated.

                if (!pair.Value.Overlaps(viewBox))
                    continue;  // Trivial rejection.

                GArc arc = (GArc)pair.Key;
                if (!InView(arc))
                    continue;  // No portion of the arc is in the view.

                //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
                // An optimization to avoid regenerating the same vertices.
                // A properly rendered arc will have nVerts == nChords + 1
                //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
                int nVerts = (rendererMap.ContainsKey(arc) ? rendererMap[arc].vertexCount : 0);
                int nChords = arc.TabulationCount(chordalTol);
                if ((nVerts - 1) == nChords)
                    continue;
                //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

                VertexList verts = new VertexList();
                arc.Tabulate(verts, chordalTol);

                VColor color = rendererMap[arc].color;
                rendererMap[arc] = new WireframeRenderer(wireframeShader, verts, color);
            }
        }

        private bool InView(GArc arc)
        {
            Closest closest;
            int count = 0;

            closest = arc.Closest(new GPoint(viewBox.xmin, viewBox.ymax));
            count += System.Math.Sign(closest.Distance);

            closest = arc.Closest(new GPoint(viewBox.xmax, viewBox.ymax));
            count += System.Math.Sign(closest.Distance);

            closest = arc.Closest(new GPoint(viewBox.xmax, viewBox.ymin));
            count += System.Math.Sign(closest.Distance);

            closest = arc.Closest(new GPoint(viewBox.xmin, viewBox.ymin));
            count += System.Math.Sign(closest.Distance);

            if (count == -4)
            {
                // All view points are contained by the arc.
                // In other words, the arc is outside of the view.
                return false;
            }

            if (count == 4)
            {
                // All view points are outside of the arc so
                // determine whether the arc center is within the view.
                return viewBox.Contains(arc.pc);
            }

            // [1..3] view points are contained by the arc.
            return true;
        }
    }
}
