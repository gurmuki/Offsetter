using System;
using System.Drawing;

namespace Offsetter.Dialogs
{
    public partial class MaskDialog : ModelessDialog
    {
        private bool initializing = true;

        //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        // These settings are maintained here so the client does have to keep track.
        public static bool InputMask { get; set; } = false;
        public static bool PathMask { get; set; } = false;
        public static bool IntermediateMask { get; set; } = false;

        public static void MasksClear()
        {
            InputMask = false;
            PathMask = false;
            IntermediateMask = false;
        }

        //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        /// <summary>Client actions to perform when a dialog checkbox is changed.</summary>
        public event EventHandler Action = null!;

        public MaskDialog(Point screenLocation)
            : base(screenLocation)
        {
            InitializeComponent();
        }

        private void MaskDialog_Load(object sender, EventArgs e)
        {
            Input.Checked = InputMask;
            Path.Checked = PathMask;
            Intermediate.Checked = IntermediateMask;

            InputMaskChanged = false;
            PathMaskChanged = false;
            IntermediateMaskChanged = false;

            initializing = false;
        }

        /// <summary>Returns true if the Input checkbox has changed.</summary>
        public bool InputMaskChanged { get; private set; } = false;

        /// <summary>Returns true if the Path checkbox has changed.</summary>
        public bool PathMaskChanged { get; private set; } = false;

        /// <summary>Returns true if the Intermediate checkbox has changed.</summary>
        public bool IntermediateMaskChanged { get; private set; } = false;

        private void Input_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing || (Action == null))
                return;

            InputMask = Input.Checked;
            InputMaskChanged = true;

            Action.Invoke(this, EventArgs.Empty);
            InputMaskChanged = false;
        }

        private void Path_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing || (Action == null))
                return;

            PathMask = Path.Checked;
            PathMaskChanged = true;

            Action.Invoke(this, EventArgs.Empty);
            PathMaskChanged = false;
        }

        private void Intermediate_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing || (Action == null))
                return;

            IntermediateMask = Intermediate.Checked;
            IntermediateMaskChanged = true;

            Action.Invoke(this, EventArgs.Empty);
            IntermediateMaskChanged = false;
        }
    }
}
