
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
            fileMenu = new ToolStripMenuItem();
            openFileMenuItem = new ToolStripMenuItem();
            reopenFileMenuItem = new ToolStripMenuItem();
            saveResultsFileMenuItem = new ToolStripMenuItem();
            fileMenuSeparator1 = new ToolStripSeparator();
            testFileMenuItem = new ToolStripMenuItem();
            geometryMenu = new ToolStripMenuItem();
            uniformOffsetGeometryMenuItem = new ToolStripMenuItem();
            nonUniformOffsetGeometryMenuItem = new ToolStripMenuItem();
            nestGeometryMenuItem = new ToolStripMenuItem();
            geometryMenuSeparator1 = new ToolStripSeparator();
            decomposeGeometryMenuItem = new ToolStripMenuItem();
            reorderGeometryMenuItem = new ToolStripMenuItem();
            geometryMenuSeparator2 = new ToolStripSeparator();
            propertiesGeometryMenuItem = new ToolStripMenuItem();
            viewMenu = new ToolStripMenuItem();
            panViewMenuItem = new ToolStripMenuItem();
            windowViewMenuItem = new ToolStripMenuItem();
            zoomViewMenuItem = new ToolStripMenuItem();
            viewMenuSeparator1 = new ToolStripSeparator();
            fullViewMenuItem = new ToolStripMenuItem();
            previousViewMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            animateToolStripMenuItem = new ToolStripMenuItem();
            saveAsContextMenuItem = new ToolStripMenuItem();
            viewPopMenu = new ContextMenuStrip(components);
            panContextMenuItem = new ToolStripMenuItem();
            windowContextMenuItem = new ToolStripMenuItem();
            zoomContextMenuItem = new ToolStripMenuItem();
            contextMenuSeparator1 = new ToolStripSeparator();
            fullViewContextMenuItem = new ToolStripMenuItem();
            previousViewContextMenuItem = new ToolStripMenuItem();
            contextMenuSeparator2 = new ToolStripSeparator();
            propertiesContextMenuItem = new ToolStripMenuItem();
            toolTip = new ToolTip(components);
            mainPanel = new Panel();
            glControl = new GLControl();
            menuStrip1.SuspendLayout();
            viewPopMenu.SuspendLayout();
            mainPanel.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.BackColor = SystemColors.ScrollBar;
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileMenu, geometryMenu, viewMenu });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1048, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileMenu
            // 
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { openFileMenuItem, reopenFileMenuItem, saveResultsFileMenuItem, fileMenuSeparator1, testFileMenuItem });
            fileMenu.Name = "fileMenu";
            fileMenu.Size = new Size(37, 20);
            fileMenu.Text = "File";
            fileMenu.DropDownOpening += fileMenu_DropDownOpening;
            // 
            // openFileMenuItem
            // 
            openFileMenuItem.Name = "openFileMenuItem";
            openFileMenuItem.Size = new Size(168, 22);
            openFileMenuItem.Text = "Open";
            openFileMenuItem.Click += openFileMenuItem_Click;
            // 
            // reopenFileMenuItem
            // 
            reopenFileMenuItem.Name = "reopenFileMenuItem";
            reopenFileMenuItem.Size = new Size(168, 22);
            reopenFileMenuItem.Text = "Reopen";
            reopenFileMenuItem.Click += reopenFileMenuItem_Click;
            // 
            // saveResultsFileMenuItem
            // 
            saveResultsFileMenuItem.Name = "saveResultsFileMenuItem";
            saveResultsFileMenuItem.Size = new Size(168, 22);
            saveResultsFileMenuItem.Text = "Save (_results.dxf)";
            saveResultsFileMenuItem.Click += saveResultsFileMenuItem_Click;
            // 
            // fileMenuSeparator1
            // 
            fileMenuSeparator1.Name = "fileMenuSeparator1";
            fileMenuSeparator1.Size = new Size(165, 6);
            // 
            // testFileMenuItem
            // 
            testFileMenuItem.Name = "testFileMenuItem";
            testFileMenuItem.Size = new Size(168, 22);
            testFileMenuItem.Text = "Test";
            testFileMenuItem.Click += testFileMenuItem_Click;
            // 
            // geometryMenu
            // 
            geometryMenu.DropDownItems.AddRange(new ToolStripItem[] { uniformOffsetGeometryMenuItem, nonUniformOffsetGeometryMenuItem, nestGeometryMenuItem, geometryMenuSeparator1, decomposeGeometryMenuItem, reorderGeometryMenuItem, geometryMenuSeparator2, propertiesGeometryMenuItem });
            geometryMenu.Name = "geometryMenu";
            geometryMenu.Size = new Size(71, 20);
            geometryMenu.Text = "Geometry";
            // 
            // uniformOffsetGeometryMenuItem
            // 
            uniformOffsetGeometryMenuItem.Name = "uniformOffsetGeometryMenuItem";
            uniformOffsetGeometryMenuItem.Size = new Size(180, 22);
            uniformOffsetGeometryMenuItem.Text = "Uniform Offset";
            uniformOffsetGeometryMenuItem.Click += uniformOffsetGeometryMenuItem_Click;
            // 
            // nonUniformOffsetGeometryMenuItem
            // 
            nonUniformOffsetGeometryMenuItem.Name = "nonUniformOffsetGeometryMenuItem";
            nonUniformOffsetGeometryMenuItem.Size = new Size(180, 22);
            nonUniformOffsetGeometryMenuItem.Text = "Non-uniform Offset";
            nonUniformOffsetGeometryMenuItem.Click += nonUniformOffsetGeometryMenuItem_Click;
            // 
            // nestGeometryMenuItem
            // 
            nestGeometryMenuItem.Name = "nestGeometryMenuItem";
            nestGeometryMenuItem.Size = new Size(180, 22);
            nestGeometryMenuItem.Text = "Nest";
            nestGeometryMenuItem.Click += nestGeometryMenuItem_Click;
            // 
            // geometryMenuSeparator1
            // 
            geometryMenuSeparator1.Name = "geometryMenuSeparator1";
            geometryMenuSeparator1.Size = new Size(177, 6);
            // 
            // decomposeGeometryMenuItem
            // 
            decomposeGeometryMenuItem.Name = "decomposeGeometryMenuItem";
            decomposeGeometryMenuItem.Size = new Size(180, 22);
            decomposeGeometryMenuItem.Text = "Decompose";
            decomposeGeometryMenuItem.Click += decomposeGeometryMenuItem_Click;
            // 
            // reorderGeometryMenuItem
            // 
            reorderGeometryMenuItem.Name = "reorderGeometryMenuItem";
            reorderGeometryMenuItem.Size = new Size(180, 22);
            reorderGeometryMenuItem.Text = "Reorder";
            reorderGeometryMenuItem.Click += reorderGeometryMenuItem_Click;
            // 
            // geometryMenuSeparator2
            // 
            geometryMenuSeparator2.Name = "geometryMenuSeparator2";
            geometryMenuSeparator2.Size = new Size(177, 6);
            // 
            // propertiesGeometryMenuItem
            // 
            propertiesGeometryMenuItem.Name = "propertiesGeometryMenuItem";
            propertiesGeometryMenuItem.ShortcutKeyDisplayString = "P";
            propertiesGeometryMenuItem.Size = new Size(180, 22);
            propertiesGeometryMenuItem.Text = "Properties";
            propertiesGeometryMenuItem.Click += propertiesGeometryMenuItem_Click;
            // 
            // viewMenu
            // 
            viewMenu.DropDownItems.AddRange(new ToolStripItem[] { panViewMenuItem, windowViewMenuItem, zoomViewMenuItem, viewMenuSeparator1, fullViewMenuItem, previousViewMenuItem, toolStripSeparator1, animateToolStripMenuItem });
            viewMenu.Name = "viewMenu";
            viewMenu.Size = new Size(44, 20);
            viewMenu.Text = "View";
            // 
            // panViewMenuItem
            // 
            panViewMenuItem.Name = "panViewMenuItem";
            panViewMenuItem.ShortcutKeyDisplayString = "C";
            panViewMenuItem.Size = new Size(161, 22);
            panViewMenuItem.Text = "Pan";
            panViewMenuItem.Click += panViewMenuItem_Click;
            // 
            // windowViewMenuItem
            // 
            windowViewMenuItem.Name = "windowViewMenuItem";
            windowViewMenuItem.ShortcutKeyDisplayString = "W";
            windowViewMenuItem.Size = new Size(161, 22);
            windowViewMenuItem.Text = "Window";
            windowViewMenuItem.Click += windowViewMenuItem_Click;
            // 
            // zoomViewMenuItem
            // 
            zoomViewMenuItem.Name = "zoomViewMenuItem";
            zoomViewMenuItem.ShortcutKeyDisplayString = "Z";
            zoomViewMenuItem.Size = new Size(161, 22);
            zoomViewMenuItem.Text = "Zoom";
            zoomViewMenuItem.Click += zoomViewMenuItem_Click;
            // 
            // viewMenuSeparator1
            // 
            viewMenuSeparator1.Name = "viewMenuSeparator1";
            viewMenuSeparator1.Size = new Size(158, 6);
            // 
            // fullViewMenuItem
            // 
            fullViewMenuItem.Name = "fullViewMenuItem";
            fullViewMenuItem.ShortcutKeyDisplayString = "F";
            fullViewMenuItem.Size = new Size(161, 22);
            fullViewMenuItem.Text = "Full View";
            fullViewMenuItem.Click += fullViewMenuItem_Click;
            // 
            // previousViewMenuItem
            // 
            previousViewMenuItem.Name = "previousViewMenuItem";
            previousViewMenuItem.ShortcutKeyDisplayString = "V";
            previousViewMenuItem.Size = new Size(161, 22);
            previousViewMenuItem.Text = "Previous View";
            previousViewMenuItem.Click += previousViewMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(158, 6);
            // 
            // animateToolStripMenuItem
            // 
            animateToolStripMenuItem.Name = "animateToolStripMenuItem";
            animateToolStripMenuItem.Size = new Size(161, 22);
            animateToolStripMenuItem.Text = "Animate";
            animateToolStripMenuItem.Click += animateToolStripMenuItem_Click;
            // 
            // saveAsContextMenuItem
            // 
            saveAsContextMenuItem.Name = "saveAsContextMenuItem";
            saveAsContextMenuItem.Size = new Size(161, 22);
            saveAsContextMenuItem.Text = "Save As ...";
            saveAsContextMenuItem.Click += saveAsFileMenuItem_Click;
            // 
            // viewPopMenu
            // 
            viewPopMenu.Items.AddRange(new ToolStripItem[] { panContextMenuItem, windowContextMenuItem, zoomContextMenuItem, contextMenuSeparator1, fullViewContextMenuItem, previousViewContextMenuItem, contextMenuSeparator2, propertiesContextMenuItem, saveAsContextMenuItem });
            viewPopMenu.Name = "viewPopMenu";
            viewPopMenu.Size = new Size(162, 170);
            // 
            // panContextMenuItem
            // 
            panContextMenuItem.Name = "panContextMenuItem";
            panContextMenuItem.ShortcutKeyDisplayString = "C";
            panContextMenuItem.Size = new Size(161, 22);
            panContextMenuItem.Text = "Pan";
            panContextMenuItem.Click += panContextMenuItem_Click;
            // 
            // windowContextMenuItem
            // 
            windowContextMenuItem.Name = "windowContextMenuItem";
            windowContextMenuItem.ShortcutKeyDisplayString = "W";
            windowContextMenuItem.Size = new Size(161, 22);
            windowContextMenuItem.Text = "Window";
            windowContextMenuItem.Click += windowContextMenuItem_Click;
            // 
            // zoomContextMenuItem
            // 
            zoomContextMenuItem.Name = "zoomContextMenuItem";
            zoomContextMenuItem.ShortcutKeyDisplayString = "Z";
            zoomContextMenuItem.Size = new Size(161, 22);
            zoomContextMenuItem.Text = "Zoom";
            zoomContextMenuItem.Click += zoomContextMenuItem_Click;
            // 
            // contextMenuSeparator1
            // 
            contextMenuSeparator1.Name = "contextMenuSeparator1";
            contextMenuSeparator1.Size = new Size(158, 6);
            // 
            // fullViewContextMenuItem
            // 
            fullViewContextMenuItem.Name = "fullViewContextMenuItem";
            fullViewContextMenuItem.ShortcutKeyDisplayString = "F";
            fullViewContextMenuItem.Size = new Size(161, 22);
            fullViewContextMenuItem.Text = "Full View";
            fullViewContextMenuItem.Click += fullViewContextMenuItem_Click;
            // 
            // previousViewContextMenuItem
            // 
            previousViewContextMenuItem.Name = "previousViewContextMenuItem";
            previousViewContextMenuItem.ShortcutKeyDisplayString = "V";
            previousViewContextMenuItem.Size = new Size(161, 22);
            previousViewContextMenuItem.Text = "Previous View";
            previousViewContextMenuItem.Click += previousViewContextMenuItem_Click;
            // 
            // contextMenuSeparator2
            // 
            contextMenuSeparator2.Name = "contextMenuSeparator2";
            contextMenuSeparator2.Size = new Size(158, 6);
            // 
            // propertiesContextMenuItem
            // 
            propertiesContextMenuItem.Name = "propertiesContextMenuItem";
            propertiesContextMenuItem.ShortcutKeyDisplayString = "P";
            propertiesContextMenuItem.Size = new Size(161, 22);
            propertiesContextMenuItem.Text = "Properties";
            propertiesContextMenuItem.Click += propertiesContextMenuItem_Click;
            // 
            // mainPanel
            // 
            mainPanel.BackColor = SystemColors.Control;
            mainPanel.Controls.Add(glControl);
            mainPanel.Location = new Point(72, 116);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(546, 525);
            mainPanel.TabIndex = 3;
            // 
            // glControl
            // 
            glControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            glControl.API = OpenTK.Windowing.Common.ContextAPI.OpenGL;
            glControl.APIVersion = new System.Version(3, 3, 0, 0);
            glControl.BackColor = SystemColors.HotTrack;
            glControl.Flags = OpenTK.Windowing.Common.ContextFlags.Default;
            glControl.IsEventDriven = true;
            glControl.Location = new Point(73, 62);
            glControl.Margin = new Padding(3, 4, 3, 4);
            glControl.Name = "glControl";
            glControl.Profile = OpenTK.Windowing.Common.ContextProfile.Compatability;
            glControl.SharedContext = null;
            glControl.Size = new Size(400, 400);
            glControl.TabIndex = 3;
            glControl.Text = "glControl1";
            glControl.PreviewKeyDown += glControl_PreviewKeyDown;
            // 
            // Offsetter
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.WindowFrame;
            ClientSize = new Size(1048, 1063);
            Controls.Add(mainPanel);
            Controls.Add(menuStrip1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
            Name = "Offsetter";
            Text = "2D Offsetter";
            FormClosing += Offsetter_FormClosing;
            FormClosed += Offsetter_FormClosed;
            Load += Form1_Load;
            LocationChanged += Offsetter_LocationChanged;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            viewPopMenu.ResumeLayout(false);
            mainPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileMenu;
        private ToolStripMenuItem openFileMenuItem;
        private ToolStripMenuItem geometryMenu;
        private ToolStripMenuItem uniformOffsetGeometryMenuItem;
        private ToolStripMenuItem saveAsContextMenuItem;
        private ToolStripMenuItem saveResultsFileMenuItem;
        private ToolStripMenuItem reopenFileMenuItem;
        private ToolStripMenuItem reorderGeometryMenuItem;
        private ToolStripSeparator fileMenuSeparator1;
        private ToolStripMenuItem testFileMenuItem;
        private ToolStripMenuItem nonUniformOffsetGeometryMenuItem;
        private ToolStripMenuItem decomposeGeometryMenuItem;
        private ToolStripMenuItem nestGeometryMenuItem;
        private System.Windows.Forms.ContextMenuStrip viewPopMenu;
        private System.Windows.Forms.ToolStripMenuItem panContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zoomContextMenuItem;
        private System.Windows.Forms.ToolTip toolTip;
        private ToolStripSeparator contextMenuSeparator1;
        private ToolStripMenuItem fullViewContextMenuItem;
        private ToolStripMenuItem previousViewContextMenuItem;
        private ToolStripSeparator contextMenuSeparator2;
        private Panel mainPanel;
        private GLControl glControl;
        private ToolStripMenuItem viewMenu;
        private ToolStripMenuItem panViewMenuItem;
        private ToolStripMenuItem windowViewMenuItem;
        private ToolStripMenuItem zoomViewMenuItem;
        private ToolStripSeparator viewMenuSeparator1;
        private ToolStripMenuItem fullViewMenuItem;
        private ToolStripMenuItem previousViewMenuItem;
        private ToolStripSeparator geometryMenuSeparator1;
        private ToolStripSeparator geometryMenuSeparator2;
        private ToolStripMenuItem propertiesGeometryMenuItem;
        private ToolStripMenuItem propertiesContextMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem animateToolStripMenuItem;
    }
}
