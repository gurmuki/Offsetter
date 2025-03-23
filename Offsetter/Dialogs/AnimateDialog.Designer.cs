namespace Offsetter.Dialogs
{
    partial class AnimateDialog
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
            label1 = new System.Windows.Forms.Label();
            pathID = new System.Windows.Forms.TextBox();
            action = new System.Windows.Forms.Button();
            close = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(10, 16);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(34, 15);
            label1.TabIndex = 0;
            label1.Text = "Path:";
            // 
            // pathID
            // 
            pathID.Location = new System.Drawing.Point(46, 12);
            pathID.Name = "pathID";
            pathID.ReadOnly = true;
            pathID.Size = new System.Drawing.Size(100, 23);
            pathID.TabIndex = 1;
            // 
            // action
            // 
            action.Location = new System.Drawing.Point(152, 12);
            action.Name = "action";
            action.Size = new System.Drawing.Size(75, 23);
            action.TabIndex = 2;
            action.Text = "<action>";
            action.UseVisualStyleBackColor = true;
            action.Click += action_Click;
            // 
            // close
            // 
            close.Location = new System.Drawing.Point(233, 12);
            close.Name = "close";
            close.Size = new System.Drawing.Size(75, 23);
            close.TabIndex = 3;
            close.Text = "close";
            close.UseVisualStyleBackColor = true;
            close.Click += close_Click;
            // 
            // AnimateDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(323, 44);
            Controls.Add(close);
            Controls.Add(action);
            Controls.Add(pathID);
            Controls.Add(label1);
            Name = "AnimateDialog";
            Text = "Animate";
            FormClosing += AnimateDialog_FormClosing;
            Load += AnimateDialog_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox pathID;
        private System.Windows.Forms.Button action;
        private System.Windows.Forms.Button close;
    }
}