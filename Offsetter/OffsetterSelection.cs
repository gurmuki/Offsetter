using Offsetter.Dialogs;
using Offsetter.Entities;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    public partial class Offsetter : Form
    {
        private void PropertiesDialogShow()
        {
            if (modelessDialog != null)
                return;

            PropertiesDialog dialog = new PropertiesDialog(geoMenuLocation);
            dialog.ShowDegrees = showDegrees;

            ModelessDialogShow(dialog);
        }

        private void UniformOffsetDialogShow()
        {
            if (modelessDialog != null)
                return;

            UniformOffsetDialog dialog = new UniformOffsetDialog(geoMenuLocation);
            dialog.Action += UniformDialogAction;
            dialog.OffsetDist = offsetDist;
            dialog.OffsetSide = offsetSide;

            ModelessDialogShow(dialog);
        }

        private void NonUniformOffsetDialogShow(bool nesting)
        {
            if (modelessDialog != null)
                return;

            NonUniformOffsetDialog dialog = new NonUniformOffsetDialog(geoMenuLocation);
            dialog.Nesting = nesting;
            dialog.Action += NonUniformDialogAction;
            dialog.OffsetSide = offsetSide;

            ModelessDialogShow(dialog);
        }

        private void AnimateDialogShow()
        {
            if (modelessDialog != null)
                return;

            AnimateDialog dialog = new AnimateDialog(geoMenuLocation);
            dialog.Action += AnimateAction;

            ModelessDialogShow(dialog);
        }

        private void ModelessDialogShow(ModelessDialog dialog)
        {
            if (modelessDialog != null)
                return;

            modelessDialog = dialog;
            modelessDialog.ClosedAction += ModelessDialogClosed;
            modelessDialog.Show(glControl);

            MenusEnable(false);
        }

        private void ModelessDialogClosed(object? sender, EventArgs e)
        {
            SelectedCurvesClear();

            modelessDialog.Dispose();
            modelessDialog = null!;

            viewMode = ViewMode.Static;

            MenusEnable(true);
        }

        private void SelectionDialogUpdate(Point mouseLocation)
        {
            if (!IsSelectionDialog(modelessDialog))
                throw new InvalidOperationException();

            SelectionDialog dialog = (SelectionDialog)modelessDialog;
            if (!dialog.SelectionAllowed)
                return;

            GCurve curve = ViewPick(mouseLocation);
            if (curve == null)
                return;

            GChain chain = curve.Owner;

            Type dialogType = dialog.GetType();
            if (dialogType == typeof(PropertiesDialog))
            {
                SelectedCurvesClear();
                SelectedCurvesAdd(curve);
                showDegrees = ((PropertiesDialog)dialog).ShowDegrees;
            }
            else if (dialogType == typeof(AnimateDialog))
            {
                if (!IsToolpath(chain))
                    return;

                SelectedCurvesClear();
                SelectedChainAdd(chain);

                // dialog.Reset()
            }
            else if (dialogType == typeof(UniformOffsetDialog))
            {
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    SelectedChainRemove(chain);
                    dialog.Remove(curve);
                    Render();
                    return;
                }

                SelectedChainAdd(chain);
            }
            else
            {
                SelectedChainAdd(chain);
            }

            Render();

            dialog.Update(curve);
        }

        private bool IsToolpath(GChain chain)
        {
            GChainType type = chain.ChainType;
            return ((type == GChainType.PATH) || (type == GChainType.POCKET) || (type == GChainType.ISLAND));
        }

        private void SelectedChainAdd(GChain chain)
        {
            GChainIterator iter = new GChainIterator(chain);

            GCurve curve = iter.FirstCurve();
            while (curve != null)
            {
                CurveHilight(curve, true);
                selectedCurves.Add(curve);

                curve = iter.NextCurve();
            }

            Render();
        }

        private void SelectedChainRemove(GChain chain)
        {
            GChainIterator iter = new GChainIterator(chain);

            GCurve curve = iter.FirstCurve();
            while (curve != null)
            {
                CurveHilight(curve, false);
                selectedCurves.Remove(curve);

                curve = iter.NextCurve();
            }

            Render();
        }

        private void SelectedCurvesAdd(GCurve curve)
        {
            CurveHilight(curve, true);
            selectedCurves.Add(curve);
        }

        private void SelectedCurvesClear()
        {
            foreach (GCurve curve in selectedCurves)
            { CurveHilight(curve, false); }

            selectedCurves.Clear();
            Render();
        }

        private void CurveHilight(GCurve curve, bool hilight)
        {
            if (curve == null)
                return;

            WireframeRenderer wr = (WireframeRenderer)rendererMap[curve];
            wr.LineWidth = (hilight ? 2 : 1);
        }
    }
}
