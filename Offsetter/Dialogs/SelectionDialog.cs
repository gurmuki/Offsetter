using Offsetter.Entities;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    // NOTE: While a dialog can be declared abstract, you
    // won't be able to edit it using the IDE form designer.
    //    public abstract partial class SelectionDialog : Form
    public partial class SelectionDialog : Form
    {
        /// <summary>Client actions to perform when this dialog has closed.</summary>
        public event EventHandler ClosedAction = null!;

        /// <summary>Required by the IDE form designer.</summary>
        protected SelectionDialog()
        {
            InitializeComponent();
        }

        /// <summary>Create a modeless selection dialog.</summary>
        /// <param name="screenLocation"></param>
        protected SelectionDialog(Point screenLocation)
        {
            InitializeComponent();

            StartPosition = FormStartPosition.Manual;
            FormLocator.Locate(this, screenLocation);
        }

        public virtual bool UpdateAllowed { get { throw new NotImplementedException(); } }

        public virtual void Update(GCurve curve) { throw new NotImplementedException(); }

        protected void SelectionDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            Visible = false;
            e.Cancel = true;
            if (ClosedAction != null)
                ClosedAction.Invoke(this, EventArgs.Empty);
        }
    }
}
