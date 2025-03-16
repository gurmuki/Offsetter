namespace Offsetter
{
    partial class Canonical
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
            xs = new System.Windows.Forms.TextBox();
            ys = new System.Windows.Forms.TextBox();
            type = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            ye = new System.Windows.Forms.TextBox();
            xe = new System.Windows.Forms.TextBox();
            arcProperties = new System.Windows.Forms.Panel();
            panel1 = new System.Windows.Forms.Panel();
            degrees = new System.Windows.Forms.RadioButton();
            radians = new System.Windows.Forms.RadioButton();
            label10 = new System.Windows.Forms.Label();
            label11 = new System.Windows.Forms.Label();
            ea = new System.Windows.Forms.TextBox();
            sa = new System.Windows.Forms.TextBox();
            label8 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            dir = new System.Windows.Forms.TextBox();
            rad = new System.Windows.Forms.TextBox();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            yc = new System.Windows.Forms.TextBox();
            xc = new System.Windows.Forms.TextBox();
            arcProperties.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // xs
            // 
            xs.Location = new System.Drawing.Point(44, 41);
            xs.Name = "xs";
            xs.ReadOnly = true;
            xs.Size = new System.Drawing.Size(100, 23);
            xs.TabIndex = 3;
            // 
            // ys
            // 
            ys.Location = new System.Drawing.Point(183, 41);
            ys.Name = "ys";
            ys.ReadOnly = true;
            ys.Size = new System.Drawing.Size(100, 23);
            ys.TabIndex = 5;
            // 
            // type
            // 
            type.Location = new System.Drawing.Point(44, 12);
            type.Name = "type";
            type.ReadOnly = true;
            type.Size = new System.Drawing.Size(100, 23);
            type.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(11, 16);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(30, 15);
            label1.TabIndex = 0;
            label1.Text = "type";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(24, 45);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(18, 15);
            label2.TabIndex = 2;
            label2.Text = "xs";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(163, 45);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(18, 15);
            label3.TabIndex = 4;
            label3.Text = "ys";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(163, 74);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(19, 15);
            label4.TabIndex = 8;
            label4.Text = "ye";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(24, 74);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(19, 15);
            label5.TabIndex = 6;
            label5.Text = "xe";
            // 
            // ye
            // 
            ye.Location = new System.Drawing.Point(183, 70);
            ye.Name = "ye";
            ye.ReadOnly = true;
            ye.Size = new System.Drawing.Size(100, 23);
            ye.TabIndex = 9;
            // 
            // xe
            // 
            xe.Location = new System.Drawing.Point(44, 70);
            xe.Name = "xe";
            xe.ReadOnly = true;
            xe.Size = new System.Drawing.Size(100, 23);
            xe.TabIndex = 7;
            // 
            // arcProperties
            // 
            arcProperties.Controls.Add(panel1);
            arcProperties.Controls.Add(label10);
            arcProperties.Controls.Add(label11);
            arcProperties.Controls.Add(ea);
            arcProperties.Controls.Add(sa);
            arcProperties.Controls.Add(label8);
            arcProperties.Controls.Add(label9);
            arcProperties.Controls.Add(dir);
            arcProperties.Controls.Add(rad);
            arcProperties.Controls.Add(label6);
            arcProperties.Controls.Add(label7);
            arcProperties.Controls.Add(yc);
            arcProperties.Controls.Add(xc);
            arcProperties.Location = new System.Drawing.Point(5, 99);
            arcProperties.Name = "arcProperties";
            arcProperties.Size = new System.Drawing.Size(297, 130);
            arcProperties.TabIndex = 11;
            // 
            // panel1
            // 
            panel1.Controls.Add(degrees);
            panel1.Controls.Add(radians);
            panel1.Location = new System.Drawing.Point(39, 90);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(239, 36);
            panel1.TabIndex = 24;
            // 
            // degrees
            // 
            degrees.AutoSize = true;
            degrees.Location = new System.Drawing.Point(121, 9);
            degrees.Name = "degrees";
            degrees.Size = new System.Drawing.Size(66, 19);
            degrees.TabIndex = 1;
            degrees.TabStop = true;
            degrees.Text = "degrees";
            degrees.UseVisualStyleBackColor = true;
            // 
            // radians
            // 
            radians.AutoSize = true;
            radians.Location = new System.Drawing.Point(41, 9);
            radians.Name = "radians";
            radians.Size = new System.Drawing.Size(63, 19);
            radians.TabIndex = 0;
            radians.TabStop = true;
            radians.Text = "radians";
            radians.UseVisualStyleBackColor = true;
            radians.CheckedChanged += radians_CheckedChanged;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(157, 65);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(19, 15);
            label10.TabIndex = 22;
            label10.Text = "ae";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(19, 65);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(18, 15);
            label11.TabIndex = 20;
            label11.Text = "as";
            // 
            // ea
            // 
            ea.Location = new System.Drawing.Point(178, 61);
            ea.Name = "ea";
            ea.ReadOnly = true;
            ea.Size = new System.Drawing.Size(100, 23);
            ea.TabIndex = 23;
            // 
            // sa
            // 
            sa.Location = new System.Drawing.Point(39, 61);
            sa.Name = "sa";
            sa.ReadOnly = true;
            sa.Size = new System.Drawing.Size(100, 23);
            sa.TabIndex = 21;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(155, 36);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(21, 15);
            label8.TabIndex = 18;
            label8.Text = "dir";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(13, 36);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(24, 15);
            label9.TabIndex = 16;
            label9.Text = "rad";
            // 
            // dir
            // 
            dir.Location = new System.Drawing.Point(178, 32);
            dir.Name = "dir";
            dir.ReadOnly = true;
            dir.Size = new System.Drawing.Size(100, 23);
            dir.TabIndex = 19;
            // 
            // rad
            // 
            rad.Location = new System.Drawing.Point(39, 32);
            rad.Name = "rad";
            rad.ReadOnly = true;
            rad.Size = new System.Drawing.Size(100, 23);
            rad.TabIndex = 17;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(158, 7);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(19, 15);
            label6.TabIndex = 14;
            label6.Text = "yc";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(19, 7);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(19, 15);
            label7.TabIndex = 12;
            label7.Text = "xc";
            // 
            // yc
            // 
            yc.Location = new System.Drawing.Point(178, 3);
            yc.Name = "yc";
            yc.ReadOnly = true;
            yc.Size = new System.Drawing.Size(100, 23);
            yc.TabIndex = 15;
            // 
            // xc
            // 
            xc.Location = new System.Drawing.Point(39, 3);
            xc.Name = "xc";
            xc.ReadOnly = true;
            xc.Size = new System.Drawing.Size(100, 23);
            xc.TabIndex = 13;
            // 
            // Canonical
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(309, 232);
            Controls.Add(arcProperties);
            Controls.Add(label4);
            Controls.Add(label5);
            Controls.Add(ye);
            Controls.Add(xe);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(type);
            Controls.Add(ys);
            Controls.Add(xs);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Name = "Canonical";
            ShowInTaskbar = false;
            Text = "Properties";
            FormClosing += Canonical_FormClosing;
            arcProperties.ResumeLayout(false);
            arcProperties.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox xs;
        private System.Windows.Forms.TextBox ys;
        private System.Windows.Forms.TextBox type;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox ye;
        private System.Windows.Forms.TextBox xe;
        private System.Windows.Forms.Panel arcProperties;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton degrees;
        private System.Windows.Forms.RadioButton radians;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox ea;
        private System.Windows.Forms.TextBox sa;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox dir;
        private System.Windows.Forms.TextBox rad;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox yc;
        private System.Windows.Forms.TextBox xc;
    }
}