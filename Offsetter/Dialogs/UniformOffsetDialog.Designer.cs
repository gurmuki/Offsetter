using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    partial class UniformOffsetDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            offsetLabel = new Label();
            offset = new TextBox();
            accept = new Button();
            left = new RadioButton();
            right = new RadioButton();
            uniformPanel = new Panel();
            chainsLabel = new Label();
            chainIDs = new TextBox();
            offsetSidePanel = new Panel();
            close = new Button();
            uniformPanel.SuspendLayout();
            offsetSidePanel.SuspendLayout();
            SuspendLayout();
            // 
            // offsetLabel
            // 
            offsetLabel.AutoSize = true;
            offsetLabel.Location = new Point(11, 38);
            offsetLabel.Name = "offsetLabel";
            offsetLabel.Size = new Size(65, 15);
            offsetLabel.TabIndex = 3;
            offsetLabel.Text = "Offset Dist:";
            // 
            // offset
            // 
            offset.Location = new Point(78, 34);
            offset.Name = "offset";
            offset.Size = new Size(100, 23);
            offset.TabIndex = 4;
            offset.Enter += offset_Enter;
            // 
            // accept
            // 
            accept.Location = new Point(108, 107);
            accept.Name = "accept";
            accept.Size = new Size(75, 23);
            accept.TabIndex = 6;
            accept.Text = "Accept";
            accept.UseVisualStyleBackColor = true;
            accept.Click += accept_Click;
            // 
            // left
            // 
            left.AutoSize = true;
            left.Location = new Point(42, 6);
            left.Name = "left";
            left.Size = new Size(56, 19);
            left.TabIndex = 0;
            left.TabStop = true;
            left.Text = "Inside";
            left.UseVisualStyleBackColor = true;
            left.CheckedChanged += left_CheckedChanged;
            // 
            // right
            // 
            right.AutoSize = true;
            right.Location = new Point(108, 6);
            right.Name = "right";
            right.Size = new Size(66, 19);
            right.TabIndex = 1;
            right.TabStop = true;
            right.Text = "Outside";
            right.UseVisualStyleBackColor = true;
            // 
            // uniformPanel
            // 
            uniformPanel.Controls.Add(chainsLabel);
            uniformPanel.Controls.Add(chainIDs);
            uniformPanel.Controls.Add(offsetLabel);
            uniformPanel.Controls.Add(offset);
            uniformPanel.Location = new Point(0, 5);
            uniformPanel.Name = "uniformPanel";
            uniformPanel.Size = new Size(200, 63);
            uniformPanel.TabIndex = 0;
            // 
            // chainsLabel
            // 
            chainsLabel.AutoSize = true;
            chainsLabel.Location = new Point(30, 9);
            chainsLabel.Name = "chainsLabel";
            chainsLabel.Size = new Size(46, 15);
            chainsLabel.TabIndex = 1;
            chainsLabel.Text = "Chains:";
            // 
            // chainIDs
            // 
            chainIDs.Location = new Point(78, 5);
            chainIDs.Name = "chainIDs";
            chainIDs.ReadOnly = true;
            chainIDs.Size = new Size(100, 23);
            chainIDs.TabIndex = 2;
            chainIDs.Enter += chainIDs_Enter;
            // 
            // offsetSidePanel
            // 
            offsetSidePanel.Controls.Add(right);
            offsetSidePanel.Controls.Add(left);
            offsetSidePanel.Location = new Point(0, 68);
            offsetSidePanel.Name = "offsetSidePanel";
            offsetSidePanel.Size = new Size(200, 33);
            offsetSidePanel.TabIndex = 5;
            // 
            // close
            // 
            close.Location = new Point(27, 107);
            close.Name = "close";
            close.Size = new Size(75, 23);
            close.TabIndex = 7;
            close.Text = "Close";
            close.UseVisualStyleBackColor = true;
            close.Click += close_Click;
            // 
            // UniformOffsetDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(200, 140);
            Controls.Add(close);
            Controls.Add(offsetSidePanel);
            Controls.Add(uniformPanel);
            Controls.Add(accept);
            Name = "UniformOffsetDialog";
            StartPosition = FormStartPosition.Manual;
            Text = "Offset";
            Load += UniformOffsetDialog_Load;
            uniformPanel.ResumeLayout(false);
            uniformPanel.PerformLayout();
            offsetSidePanel.ResumeLayout(false);
            offsetSidePanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Label offsetLabel;
        private TextBox offset;
        private Button accept;
        private RadioButton left;
        private RadioButton right;
        private Panel uniformPanel;
        private Panel offsetSidePanel;
        private Label chainsLabel;
        private TextBox chainIDs;
        private Button close;
    }
}
