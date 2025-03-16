using Offsetter.Entities;
using Offsetter.Math;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    /// <summary>Canonical is a modeless dialog displaying entity properties.</summary>
    public partial class Canonical : Form
    {
        private GCurve curve = null!;
        private Point scrLoc;
        private Size fullSize;
        private ClosedAction closedAction;

        public delegate void ClosedAction();

        public Canonical(ClosedAction f)
        {
            InitializeComponent();
            fullSize = this.Size;
            closedAction = f;
        }

        public bool ShowDegrees { get; set; } = false;

        private void Canonical_FormClosing(object sender, FormClosingEventArgs e)
        {
            Visible = false;
            e.Cancel = true;
            closedAction();
        }

        private void radians_CheckedChanged(object sender, EventArgs e)
        {
            ShowDegrees = degrees.Checked;

            ArcAnglesUpdate();
            Update();
        }

        public void PropertiesUpdate(Point screenLocation, GCurve curve)
        {
            this.Focus();

            this.curve = curve;
            this.scrLoc = screenLocation;

            if (curve.GetType() == typeof(GLine))
            {
                arcProperties.Visible = false;
                this.Size = new System.Drawing.Size(fullSize.Width, fullSize.Height - arcProperties.Height);
            }
            else
            {
                arcProperties.Visible = true;
                this.Size = fullSize;
            }

            PropertiesUpdateCore();
        }

        private void PropertiesUpdateCore()
        {
            bool isArc = (curve.GetType() == typeof(GArc));

            type.Text = (isArc ? " Arc" : "Line");

            xs.Text = Format(curve.ps.x);
            ys.Text = Format(curve.ps.y);

            xe.Text = Format(curve.pe.x);
            ye.Text = Format(curve.pe.y);

            if (isArc)
            {
                GArc arc = (GArc)curve;

                xc.Text = Format(arc.pc.x);
                yc.Text = Format(arc.pc.y);

                rad.Text = Format(arc.rad);
                dir.Text = arc.dir.ToString();

                ArcAnglesUpdate();
            }

            if (Owner != null)
            {
                // Ensure the dialog is location wholly with the bounds of its owner.
                Rectangle ownerRect = Owner.RectangleToScreen(Owner.ClientRectangle);

                int dx = this.Width;
                int dy = this.Height;

                Rectangle thisRect = new Rectangle();
                for (int i = 0; i < 4; i++)
                {
                    Point loc = new Point(scrLoc.X, scrLoc.Y);
                    if (i == 1)
                        loc.X -= dx;
                    else if (i == 2)
                        loc.Y -= dy;
                    else if (i == 3)
                        loc = new Point(scrLoc.X - dx, scrLoc.Y - dy);

                    thisRect = new Rectangle(loc.X, loc.Y, dx, dy);
                    if (ownerRect.Contains(thisRect))
                        break;
                }

                scrLoc = thisRect.Location;
            }

            FormLocator.Locate(this, scrLoc);
        }

        private void ArcAnglesUpdate()
        {
            GArc arc = (GArc)curve;

            if (ShowDegrees)
                degrees.Checked = true;
            else
                radians.Checked = true;

            double factor = (ShowDegrees ? GConst.RAD2DEG : 1);

            sa.Text = Format((arc.sa * factor));
            ea.Text = Format((arc.ea * factor));
        }

        private string Format(double val)
        {
            string s = string.Format("{0:0.000000}", val);
            return (s.Contains('.') ? s.TrimEnd('0').TrimEnd('.') : s);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
