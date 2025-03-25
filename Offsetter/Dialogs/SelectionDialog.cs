using Offsetter.Entities;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    /// <summary>A modeless selection dialog supporting curve selection.</summary>
    public partial class SelectionDialog : ModelessDialog
    {
        /// <summary>Required by the IDE form designer.</summary>
        protected SelectionDialog() : base() { }

        /// <summary>Create a modeless selection dialog supporting curve selection.</summary>
        /// <param name="screenLocation">The initial display position (screen coordinate).</param>
        protected SelectionDialog(Point screenLocation)
            : base(screenLocation)
        {
            InitializeComponent();

            StartPosition = FormStartPosition.Manual;
            FormLocator.Locate(this, screenLocation);
        }

        /// <summary>Intended to return true when entity selection is allowed.</summary>
        public virtual bool SelectionAllowed { get { throw new NotImplementedException(); } }

        public virtual void Update(GCurve curve) { throw new NotImplementedException(); }

        public virtual void Remove(GCurve curve) { throw new NotImplementedException(); }
    }
}
