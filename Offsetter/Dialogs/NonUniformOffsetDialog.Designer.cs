using System.Drawing;
using System.Windows.Forms;

namespace Offsetter
{
    partial class NonUniformOffsetDialog
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
            accept = new Button();
            left = new RadioButton();
            right = new RadioButton();
            nonUniformPanel = new Panel();
            toolLabel = new Label();
            toolID = new TextBox();
            shapeLabel = new Label();
            shapeID = new TextBox();
            offsetSidePanel = new Panel();
            close = new Button();
            nonUniformPanel.SuspendLayout();
            offsetSidePanel.SuspendLayout();
            SuspendLayout();
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
            // nonUniformPanel
            // 
            nonUniformPanel.Controls.Add(toolLabel);
            nonUniformPanel.Controls.Add(toolID);
            nonUniformPanel.Controls.Add(shapeLabel);
            nonUniformPanel.Controls.Add(shapeID);
            nonUniformPanel.Location = new Point(0, 5);
            nonUniformPanel.Name = "nonUniformPanel";
            nonUniformPanel.Size = new Size(200, 63);
            nonUniformPanel.TabIndex = 0;
            // 
            // toolLabel
            // 
            toolLabel.AutoSize = true;
            toolLabel.Location = new Point(37, 38);
            toolLabel.Name = "toolLabel";
            toolLabel.Size = new Size(32, 15);
            toolLabel.TabIndex = 3;
            toolLabel.Text = "Tool:";
            // 
            // toolID
            // 
            toolID.Location = new Point(71, 34);
            toolID.Name = "toolID";
            toolID.ReadOnly = true;
            toolID.Size = new Size(100, 23);
            toolID.TabIndex = 4;
            toolID.Enter += toolID_Enter;
            // 
            // shapeLabel
            // 
            shapeLabel.AutoSize = true;
            shapeLabel.Location = new Point(27, 9);
            shapeLabel.Name = "shapeLabel";
            shapeLabel.Size = new Size(42, 15);
            shapeLabel.TabIndex = 1;
            shapeLabel.Text = "Shape:";
            // 
            // shapeID
            // 
            shapeID.Location = new Point(71, 5);
            shapeID.Name = "shapeID";
            shapeID.ReadOnly = true;
            shapeID.Size = new Size(100, 23);
            shapeID.TabIndex = 2;
            shapeID.Enter += shapeID_Enter;
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
            // NonUniformOffsetDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(200, 140);
            Controls.Add(close);
            Controls.Add(offsetSidePanel);
            Controls.Add(nonUniformPanel);
            Controls.Add(accept);
            Name = "NonUniformOffsetDialog";
            StartPosition = FormStartPosition.Manual;
            Text = "Offset";
            Load += NonUniformOffsetDialog_Load;
            nonUniformPanel.ResumeLayout(false);
            nonUniformPanel.PerformLayout();
            offsetSidePanel.ResumeLayout(false);
            offsetSidePanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Button accept;
        private RadioButton left;
        private RadioButton right;
        private Panel nonUniformPanel;
        private Panel offsetSidePanel;
        private Label shapeLabel;
        private TextBox shapeID;
        private Label toolLabel;
        private TextBox toolID;
        private Button close;
    }
}
