using Offsetter.Dialogs;
using Offsetter.Entities;
using Offsetter.Math;
using System;
using System.Windows.Forms;

namespace Offsetter
{
    public partial class Offsetter : Form
    {
        private void MaskDialogShow()
        {
            MaskDialog dialog = new MaskDialog(geoMenuLocation);

            dialog.ClosedAction += MaskDialogClosed;
            dialog.Action += MaskDialogAction;

            modelessDialog = dialog;
            modelessDialog.Show(glControl);

            MenusEnable(false);
        }

        private void MaskDialogClosed(object? sender, EventArgs e)
        {
            modelessDialog.Dispose();
            modelessDialog = null!;

            MenusEnable(true);
        }

        private void MaskDialogAction(object? sender, EventArgs e)
        {
            MaskDialog dialog = (MaskDialog)modelessDialog;

            if (dialog.InputMaskChanged)
                ChainMask(Layer.PART, MaskDialog.InputMask);

            if (dialog.PathMaskChanged)
                ChainMask(Layer.PATH, MaskDialog.PathMask);

            if (dialog.IntermediateMaskChanged)
                ChainMask(Layer.INTERMEDIATE, MaskDialog.IntermediateMask);

            Render();
        }

        private void ChainMask(Layer layer, bool mask)
        {
            if (layer == Layer.PART)
            {
                foreach (var chain in ichains)
                { ChainMask(chain, mask); }
            }
            else
            {
                foreach (var chain in ochains)
                {
                    if (chain.layer == layer)
                        ChainMask(chain, mask);
                }
            }
        }

        private void ChainMask(GChain chain, bool mask)
        {
            GChainIterator iter = new GChainIterator(chain);

            GCurve curve = iter.FirstCurve();
            while (curve != null)
            {
                rendererMap[curve].IsMasked = mask;

                curve = iter.NextCurve();
            }
        }
    }
}
