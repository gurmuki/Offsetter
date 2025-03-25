using Offsetter.Entities;
using Offsetter.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    /// <summary>UniformOffsetDialog is a modeless dialog for obtaining uniform offset input.</summary>
    public partial class UniformOffsetDialog : SelectionDialog
    {
        private Control previousActive = null!;

        // The chains used as input to the client's offsetting method.
        private List<GChain> chains = new List<GChain>();

        /// <summary>Client actions to perform when this dialog's Accept button is clicked.</summary>
        public event EventHandler Action = null!;

        /// <summary>Create a modeless dialog for obtaining uniform offset input.</summary>
        /// <param name="screenLocation">The screen coordinate where the dialog should be presented.</param>
        public UniformOffsetDialog(Point screenLocation)
            : base(screenLocation)
        {
            InitializeComponent();
        }

        public int OffsetSide { get; set; } = 0;

        public double OffsetDist { get; set; } = 0;

        public List<GChain> Chains => chains;

        /// <summary>Returns true when entity selection is allowed.</summary>
        public override bool SelectionAllowed { get { return (ActiveControl == chainIDs); } }

        public override void Update(GCurve curve)
        {
            this.Focus();

            GChain chain = curve.Owner;
            if (chains.Contains(chain))
                return;

            chains.Add(chain);

            UpdateAndEnable();
        }

        public override void Remove(GCurve curve)
        {
            this.Focus();

            GChain chain = curve.Owner;
            if (!chains.Contains(chain))
                return;

            chains.Remove(chain);

            UpdateAndEnable();
        }

        private void UpdateAndEnable()
        {
            chainIDs.Text = string.Empty;

            foreach (GChain chain in chains)
            {
                if (chainIDs.Text.Length > 0)
                    chainIDs.Text += ",";

                chainIDs.Text += chain.id.ToString();
            }

            accept.Enabled = (chains.Count > 0);
        }

        private void UniformOffsetDialog_Load(object sender, EventArgs e)
        {
            OffsetSide = ((OffsetSide < 0) ? GConst.RIGHT : GConst.LEFT);
            OffsetDist = System.Math.Abs(OffsetDist);

            left.Checked = (OffsetSide == GConst.LEFT);
            right.Checked = !left.Checked;

            offset.Text = OffsetDist.ToString();

            accept.Enabled = false;
        }

        private void accept_Click(object sender, EventArgs e)
        {
            OffsetSide = (left.Checked ? GConst.LEFT : GConst.RIGHT);

            double dval = 0;
            if (double.TryParse(offset.Text, out dval))
                OffsetDist = dval;

            if (Action != null)
                Action.Invoke(this, EventArgs.Empty);
        }

        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void chainIDs_Enter(object sender, EventArgs e) { previousActive = chainIDs; }

        private void offset_Enter(object sender, EventArgs e) { previousActive = offset; }

        // Send the cursor back to the previously active control.
        private void left_CheckedChanged(object sender, EventArgs e) { ActiveControl = previousActive; }
    }
}
