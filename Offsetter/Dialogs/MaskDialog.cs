using System;
using System.Drawing;

namespace Offsetter.Dialogs
{
    public partial class MaskDialog : ModelessDialog
    {
        private bool initializing = true;

        /// <summary>Client actions to perform when a dialog checkbox is changed.</summary>
        public event EventHandler Action = null!;

        public MaskDialog(Point screenLocation)
            : base(screenLocation)
        {
            InitializeComponent();
        }

        public bool InputMask { get; set; } = false;
        public bool PathMask { get; set; } = false;
        public bool IntermediateMask { get; set; } = false;

        private void MaskDialog_Load(object sender, EventArgs e)
        {
            Input.Checked = InputMask;
            Path.Checked = PathMask;
            Intermediate.Checked = IntermediateMask;

            initializing = false;
        }

        private void Input_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing || (Action == null))
                return;

            InputMask = Input.Checked;
            Action.Invoke(this, EventArgs.Empty);
        }

        private void Path_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing || (Action == null))
                return;

            PathMask = Path.Checked;
            Action.Invoke(this, EventArgs.Empty);
        }

        private void Intermediate_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing || (Action == null))
                return;

            IntermediateMask = Intermediate.Checked;
            Action.Invoke(this, EventArgs.Empty);
        }
    }
}
