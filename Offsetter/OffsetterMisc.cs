using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    public partial class Offsetter : Form
    {

        //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
        // Miscellaneous (and view-related) methods.
        //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        private Point TopLeft(Rectangle rect)
        {
            return (new Point(rect.Left, rect.Top));
        }

        private Point BottomRight(Rectangle rect)
        {
            return (new Point(rect.Right, rect.Bottom));
        }

        private bool NonEmpty(TextBox tb)
        {
            return (tb.Text.Length > 0);
        }

        private string MsgFormat(string template, string projectName)
        {
            string fmt = template + "\"{0}\"";
            return string.Format(fmt, projectName);
        }
    }
}
