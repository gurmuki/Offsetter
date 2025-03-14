﻿
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
// NOTE: Compilation issues will arise after modifying the dialog using the Designer.
//   Whatever the reason, all references to CustomControls.CComboBox will be changed
//   to Offsetter.CustomControls.CComboBox, causing an error. I have not been
//   able to find any reference/solution to such behavior on the web. As such, the
//   current solution is simply to edit the offending code.
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

using OpenTK.GLControl;
using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    partial class Offsetter
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Offsetter));
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            reopenToolStripMenuItem = new ToolStripMenuItem();
            saveresultsdxfToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            testToolStripMenuItem = new ToolStripMenuItem();
            geometryToolStripMenuItem = new ToolStripMenuItem();
            uniformOffsetToolStripMenuItem = new ToolStripMenuItem();
            convexHullOffsetToolStripMenuItem = new ToolStripMenuItem();
            nestToolStripMenuItem = new ToolStripMenuItem();
            decomposeToolStripMenuItem = new ToolStripMenuItem();
            toolingToolStripMenuItem = new ToolStripMenuItem();
            reorderToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            glControl = new GLControl();
            viewPopMenu = new ContextMenuStrip(components);
            panToolStripMenuItem = new ToolStripMenuItem();
            windowToolStripMenuItem = new ToolStripMenuItem();
            zoomToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            fullViewToolStripMenuItem = new ToolStripMenuItem();
            previousViewToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            toolTip = new ToolTip(components);
            menuStrip1.SuspendLayout();
            viewPopMenu.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, geometryToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1048, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, reopenToolStripMenuItem, saveresultsdxfToolStripMenuItem, toolStripSeparator1, testToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            fileToolStripMenuItem.DropDownOpening += fileToolStripMenuItem_DropDownOpening;
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(168, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // reopenToolStripMenuItem
            // 
            reopenToolStripMenuItem.Name = "reopenToolStripMenuItem";
            reopenToolStripMenuItem.Size = new Size(168, 22);
            reopenToolStripMenuItem.Text = "Reopen";
            reopenToolStripMenuItem.Click += reopenToolStripMenuItem_Click;
            // 
            // saveresultsdxfToolStripMenuItem
            // 
            saveresultsdxfToolStripMenuItem.Name = "saveresultsdxfToolStripMenuItem";
            saveresultsdxfToolStripMenuItem.Size = new Size(168, 22);
            saveresultsdxfToolStripMenuItem.Text = "Save (_results.dxf)";
            saveresultsdxfToolStripMenuItem.Click += saveresultsdxfToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(165, 6);
            // 
            // testToolStripMenuItem
            // 
            testToolStripMenuItem.Name = "testToolStripMenuItem";
            testToolStripMenuItem.Size = new Size(168, 22);
            testToolStripMenuItem.Text = "Test";
            testToolStripMenuItem.Click += testToolStripMenuItem_Click;
            // 
            // geometryToolStripMenuItem
            // 
            geometryToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { uniformOffsetToolStripMenuItem, convexHullOffsetToolStripMenuItem, nestToolStripMenuItem, decomposeToolStripMenuItem, toolingToolStripMenuItem, reorderToolStripMenuItem });
            geometryToolStripMenuItem.Name = "geometryToolStripMenuItem";
            geometryToolStripMenuItem.Size = new Size(71, 20);
            geometryToolStripMenuItem.Text = "Geometry";
            // 
            // uniformOffsetToolStripMenuItem
            // 
            uniformOffsetToolStripMenuItem.Name = "uniformOffsetToolStripMenuItem";
            uniformOffsetToolStripMenuItem.Size = new Size(174, 22);
            uniformOffsetToolStripMenuItem.Text = "Uniform Offset";
            uniformOffsetToolStripMenuItem.Click += uniformOffsetToolStripMenuItem_Click;
            // 
            // convexHullOffsetToolStripMenuItem
            // 
            convexHullOffsetToolStripMenuItem.Name = "convexHullOffsetToolStripMenuItem";
            convexHullOffsetToolStripMenuItem.Size = new Size(174, 22);
            convexHullOffsetToolStripMenuItem.Text = "Convex Hull Offset";
            convexHullOffsetToolStripMenuItem.Click += convexHullOffsetToolStripMenuItem_Click;
            // 
            // nestToolStripMenuItem
            // 
            nestToolStripMenuItem.Name = "nestToolStripMenuItem";
            nestToolStripMenuItem.Size = new Size(174, 22);
            nestToolStripMenuItem.Text = "Nest";
            nestToolStripMenuItem.Click += nestToolStripMenuItem_Click;
            // 
            // decomposeToolStripMenuItem
            // 
            decomposeToolStripMenuItem.Name = "decomposeToolStripMenuItem";
            decomposeToolStripMenuItem.Size = new Size(174, 22);
            decomposeToolStripMenuItem.Text = "Decompose";
            decomposeToolStripMenuItem.Click += decomposeToolStripMenuItem_Click;
            // 
            // toolingToolStripMenuItem
            // 
            toolingToolStripMenuItem.CheckOnClick = true;
            toolingToolStripMenuItem.Name = "toolingToolStripMenuItem";
            toolingToolStripMenuItem.Size = new Size(174, 22);
            toolingToolStripMenuItem.Text = "Tooling";
            toolingToolStripMenuItem.Click += toolingToolStripMenuItem_Click;
            // 
            // reorderToolStripMenuItem
            // 
            reorderToolStripMenuItem.Name = "reorderToolStripMenuItem";
            reorderToolStripMenuItem.Size = new Size(174, 22);
            reorderToolStripMenuItem.Text = "Reorder";
            reorderToolStripMenuItem.Click += reorderToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new Size(161, 22);
            saveAsToolStripMenuItem.Text = "Save As ...";
            saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
            // 
            // glControl
            // 
            glControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            glControl.API = OpenTK.Windowing.Common.ContextAPI.OpenGL;
            glControl.APIVersion = new System.Version(3, 3, 0, 0);
            glControl.BackColor = SystemColors.Control;
            glControl.Flags = OpenTK.Windowing.Common.ContextFlags.Default;
            glControl.IsEventDriven = true;
            glControl.Location = new Point(162, 144);
            glControl.Margin = new Padding(3, 4, 3, 4);
            glControl.Name = "glControl";
            glControl.Profile = OpenTK.Windowing.Common.ContextProfile.Compatability;
            glControl.SharedContext = null;
            glControl.Size = new Size(400, 400);
            glControl.TabIndex = 2;
            glControl.Text = "glControl1";
            glControl.PreviewKeyDown += glControl_PreviewKeyDown;
            // 
            // viewPopMenu
            // 
            viewPopMenu.Items.AddRange(new ToolStripItem[] { panToolStripMenuItem, windowToolStripMenuItem, zoomToolStripMenuItem, toolStripSeparator2, fullViewToolStripMenuItem, previousViewToolStripMenuItem, toolStripSeparator3, saveAsToolStripMenuItem });
            viewPopMenu.Name = "viewPopMenu";
            viewPopMenu.Size = new Size(181, 170);
            // 
            // panToolStripMenuItem
            // 
            panToolStripMenuItem.Name = "panToolStripMenuItem";
            panToolStripMenuItem.ShortcutKeyDisplayString = "C";
            panToolStripMenuItem.Size = new Size(161, 22);
            panToolStripMenuItem.Text = "Pan";
            panToolStripMenuItem.Click += panToolStripMenuItem_Click;
            // 
            // windowToolStripMenuItem
            // 
            windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            windowToolStripMenuItem.ShortcutKeyDisplayString = "W";
            windowToolStripMenuItem.Size = new Size(161, 22);
            windowToolStripMenuItem.Text = "Window";
            windowToolStripMenuItem.Click += windowToolStripMenuItem_Click;
            // 
            // zoomToolStripMenuItem
            // 
            zoomToolStripMenuItem.Name = "zoomToolStripMenuItem";
            zoomToolStripMenuItem.ShortcutKeyDisplayString = "Z";
            zoomToolStripMenuItem.Size = new Size(161, 22);
            zoomToolStripMenuItem.Text = "Zoom";
            zoomToolStripMenuItem.Click += zoomToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(158, 6);
            // 
            // fullViewToolStripMenuItem
            // 
            fullViewToolStripMenuItem.Name = "fullViewToolStripMenuItem";
            fullViewToolStripMenuItem.ShortcutKeyDisplayString = "F";
            fullViewToolStripMenuItem.Size = new Size(180, 22);
            fullViewToolStripMenuItem.Text = "Full View";
            fullViewToolStripMenuItem.Click += fullViewToolStripMenuItem_Click;
            // 
            // previousViewToolStripMenuItem
            // 
            previousViewToolStripMenuItem.Name = "previousViewToolStripMenuItem";
            previousViewToolStripMenuItem.ShortcutKeyDisplayString = "V";
            previousViewToolStripMenuItem.Size = new Size(180, 22);
            previousViewToolStripMenuItem.Text = "Previous View";
            previousViewToolStripMenuItem.Click += previousViewToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(158, 6);
            // 
            // Offsetter
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.WindowFrame;
            ClientSize = new Size(1048, 1063);
            Controls.Add(glControl);
            Controls.Add(menuStrip1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
            Name = "Offsetter";
            Text = "2D Offsetter";
            FormClosed += Offsetter_FormClosed;
            Load += Form1_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            viewPopMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem geometryToolStripMenuItem;
        private ToolStripMenuItem uniformOffsetToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripMenuItem saveresultsdxfToolStripMenuItem;
        private ToolStripMenuItem reopenToolStripMenuItem;
        private ToolStripMenuItem reorderToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem testToolStripMenuItem;
        private ToolStripMenuItem toolingToolStripMenuItem;
        private ToolStripMenuItem convexHullOffsetToolStripMenuItem;
        private ToolStripMenuItem decomposeToolStripMenuItem;
        private ToolStripMenuItem nestToolStripMenuItem;
        private GLControl glControl;
        private System.Windows.Forms.ContextMenuStrip viewPopMenu;
        private System.Windows.Forms.ToolStripMenuItem panToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zoomToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem fullViewToolStripMenuItem;
        private ToolStripMenuItem previousViewToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
    }
}
