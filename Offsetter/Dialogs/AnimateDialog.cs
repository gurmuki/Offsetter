using Offsetter.Entities;
using System;
using System.Drawing;

namespace Offsetter.Dialogs
{
    /// <summary>Create a modeless dialog for animating tool movement along a path.</summary>
    public partial class AnimateDialog : SelectionDialog
    {
        private const string START = "Start";
        private const string STOP = "Stop";

        private GChain chain = null!;
        private bool idle = true;

        /// <summary>Client actions to perform when this dialog's Action button is clicked.</summary>
        public event EventHandler<GChain> Action = null!;

        /// <summary>Create a modeless dialog for animating tool movement along a path.</summary>
        /// <param name="screenLocation">The screen coordinate where the dialog should be presented.</param>
        public AnimateDialog(Point screenLocation)
            : base(screenLocation)
        {
            InitializeComponent();
        }

        public override bool UpdateAllowed { get { return idle; } }

        public override void Update(GCurve curve)
        {
            this.Focus();

            chain = curve.Owner;
            pathID.Text = chain.id.ToString();

            action.Enabled = true;
        }

        public override void Remove(GCurve curve)
        {
            this.Focus();

            Reset();
            Update();
        }

        public void Reset()
        {
            chain = null!;
            idle = true;

            action.Text = START;

            pathID.Text = string.Empty;
            pathID.Focus();

            action.Enabled = false;
        }

        private void AnimateDialog_Load(object sender, EventArgs e)
        {
            Reset();
        }

        private void AnimateDialog_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if ((chain != null) && (Action != null))
                Action.Invoke(this, null!);  // Stop any active animation.
        }

        private void action_Click(object sender, EventArgs e)
        {
            if (idle)
            {
                action.Text = STOP;
                idle = false;
            }
            else
            {
                action.Text = START;
                idle = true;
                chain = null!;
                pathID.Text = string.Empty;
            }

            pathID.Focus();

            if (Action != null)
                Action.Invoke(this, chain);
        }

        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
