using System;
using System.Windows.Forms;

namespace Offsetter
{
    // Keyboard processing functionality.
    public partial class Offsetter : Form
    {
        // Hookup the key and mouse event handlers.
        private void GLControlEventHandlersBind()
        {
            // Log any focus changes.
            #region UnusedEventHandlers
#if UNIMPLEMENTED_EVENT_HANDLERS
            glControl.GotFocus += (sender, e) =>
                Log("Focus in");

            glControl.LostFocus += (sender, e) =>
                Log("Focus out");

            // See also Offsetter_KeyPress() because it intercepts key presses.
            glControl.PreviewKeyDown += (sender, e) =>
            {
                PreviewKeyEvent(e.KeyCode);
            };

            glControl.KeyDown += (sender, e) =>
            {
                // Log($"Key down: {e.KeyCode}");
            };

            glControl.KeyUp += (sender, e) =>
            {
                // Log($"Key up: {e.KeyCode}");
            };

            glControl.KeyPress += (sender, e) =>
            {
                // Log($"Key press: {e.KeyChar}");
            };
#endif
            #endregion

            glControl.MouseDown += MouseDownHandler!;
            glControl.MouseUp += MouseUpHandler!;
            glControl.MouseMove += MouseMoveHandler!;
            glControl.MouseWheel += MouseWheelHandler!;
        }

        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
            // Log($"Mouse down: ({e.X},{e.Y})");
            glControl.Focus();

            if (viewMode == ViewMode.Static)
                return;

            if (viewMode == ViewMode.Windowing)
            {
                RectUpdate(MouseEvent.LButtonDown, e.Location);
            }
            else if (viewMode == ViewMode.Panning)
            {
                viewPt = e.Location;
                viewPtLocked = true;
            }
        }

        private bool showDegrees = false;

        private void MouseUpHandler(object sender, MouseEventArgs e)
        {
            // Log($"Mouse up: ({e.X},{e.Y})");
            if (modelessDialog != null)
            {
                // Ignore all actions excepting curve selection.
                if ((e.Button == MouseButtons.Left) && IsSelectionDialog(modelessDialog))
                    SelectionDialogUpdate(e.Location);

                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                this.ContextMenuStrip = viewPopMenu;
                this.ContextMenuStrip.Show(Cursor.Position);
                return;
            }

            if (viewMode == ViewMode.Static)
                return;

            if (viewMode == ViewMode.Windowing)
            {
                double[] glcoords = windowingObject.GLCoordinates();
                RectUpdate(MouseEvent.LButtonUp, e.Location);

                ViewWindow(glcoords);
                Render();

                viewMode = ViewMode.Static;
            }
            else if (viewMode == ViewMode.Panning)
            {
                viewPtLocked = false;
                viewMode = ViewMode.Static;
            }
            else if (viewMode == ViewMode.Zooming)
            {
                if (!viewPtLocked)
                {
                    viewPt = e.Location;
                    viewPtLocked = true;
                }
            }
            else if (viewMode == ViewMode.Picking)
            {
                viewPtLocked = false;
                SelectionDialogShow(PROPERTIES);
            }
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            // Log($"Mouse move: ({e.X},{e.Y})");

            if (viewMode == ViewMode.Static)
                return;

            if (viewMode == ViewMode.Windowing)
            {
                RectUpdate(MouseEvent.LButtonDownMoving, e.Location);
                Render();
            }
            else if (viewMode == ViewMode.Panning)
            {
                if (!viewPtLocked)
                    return;

                int dx = viewPt.X - e.X;
                int dy = viewPt.Y - e.Y;
                if ((dx == 0) && (dy == 0))
                    return;

                viewPt = e.Location;

                double delta = ((viewBox.dx > viewBox.dy) ? viewBox.dx : viewBox.dy);

                float vdx = (dx / (float)glControl.Width) * (float)delta;
                float vdy = (dy / (float)glControl.Height) * (float)delta;

                ViewTranslate(vdx, -vdy);

                Render();
            }
        }

        // NOTE: From the user's perspective, there are two ways to initiate zooming:
        // 1) Place the cursor where you want the "about point" to be and then
        //    start spinning the mouse wheel.
        // 2) Place the cursor where you want the "about point" to be, left mouse
        //    button click and then start spinning the mouse wheel.
        //
        // Why these two methods? I found myself frequently forgetting to mouse click
        // and found it the behavior to be very annoying.
        private void MouseWheelHandler(object sender, MouseEventArgs e)
        {
            // The gl delta-z for each mouse wheel click.
            const float ZOOMDELTA = 0.01f;

            if (viewMode != ViewMode.Zooming)
                return;

            if (!viewPtLocked)
            {
                viewPt = e.Location;
                viewPtLocked = true;
            }

            double glx = XClip(viewPt.X);
            double gly = YClip(viewPt.Y);
            double dz = ((e.Delta > 0) ? -ZOOMDELTA : ZOOMDELTA);
            ViewZoom(glx, gly, 1 + dz);

            Render();
        }

        private void KeyPreviewExecute(Keys keyCode)
        {
            if (modelessDialog != null)
            {
                if (keyCode == Keys.Escape)
                {
                    modelessDialog.Close();
                }
                else if (keyCode == Keys.F)
                {
                    ViewsClear();
                    ViewBase();
                    Render();
                }

                return;
            }

            if (keyCode == Keys.Escape)
            {
                if (viewMode == ViewMode.Static)
                    return;  // Nothing to do.

                if (viewMode == ViewMode.Windowing)
                    windowingObject.Clear();

                ViewPop();
                Render();

                viewPtLocked = false;
                viewMode = ViewMode.Static;
            }
            else if (keyCode == Keys.F)
            {
                ViewsClear();
                ViewBase();
                Render();

                viewPtLocked = false;
                viewMode = ViewMode.Static;
            }
            else if (keyCode == Keys.W)
            {
                ViewPush();

                viewPtLocked = false;
                viewMode = ViewMode.Windowing;
            }
            else if (keyCode == Keys.C)
            {
                ViewPush();

                viewPtLocked = false;
                viewMode = ViewMode.Panning;
            }
            else if (keyCode == Keys.Z)
            {
                ViewPush();

                viewPtLocked = false;
                viewMode = ViewMode.Zooming;
            }
            else if (keyCode == Keys.V)
            {
                ViewPop();
                Render();

                viewPtLocked = false;
                viewMode = ViewMode.Static;
            }
            else if (keyCode == Keys.P)
            {
                SelectionDialogShow(PROPERTIES);

                viewPtLocked = false;
                viewMode = ViewMode.Picking;
            }
        }

        private bool IsSelectionDialog(ModelessDialog dialog)
        {
            if (dialog == null)
                throw new ArgumentNullException();

            return (dialog.GetType().BaseType == typeof(SelectionDialog));
        }
    }
}
