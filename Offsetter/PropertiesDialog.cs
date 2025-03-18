using Offsetter.Entities;
using Offsetter.Math;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    // NOTE: I tried using the AutoSize property to manage form resizing
    // but the behavior appearred to be problematic and not worth the
    // bother to "fix". Perhaps my ignorance is getting in the way.

    /// <summary>PropertiesDialog is a modeless dialog displaying entity properties.</summary>
    public partial class PropertiesDialog : Form
    {
        private const int PADDING = 8;
        private GCurve curve = null!;
        private ClosedAction closedAction;

        public delegate void ClosedAction();

        public PropertiesDialog(Point screenLocation, ClosedAction f)
        {
            if (f == null)
                throw new ArgumentNullException();

            InitializeComponent();

            closedAction = f;

            FormLocator.Locate(this, screenLocation);
        }

        public bool ShowDegrees { get; set; } = false;

        private void PropertiesDialog_Load(object sender, EventArgs e)
        {
            endptProperties.Visible = false;
            arcProperties.Visible = false;

            close.Location = new Point(close.Location.X, type.Bottom + PADDING);
            this.ClientSize = new Size(this.ClientSize.Width, close.Bottom + PADDING);
        }

        private void Properties_FormClosing(object sender, FormClosingEventArgs e)
        {
            Visible = false;
            e.Cancel = true;
            closedAction();
        }

        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
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

            endptProperties.Visible = true;

            if (curve.GetType() == typeof(GLine))
            {
                arcProperties.Visible = false;
                close.Location = new Point(close.Location.X, endptProperties.Bottom);
            }
            else
            {
                arcProperties.Visible = true;
                close.Location = new Point(close.Location.X, arcProperties.Bottom);
            }

            this.ClientSize = new Size(this.ClientSize.Width, close.Bottom + PADDING);

            PropertiesUpdateCore();
        }

        private void PropertiesUpdateCore()
        {
            bool isArc = (curve.GetType() == typeof(GArc));

            id.Text = curve.id.ToString();
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
