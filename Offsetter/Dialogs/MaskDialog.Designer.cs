namespace Offsetter.Dialogs
{
    partial class MaskDialog
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
            Input = new System.Windows.Forms.CheckBox();
            Path = new System.Windows.Forms.CheckBox();
            Intermediate = new System.Windows.Forms.CheckBox();
            SuspendLayout();
            // 
            // Input
            // 
            Input.AutoSize = true;
            Input.Location = new System.Drawing.Point(12, 12);
            Input.Name = "Input";
            Input.Size = new System.Drawing.Size(54, 19);
            Input.TabIndex = 0;
            Input.Text = "Input";
            Input.UseVisualStyleBackColor = true;
            Input.CheckedChanged += Input_CheckedChanged;
            // 
            // Path
            // 
            Path.AutoSize = true;
            Path.Location = new System.Drawing.Point(72, 12);
            Path.Name = "Path";
            Path.Size = new System.Drawing.Size(50, 19);
            Path.TabIndex = 1;
            Path.Text = "Path";
            Path.UseVisualStyleBackColor = true;
            Path.CheckedChanged += Path_CheckedChanged;
            // 
            // Intermediate
            // 
            Intermediate.AutoSize = true;
            Intermediate.Location = new System.Drawing.Point(128, 12);
            Intermediate.Name = "Intermediate";
            Intermediate.Size = new System.Drawing.Size(93, 19);
            Intermediate.TabIndex = 2;
            Intermediate.Text = "Intermediate";
            Intermediate.UseVisualStyleBackColor = true;
            Intermediate.CheckedChanged += Intermediate_CheckedChanged;
            // 
            // MaskDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(221, 41);
            Controls.Add(Intermediate);
            Controls.Add(Path);
            Controls.Add(Input);
            Name = "MaskDialog";
            StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            Text = "Mask";
            Load += MaskDialog_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.CheckBox Input;
        private System.Windows.Forms.CheckBox Path;
        private System.Windows.Forms.CheckBox Intermediate;
    }
}