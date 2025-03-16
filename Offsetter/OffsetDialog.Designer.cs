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
            label = new Label();
            offset = new TextBox();
            ok = new Button();
            left = new RadioButton();
            right = new RadioButton();
            groupBox = new GroupBox();
            groupBox.SuspendLayout();
            SuspendLayout();
            // 
            // label
            // 
            label.AutoSize = true;
            label.Location = new Point(33, 73);
            label.Name = "label";
            label.Size = new Size(65, 15);
            label.TabIndex = 1;
            label.Text = "Offset Dist:";
            // 
            // offset
            // 
            offset.Location = new Point(104, 70);
            offset.Name = "offset";
            offset.Size = new Size(100, 23);
            offset.TabIndex = 3;
            // 
            // ok
            // 
            ok.DialogResult = DialogResult.OK;
            ok.Location = new Point(150, 99);
            ok.Name = "ok";
            ok.Size = new Size(75, 23);
            ok.TabIndex = 4;
            ok.Text = "OK";
            ok.UseVisualStyleBackColor = true;
            ok.Click += ok_Click;
            // 
            // left
            // 
            left.AutoSize = true;
            left.Location = new Point(31, 17);
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
            right.Location = new Point(106, 17);
            right.Name = "right";
            right.Size = new Size(66, 19);
            right.TabIndex = 1;
            right.TabStop = true;
            right.Text = "Outside";
            right.UseVisualStyleBackColor = true;
            right.CheckedChanged += right_CheckedChanged;
            // 
            // groupBox
            // 
            groupBox.Controls.Add(right);
            groupBox.Controls.Add(left);
            groupBox.Location = new Point(33, 12);
            groupBox.Name = "groupBox";
            groupBox.Size = new Size(192, 47);
            groupBox.TabIndex = 5;
            groupBox.TabStop = false;
            // 
            // OffsetDialog
            // 
            AcceptButton = ok;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(246, 135);
            Controls.Add(groupBox);
            Controls.Add(ok);
            Controls.Add(offset);
            Controls.Add(label);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "OffsetDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Offset";
            Load += OffsetDialog_Load;
            groupBox.ResumeLayout(false);
            groupBox.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label;
        private TextBox offset;
        private Button ok;
        private RadioButton left;
        private RadioButton right;
        private GroupBox groupBox;
    }
}