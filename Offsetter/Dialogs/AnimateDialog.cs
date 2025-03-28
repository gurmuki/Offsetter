using Offsetter.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static Offsetter.Dialogs.AnimationData;

namespace Offsetter.Dialogs
{
    /// <summary>Create a modeless dialog for animating tool movement along a path.</summary>
    public partial class AnimateDialog : SelectionDialog
    {
        private const string START = "Start";
        private const string STOP = "Stop";

        private Dictionary<Keys, Label> buttons = new Dictionary<Keys, Label>();
        private Dictionary<Keys, int> speeds = new Dictionary<Keys, int>();

        private GChain path = null!;

        private bool Idle { get; set; }

        //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        private bool loading = false;

        /// <summary>The current speed key.</summary>
        /// It's maintained here so the client does have to keep track.
        private static Keys SpeedKey { get; set; } = Keys.D5;

        /// <summary>Returns true when the given key a "speed control" key?</summary>
        public static bool IsSpeedKey(Keys keyCode)
        {
            return ((keyCode >= Keys.D0) && (keyCode <= Keys.D9));
        }

        //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        /// <summary>Client actions to perform when this dialog's Action button is clicked.</summary>
        public event EventHandler<AnimationData> Action = null!;

        /// <summary>Create a modeless dialog for animating tool movement along a path.</summary>
        /// <param name="screenLocation">The screen coordinate where the dialog should be presented.</param>
        public AnimateDialog(Point screenLocation)
            : base(screenLocation)
        {
            InitializeComponent();
        }

        // Clearly, the application will crash when Action is not defined. This
        // guard makes unnecessary the need to check (Action == null) elsewhere.
        private void AnimateDialog_Load(object sender, EventArgs e)
        {
            if (Action == null)
                throw new InvalidOperationException("AnimateDialog requires a defined Action.");

            loading = true;
            DialogInit();
            loading = false;
        }

        private void AnimateDialog_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (path != null)
                Action.Invoke(this, new AnimationData(null!, ActionType.TERMINATE));
        }

        /// <summary>Set the animation speed.</summary>
        public void SpeedSet(Keys keyCode)
        {
            if (loading || (keyCode != SpeedKey))
            {
                SpeedKey = keyCode;

                foreach (var pair in buttons)
                { pair.Value.BackColor = System.Drawing.SystemColors.Control; }

                if (buttons.ContainsKey(SpeedKey))
                    buttons[SpeedKey].BackColor = System.Drawing.SystemColors.ActiveCaption;
            }

            ButtonsEnable();

            if (step.Enabled)
            {
                action.Text = START;
                Idle = true;
                Action.Invoke(this, new AnimationData(null!, ActionType.STOP));
            }
        }

        /// <summary>Get the delay time (ms) associated with the current speed.</summary>
        public int Delay { get { return speeds[SpeedKey]; } }

        /// <summary>Returns true when entity selection is allowed.</summary>
        public override bool SelectionAllowed { get { return Idle; } }

        public override void Update(GCurve curve)
        {
            this.Focus();

            GChain owner = curve.Owner;
            if (owner == path)
                return;

            Action.Invoke(this, new AnimationData(null!, ActionType.TERMINATE));

            path = owner;
            pathID.Text = path.id.ToString();

            ButtonsEnable();

            if (path != null)
                Action.Invoke(this, new AnimationData(path, ActionType.INITIALIZE));
        }

        public override void Remove(GCurve curve)
        {
            this.Focus();
            Update();
        }

        public void Reset()
        {
            Idle = true;

            action.Text = START;
            path = null!;

            pathID.Focus();

            ButtonsEnable();
        }

        private void action_Click(object sender, EventArgs e)
        {
            pathID.Focus();

            if (Idle)
            {
                if (SpeedKey == Keys.D0)
                {
                    step_Click(sender, e);
                    return;
                }

                action.Text = STOP;
                Idle = false;

                Action.Invoke(this, new AnimationData(path, ActionType.START));
            }
            else
            {
                action.Text = START;
                Idle = true;

                Action.Invoke(this, new AnimationData(null!, ActionType.STOP));
            }
        }

        // Not real happy with the solution ... but it works.
        private void step_Click(object sender, EventArgs e)
        {
            Action.Invoke(this, new AnimationData(path, ActionType.STEP));

            b1_Click(sender, e);
            Application.DoEvents();

            b0_Click(sender, e);
            Application.DoEvents();
        }

        private void DialogInit()
        {
            path = null!;
            pathID.Text = string.Empty;

            ButtonsInit();
            SpeedsInit();

            SpeedSet(SpeedKey);
            Reset();
        }

        private void ButtonsEnable()
        {
            action.Enabled = (pathID.Text != string.Empty);
            step.Enabled = (action.Enabled && (SpeedKey == Keys.D0));
        }

        private void b0_Click(object sender, EventArgs e) => SendKeys.Send(KeyString(Keys.D0));
        private void b1_Click(object sender, EventArgs e) => SendKeys.Send(KeyString(Keys.D1));
        private void b2_Click(object sender, EventArgs e) => SendKeys.Send(KeyString(Keys.D2));
        private void b3_Click(object sender, EventArgs e) => SendKeys.Send(KeyString(Keys.D3));
        private void b4_Click(object sender, EventArgs e) => SendKeys.Send(KeyString(Keys.D4));
        private void b5_Click(object sender, EventArgs e) => SendKeys.Send(KeyString(Keys.D5));
        private void b6_Click(object sender, EventArgs e) => SendKeys.Send(KeyString(Keys.D6));
        private void b7_Click(object sender, EventArgs e) => SendKeys.Send(KeyString(Keys.D7));
        private void b8_Click(object sender, EventArgs e) => SendKeys.Send(KeyString(Keys.D8));
        private void b9_Click(object sender, EventArgs e) => SendKeys.Send(KeyString(Keys.D9));

        private string KeyString(Keys key)
        {
            int i = (int)key - (int)Keys.D0;
            return i.ToString();
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

        private void ButtonsInit()
        {
            buttons = new Dictionary<Keys, Label>();
            buttons[Keys.D0] = b0;
            buttons[Keys.D1] = b1;
            buttons[Keys.D2] = b2;
            buttons[Keys.D3] = b3;
            buttons[Keys.D4] = b4;
            buttons[Keys.D5] = b5;
            buttons[Keys.D6] = b6;
            buttons[Keys.D7] = b7;
            buttons[Keys.D8] = b8;
            buttons[Keys.D9] = b9;
        }

        private void SpeedsInit()
        {
            speeds = new Dictionary<Keys, int>();
            speeds[Keys.D0] = 0;
            speeds[Keys.D1] = 64;
            speeds[Keys.D2] = 32;
            speeds[Keys.D3] = 16;
            speeds[Keys.D4] = 12;
            speeds[Keys.D5] = 8;
            speeds[Keys.D6] = 6;
            speeds[Keys.D7] = 4;
            speeds[Keys.D8] = 2;
            speeds[Keys.D9] = 1;
        }
    }

    public class AnimationData
    {
        public enum ActionType { INITIALIZE, START, STEP, STOP, TERMINATE }

        public AnimationData(GChain path, ActionType action)
        {
            Path = path;
            Action = action;
        }

        public GChain Path { get; private set; } = null!;

        public ActionType Action { get; private set; } = ActionType.STOP;
    }
}
