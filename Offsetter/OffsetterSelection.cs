using Offsetter.Entities;
using Offsetter.Io;
using Offsetter.Math;
using Offsetter.Solver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Offsetter
{
    using GChainList = List<GChain>;

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
                    dialog.AcceptAction += UniformDialogAcceptAction;
                    dialog.OffsetDist = offsetDist;
                    dialog.OffsetSide = offsetSide;
                    selectionDialog = dialog;
                }
                else
                {
                    NonUniformOffsetDialog dialog = new NonUniformOffsetDialog(geoMenuLocation);
                    dialog.AcceptAction += NonUniformDialogAcceptAction;
                    dialog.OffsetSide = offsetSide;
                    selectionDialog = dialog;
                }

                selectionDialog.Text = dialogTitle;
                selectionDialog.ClosedAction += SelectionDialogClosedAction;
                selectionDialog.Show(glControl);

                MenusEnable(false);
            }
        }

        private void SelectionDialogClosedAction(object? sender, EventArgs e)
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

            if (selectionDialog.Text == PROPERTIES)
            {
                SelectedCurvesClear();
                SelectedCurvesAdd(curve);
                showDegrees = ((PropertiesDialog)selectionDialog).ShowDegrees;
            }
            else if (selectionDialog.Text == UNIFORM)
            {
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    SelectedChainRemove(curve.Owner);
                    ((UniformOffsetDialog)selectionDialog).Remove(curve);
                    Render();
                    return;
                }
                else
                {
                    SelectedChainAdd(curve.Owner);
                }
            }
            else
            {
                SelectedChainAdd(curve.Owner);
            }

            Render();

            selectionDialog.Update(curve);
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
