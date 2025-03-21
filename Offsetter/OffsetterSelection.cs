using Offsetter.Dialogs;
using Offsetter.Entities;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    public partial class Offsetter : Form
    {
        private void SelectionDialogShow(string dialogTitle)
        {
            if (selectionDialog == null)
            {
                if (dialogTitle == PROPERTIES)
                {
                    PropertiesDialog dialog = new PropertiesDialog(geoMenuLocation);
                    dialog.ShowDegrees = showDegrees;
                    selectionDialog = dialog;
                }
                else if (dialogTitle == UNIFORM)
                {
                    UniformOffsetDialog dialog = new UniformOffsetDialog(geoMenuLocation);
                    dialog.Action += UniformDialogAction;
                    dialog.OffsetDist = offsetDist;
                    dialog.OffsetSide = offsetSide;
                    selectionDialog = dialog;
                }
                else if ((dialogTitle == NON_UNIFORM) || (dialogTitle == NESTING))
                {
                    NonUniformOffsetDialog dialog = new NonUniformOffsetDialog(geoMenuLocation);
                    dialog.Action += NonUniformDialogAction;
                    dialog.OffsetSide = offsetSide;
                    selectionDialog = dialog;
                }
                else if (dialogTitle == ANIMATE)
                {
                    AnimateDialog dialog = new AnimateDialog(geoMenuLocation);
                    dialog.Action += AnimateDialogAction;
                    selectionDialog = dialog;
                }
                else
                {
                    throw new NotImplementedException($"{dialogTitle}");
                }

                selectionDialog!.Text = dialogTitle;
                selectionDialog.ClosedAction += SelectionDialogClosed;
                selectionDialog.Show(glControl);

                MenusEnable(false);
            }
        }

        private void SelectionDialogClosed(object? sender, EventArgs e)
        {
            SelectedCurvesClear();

            selectionDialog.Dispose();
            selectionDialog = null!;

            viewMode = ViewMode.Static;

            MenusEnable(true);
        }

        private void SelectionDialogUpdate(Point mouseLocation)
        {
            if (!selectionDialog.UpdateAllowed)
                return;

            GCurve curve = ViewPick(mouseLocation);
            if (curve == null)
                return;

            GChain chain = curve.Owner;
            if (selectionDialog.Text == PROPERTIES)
            {
                SelectedCurvesClear();
                SelectedCurvesAdd(curve);
                showDegrees = ((PropertiesDialog)selectionDialog).ShowDegrees;
            }
            else if (selectionDialog.Text == ANIMATE)
            {
                if (selectionDialog.Text == ANIMATE)
                {
                    if (!IsToolpath(chain))
                        return;
                }

                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    SelectedChainRemove(chain);
                    selectionDialog.Remove(curve);
                    Render();
                    return;
                }

                SelectedCurvesClear();
                SelectedChainAdd(chain);
            }
            else if (selectionDialog.Text == UNIFORM)
            {
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    SelectedChainRemove(chain);
                    selectionDialog.Remove(curve);
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

            selectionDialog.Update(curve);
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
