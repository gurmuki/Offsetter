using System;
using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
    // NOTE: While a dialog can be declared abstract, you
    // won't be able to edit it using the IDE form designer.
    //
    //    public abstract partial class ModelessDialog : Form
    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

    /// <summary>A modeless dialog base class.</summary>
    public partial class ModelessDialog : Form
    {
        /// <summary>Client actions to perform when this dialog has closed.</summary>
        public event EventHandler ClosedAction = null!;

        /// <summary>Required by the IDE form designer.</summary>
        protected ModelessDialog()
        {
            InitializeComponent();
        }

        /// <summary>Create a modeless dialog base class instance.</summary>
        /// <param name="screenLocation">The initial display position (screen coordinate).</param>
        protected ModelessDialog(Point screenLocation)
        {
            InitializeComponent();

            StartPosition = FormStartPosition.Manual;
            FormLocator.Locate(this, screenLocation);
        }

        protected void ModelessDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            Visible = false;
            e.Cancel = true;
            if (ClosedAction != null)
                ClosedAction.Invoke(this, EventArgs.Empty);
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
