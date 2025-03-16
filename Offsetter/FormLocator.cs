using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    public class FormLocator
    {
        public static void Locate(Form form, Point screenLocation)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = screenLocation;
        }
    }
}
