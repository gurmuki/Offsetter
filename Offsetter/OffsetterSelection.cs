using Offsetter.Dialogs;
using Offsetter.Entities;
using Offsetter.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    public partial class Offsetter : Form
    {
        private void SelectionDialogShow(string dialogTitle)
        {
            if (modelessDialog == null)
            {
                if (dialogTitle == PROPERTIES)
                {
                    PropertiesDialog dialog = new PropertiesDialog(geoMenuLocation);
                    dialog.ShowDegrees = showDegrees;
                    modelessDialog = dialog;
                }
                else if (dialogTitle == UNIFORM)
                {
                    UniformOffsetDialog dialog = new UniformOffsetDialog(geoMenuLocation);
                    dialog.Action += UniformDialogAction;
                    dialog.OffsetDist = offsetDist;
                    dialog.OffsetSide = offsetSide;
                    modelessDialog = dialog;
                }
                else if ((dialogTitle == NON_UNIFORM) || (dialogTitle == NESTING))
                {
                    NonUniformOffsetDialog dialog = new NonUniformOffsetDialog(geoMenuLocation);
                    dialog.Action += NonUniformDialogAction;
                    dialog.OffsetSide = offsetSide;
                    modelessDialog = dialog;
                }
                else if (dialogTitle == ANIMATE)
                {
                    AnimateDialog dialog = new AnimateDialog(geoMenuLocation);
                    dialog.Action += AnimateDialogAction;
                    modelessDialog = dialog;
                }
                else
                {
                    throw new NotImplementedException($"{dialogTitle}");
                }

                modelessDialog!.Text = dialogTitle;
                modelessDialog.ClosedAction += SelectionDialogClosed;
                modelessDialog.Show(glControl);

                MenusEnable(false);
            }
        }

        private void SelectionDialogClosed(object? sender, EventArgs e)
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
            if (!dialog.UpdateAllowed)
                return;

            GCurve curve = ViewPick(mouseLocation);
            if (curve == null)
                return;

            GChain chain = curve.Owner;
            if (dialog.Text == PROPERTIES)
            {
                SelectedCurvesClear();
                SelectedCurvesAdd(curve);
                showDegrees = ((PropertiesDialog)dialog).ShowDegrees;
            }
            else if (dialog.Text == ANIMATE)
            {
                if (dialog.Text == ANIMATE)
                {
                    if (!IsToolpath(chain))
                        return;
                }

                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    SelectedChainRemove(chain);
                    dialog.Remove(curve);
                    Render();
                    return;
                }

                SelectedCurvesClear();
                SelectedChainAdd(chain);
            }
            else if (dialog.Text == UNIFORM)
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

        private void MaskDialogShow()
        {
            MaskDialog dialog = new MaskDialog(geoMenuLocation);

            dialog.ClosedAction += MaskDialogClosed;
            dialog.Action += MaskDialogAction;

            dialog.InputMask = inputMask;
            dialog.PathMask = pathMask;
            dialog.IntermediateMask = intermediateMask;

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

            if (dialog.InputMask != inputMask)
            {
                inputMask = dialog.InputMask;
                ChainMask(Layer.PART, inputMask);
            }

            if (dialog.PathMask != pathMask)
            {
                pathMask = dialog.PathMask;
                ChainMask(Layer.PATH, pathMask);
            }

            if (dialog.IntermediateMask != intermediateMask)
            {
                intermediateMask = dialog.IntermediateMask;
                ChainMask(Layer.INTERMEDIATE, intermediateMask);
            }

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
