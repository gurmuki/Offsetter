using Offsetter.Entities;
using Offsetter.Math;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    /// <summary>NonUniformOffsetDialog is a modeless dialog for obtaining non-uniform offset input.</summary>
    public partial class NonUniformOffsetDialog : SelectionDialog
    {
        private Control previousActive = null!;

        private GChain shape = null!;
        private GChain tool = null!;

        /// <summary>Client actions to perform when this dialog's Accept button is clicked.</summary>
        public event EventHandler Action = null!;

        /// <summary>Create a modeless dialog for obtaining non-uniform offset input.</summary>
        /// <param name="screenLocation">The screen coordinate where the dialog should be presented.</param>
        public NonUniformOffsetDialog(Point screenLocation)
            : base(screenLocation)
        {
            InitializeComponent();
        }

        public int OffsetSide { get; set; } = 0;

        public GChain Shape => shape;
        public GChain Tool => tool;

        public override bool UpdateAllowed { get { return ((ActiveControl == shapeID) || (ActiveControl == toolID)); } }

        public override void Update(GCurve curve)
        {
            this.Focus();

            if (shapeID.Focused)
            {
                shape = curve.Owner;
                shapeID.Text = shape.id.ToString();

                this.ActiveControl = toolID;
            }
            else if (toolID.Focused)
            {
                tool = curve.Owner;
                toolID.Text = tool.id.ToString();

                this.ActiveControl = shapeID;
            }

            accept.Enabled = ((shapeID.Text != toolID.Text)
                && (shapeID.Text != string.Empty) && (toolID.Text != string.Empty));
        }

        private void NonUniformOffsetDialog_Load(object sender, EventArgs e)
        {
            OffsetSide = ((OffsetSide < 0) ? GConst.RIGHT : GConst.LEFT);

            left.Checked = (OffsetSide == GConst.LEFT);
            right.Checked = !left.Checked;

            shapeID.Focus();
            accept.Enabled = false;
        }

        private void accept_Click(object sender, EventArgs e)
        {
            OffsetSide = (left.Checked ? GConst.LEFT : GConst.RIGHT);

            if (Action != null)
                Action.Invoke(this, EventArgs.Empty);
        }

        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void shapeID_Enter(object sender, EventArgs e) { previousActive = shapeID; }

        private void toolID_Enter(object sender, EventArgs e) { previousActive = toolID; }

        private void left_CheckedChanged(object sender, EventArgs e) { this.ActiveControl = previousActive; }
    }
}
