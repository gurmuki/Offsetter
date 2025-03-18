using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    partial class OffsetDialog
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
            chains = new TextBox();
            nonUniformPanel = new Panel();
            toolLabel = new Label();
            tool = new TextBox();
            shapeLabel = new Label();
            textBox1 = new TextBox();
            offsetSidePanel = new Panel();
            cancel = new Button();
            uniformPanel.SuspendLayout();
            nonUniformPanel.SuspendLayout();
            offsetSidePanel.SuspendLayout();
            SuspendLayout();
            // 
            // offsetLabel
            // 
            offsetLabel.AutoSize = true;
            offsetLabel.Location = new Point(11, 38);
            offsetLabel.Name = "offsetLabel";
            offsetLabel.Size = new Size(65, 15);
            offsetLabel.TabIndex = 1;
            offsetLabel.Text = "Offset Dist:";
            // 
            // offset
            // 
            offset.Location = new Point(78, 34);
            offset.Name = "offset";
            offset.Size = new Size(100, 23);
            offset.TabIndex = 3;
            // 
            // accept
            // 
            accept.DialogResult = DialogResult.OK;
            accept.Location = new Point(108, 107);
            accept.Name = "accept";
            accept.Size = new Size(75, 23);
            accept.TabIndex = 4;
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
            right.CheckedChanged += right_CheckedChanged;
            // 
            // uniformPanel
            // 
            uniformPanel.Controls.Add(chainsLabel);
            uniformPanel.Controls.Add(chains);
            uniformPanel.Controls.Add(offsetLabel);
            uniformPanel.Controls.Add(offset);
            uniformPanel.Location = new Point(0, 5);
            uniformPanel.Name = "uniformPanel";
            uniformPanel.Size = new Size(200, 63);
            uniformPanel.TabIndex = 6;
            // 
            // chainsLabel
            // 
            chainsLabel.AutoSize = true;
            chainsLabel.Location = new Point(30, 9);
            chainsLabel.Name = "chainsLabel";
            chainsLabel.Size = new Size(46, 15);
            chainsLabel.TabIndex = 4;
            chainsLabel.Text = "Chains:";
            // 
            // chains
            // 
            chains.Location = new Point(78, 5);
            chains.Name = "chains";
            chains.Size = new Size(100, 23);
            chains.TabIndex = 5;
            // 
            // nonUniformPanel
            // 
            nonUniformPanel.Controls.Add(toolLabel);
            nonUniformPanel.Controls.Add(tool);
            nonUniformPanel.Controls.Add(shapeLabel);
            nonUniformPanel.Controls.Add(textBox1);
            nonUniformPanel.Location = new Point(216, 14);
            nonUniformPanel.Name = "nonUniformPanel";
            nonUniformPanel.Size = new Size(200, 63);
            nonUniformPanel.TabIndex = 7;
            // 
            // toolLabel
            // 
            toolLabel.AutoSize = true;
            toolLabel.Location = new Point(37, 38);
            toolLabel.Name = "toolLabel";
            toolLabel.Size = new Size(32, 15);
            toolLabel.TabIndex = 8;
            toolLabel.Text = "Tool:";
            // 
            // tool
            // 
            tool.Location = new Point(71, 34);
            tool.Name = "tool";
            tool.Size = new Size(100, 23);
            tool.TabIndex = 9;
            // 
            // shapeLabel
            // 
            shapeLabel.AutoSize = true;
            shapeLabel.Location = new Point(27, 9);
            shapeLabel.Name = "shapeLabel";
            shapeLabel.Size = new Size(42, 15);
            shapeLabel.TabIndex = 6;
            shapeLabel.Text = "Shape:";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(71, 5);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(100, 23);
            textBox1.TabIndex = 7;
            // 
            // offsetSidePanel
            // 
            offsetSidePanel.Controls.Add(right);
            offsetSidePanel.Controls.Add(left);
            offsetSidePanel.Location = new Point(0, 68);
            offsetSidePanel.Name = "offsetSidePanel";
            offsetSidePanel.Size = new Size(200, 33);
            offsetSidePanel.TabIndex = 8;
            // 
            // cancel
            // 
            cancel.DialogResult = DialogResult.Cancel;
            cancel.Location = new Point(27, 107);
            cancel.Name = "cancel";
            cancel.Size = new Size(75, 23);
            cancel.TabIndex = 9;
            cancel.Text = "Cancel";
            cancel.UseVisualStyleBackColor = true;
            // 
            // OffsetDialog
            // 
            AcceptButton = accept;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancel;
            ClientSize = new Size(425, 140);
            Controls.Add(cancel);
            Controls.Add(offsetSidePanel);
            Controls.Add(nonUniformPanel);
            Controls.Add(uniformPanel);
            Controls.Add(accept);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "OffsetDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Offset";
            Load += OffsetDialog_Load;
            uniformPanel.ResumeLayout(false);
            uniformPanel.PerformLayout();
            nonUniformPanel.ResumeLayout(false);
            nonUniformPanel.PerformLayout();
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
        private Panel nonUniformPanel;
        private Panel offsetSidePanel;
        private Label chainsLabel;
        private TextBox chains;
        private Label shapeLabel;
        private TextBox textBox1;
        private Label toolLabel;
        private TextBox tool;
        private Button cancel;
    }
}