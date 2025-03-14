using Offsetter.Entities;
using Offsetter.Math;
using System;
using System.Windows.Forms;

namespace Offsetter
{
    public partial class Canonical : Form
    {
        private GCurve curve;

        public Canonical(GCurve curve)
        {
            InitializeComponent();

            this.curve = curve;

            if (ShowDegrees)
                degrees.Checked = true;
            else
                radians.Checked = true;
        }

        public bool ShowDegrees { get; set; } = false;

        private void Canonical_Load(object sender, EventArgs e)
        {
            if (curve.GetType() == typeof(GLine))
            {
                arcProperties.Visible = false;
                this.Size = new System.Drawing.Size(this.Width, this.Height - 126);
            }

            if (ShowDegrees)
                degrees.Checked = true;
            else
                radians.Checked = true;

            PropertiesUpdate();
        }

        private void radians_CheckedChanged(object sender, EventArgs e)
        {
            ShowDegrees = degrees.Checked;

            PropertiesUpdate();
            Update();
        }

        private void PropertiesUpdate()
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

                double factor = (ShowDegrees ? GConst.RAD2DEG : 1);

                sa.Text = Format((arc.sa * factor));
                ea.Text = Format((arc.ea * factor));
            }
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
