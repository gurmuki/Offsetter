using Offsetter.Math;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    public partial class OffsetDialog : Form
    {
        public OffsetDialog(Point screenLocation, string title, int offsetSide, double offsetDist)
        {
            InitializeComponent();

            this.Text = title;

            uniformPanel.Visible = true;
            nonUniformPanel.Visible = false;

            this.OffsetSide = ((offsetSide < 0) ? GConst.RIGHT : GConst.LEFT);
            this.OffsetDist = System.Math.Abs(offsetDist);

            this.Size = new Size(226, this.Height);

            FormLocator.Locate(this, screenLocation);
        }

        public OffsetDialog(Point screenLocation, string title, int offsetSide)
        {
            InitializeComponent();

            this.Text = title;

            nonUniformPanel.Location = uniformPanel.Location;

            nonUniformPanel.Visible = true;
            uniformPanel.Visible = false;

            this.OffsetSide = ((offsetSide < 0) ? GConst.RIGHT : GConst.LEFT);

            this.Size = new Size(226, this.Height);

            FormLocator.Locate(this, screenLocation);
        }

        public int OffsetSide { private set; get; } = 0;

        public double OffsetDist { private set; get; } = 0;

        private void OffsetDialog_Load(object sender, EventArgs e)
        {
            left.Checked = (OffsetSide == GConst.LEFT);
            right.Checked = !left.Checked;

            offset.Text = OffsetDist.ToString();
        }

        private void accept_Click(object sender, EventArgs e)
        {
            OffsetSide = (left.Checked ? GConst.LEFT : GConst.RIGHT);

            double dval = 0;
            if (double.TryParse(offset.Text, out dval))
                OffsetDist = dval;

            this.Close();
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

        private void left_CheckedChanged(object sender, EventArgs e)
        {
            OffsetSelect();
        }

        private void right_CheckedChanged(object sender, EventArgs e)
        {
            OffsetSelect();
        }

        private void OffsetSelect()
        {
            offset.Focus();
            offset.SelectAll();
        }
    }
}
