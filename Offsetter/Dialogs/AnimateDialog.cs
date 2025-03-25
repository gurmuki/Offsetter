using Offsetter.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Offsetter.Dialogs
{
    /// <summary>Create a modeless dialog for animating tool movement along a path.</summary>
    public partial class AnimateDialog : SelectionDialog
    {
        private const string START = "Start";
        private const string STOP = "Stop";

        private Dictionary<Keys, Label> buttons = new Dictionary<Keys, Label>();
        private Dictionary<Keys, int> speeds = new Dictionary<Keys, int>();

        private GChain chain = null!;
        private bool idle = true;

        //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        /// <summary>The current speed key.</summary>
        /// It's maintained here so the client does have to keep track.
        private static Keys SpeedKey { get; set; } = Keys.D5;

        /// <summary>Returns true when the given key a "speed control" key?</summary>
        public static bool IsSpeedKey(Keys keyCode)
        {
            return ((keyCode >= Keys.D1) && (keyCode <= Keys.D9));
        }

        //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        /// <summary>Client actions to perform when this dialog's Action button is clicked.</summary>
        public event EventHandler<GChain> Action = null!;

        /// <summary>Create a modeless dialog for animating tool movement along a path.</summary>
        /// <param name="screenLocation">The screen coordinate where the dialog should be presented.</param>
        public AnimateDialog(Point screenLocation)
            : base(screenLocation)
        {
            InitializeComponent();
        }

        /// <summary>Set the animation speed.</summary>
        public void SpeedSet(Keys keyCode)
        {
            SpeedKey = keyCode;

            foreach (var pair in buttons)
            { pair.Value.BackColor = System.Drawing.SystemColors.Control; }

            buttons[SpeedKey].BackColor = System.Drawing.SystemColors.ActiveCaption;
        }

        /// <summary>Get the delay time (ms) associated with the current speed.</summary>
        public int Delay { get { return speeds[SpeedKey]; } }

        /// <summary>Returns true when entity selection is allowed.</summary>
        public override bool SelectionAllowed { get { return idle; } }

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
            idle = true;

            action.Text = START;

            pathID.Focus();

            action.Enabled = (pathID.Text != string.Empty);
        }

        private void AnimateDialog_Load(object sender, EventArgs e)
        {
            chain = null!;
            pathID.Text = string.Empty;

            buttons[Keys.D1] = b1;
            buttons[Keys.D2] = b2;
            buttons[Keys.D3] = b3;
            buttons[Keys.D4] = b4;
            buttons[Keys.D5] = b5;
            buttons[Keys.D6] = b6;
            buttons[Keys.D7] = b7;
            buttons[Keys.D8] = b8;
            buttons[Keys.D9] = b9;

            speeds = new Dictionary<Keys, int>();
            speeds[Keys.D1] = 64;
            speeds[Keys.D2] = 32;
            speeds[Keys.D3] = 16;
            speeds[Keys.D4] = 12;
            speeds[Keys.D5] = 8;
            speeds[Keys.D6] = 6;
            speeds[Keys.D7] = 4;
            speeds[Keys.D8] = 2;
            speeds[Keys.D9] = 1;

            SpeedSet(SpeedKey);
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
            }

            pathID.Focus();

            if (Action != null)
                Action.Invoke(this, chain);
        }

        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (IsSpeedKey(keyData))
            {
                SpeedSet(keyData);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
