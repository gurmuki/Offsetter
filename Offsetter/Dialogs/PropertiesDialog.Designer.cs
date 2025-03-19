namespace Offsetter
{
    partial class PropertiesDialog
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
            endptProperties = new System.Windows.Forms.Panel();
            xsLabel = new System.Windows.Forms.Label();
            xs = new System.Windows.Forms.TextBox();
            ysLabel = new System.Windows.Forms.Label();
            ys = new System.Windows.Forms.TextBox();
            xeLabel = new System.Windows.Forms.Label();
            xe = new System.Windows.Forms.TextBox();
            yeLabel = new System.Windows.Forms.Label();
            ye = new System.Windows.Forms.TextBox();
            degradPanel = new System.Windows.Forms.Panel();
            degrees = new System.Windows.Forms.RadioButton();
            radians = new System.Windows.Forms.RadioButton();
            type = new System.Windows.Forms.TextBox();
            typeLabel = new System.Windows.Forms.Label();
            arcProperties = new System.Windows.Forms.Panel();
            eaLabel = new System.Windows.Forms.Label();
            saLabel = new System.Windows.Forms.Label();
            ea = new System.Windows.Forms.TextBox();
            sa = new System.Windows.Forms.TextBox();
            dirLabel = new System.Windows.Forms.Label();
            radLabel = new System.Windows.Forms.Label();
            dir = new System.Windows.Forms.TextBox();
            rad = new System.Windows.Forms.TextBox();
            ycLabel = new System.Windows.Forms.Label();
            xcLabel = new System.Windows.Forms.Label();
            yc = new System.Windows.Forms.TextBox();
            xc = new System.Windows.Forms.TextBox();
            idLabel = new System.Windows.Forms.Label();
            id = new System.Windows.Forms.TextBox();
            close = new System.Windows.Forms.Button();
            endptProperties.SuspendLayout();
            degradPanel.SuspendLayout();
            arcProperties.SuspendLayout();
            SuspendLayout();
            // 
            // endptProperties
            // 
            endptProperties.Controls.Add(xsLabel);
            endptProperties.Controls.Add(xs);
            endptProperties.Controls.Add(ysLabel);
            endptProperties.Controls.Add(ys);
            endptProperties.Controls.Add(xeLabel);
            endptProperties.Controls.Add(xe);
            endptProperties.Controls.Add(yeLabel);
            endptProperties.Controls.Add(ye);
            endptProperties.Location = new System.Drawing.Point(5, 39);
            endptProperties.Name = "endptProperties";
            endptProperties.Size = new System.Drawing.Size(294, 63);
            endptProperties.TabIndex = 4;
            // 
            // xsLabel
            // 
            xsLabel.AutoSize = true;
            xsLabel.Location = new System.Drawing.Point(19, 9);
            xsLabel.Name = "xsLabel";
            xsLabel.Size = new System.Drawing.Size(18, 15);
            xsLabel.TabIndex = 5;
            xsLabel.Text = "xs";
            // 
            // xs
            // 
            xs.Location = new System.Drawing.Point(39, 5);
            xs.Name = "xs";
            xs.ReadOnly = true;
            xs.Size = new System.Drawing.Size(100, 23);
            xs.TabIndex = 6;
            // 
            // ysLabel
            // 
            ysLabel.AutoSize = true;
            ysLabel.Location = new System.Drawing.Point(158, 9);
            ysLabel.Name = "ysLabel";
            ysLabel.Size = new System.Drawing.Size(18, 15);
            ysLabel.TabIndex = 7;
            ysLabel.Text = "ys";
            // 
            // ys
            // 
            ys.Location = new System.Drawing.Point(178, 5);
            ys.Name = "ys";
            ys.ReadOnly = true;
            ys.Size = new System.Drawing.Size(100, 23);
            ys.TabIndex = 8;
            // 
            // xeLabel
            // 
            xeLabel.AutoSize = true;
            xeLabel.Location = new System.Drawing.Point(19, 38);
            xeLabel.Name = "xeLabel";
            xeLabel.Size = new System.Drawing.Size(19, 15);
            xeLabel.TabIndex = 9;
            xeLabel.Text = "xe";
            // 
            // xe
            // 
            xe.Location = new System.Drawing.Point(39, 34);
            xe.Name = "xe";
            xe.ReadOnly = true;
            xe.Size = new System.Drawing.Size(100, 23);
            xe.TabIndex = 10;
            // 
            // yeLabel
            // 
            yeLabel.AutoSize = true;
            yeLabel.Location = new System.Drawing.Point(158, 38);
            yeLabel.Name = "yeLabel";
            yeLabel.Size = new System.Drawing.Size(19, 15);
            yeLabel.TabIndex = 11;
            yeLabel.Text = "ye";
            // 
            // ye
            // 
            ye.Location = new System.Drawing.Point(178, 34);
            ye.Name = "ye";
            ye.ReadOnly = true;
            ye.Size = new System.Drawing.Size(100, 23);
            ye.TabIndex = 12;
            // 
            // degradPanel
            // 
            degradPanel.Controls.Add(degrees);
            degradPanel.Controls.Add(radians);
            degradPanel.Location = new System.Drawing.Point(39, 90);
            degradPanel.Name = "degradPanel";
            degradPanel.Size = new System.Drawing.Size(239, 36);
            degradPanel.TabIndex = 26;
            // 
            // degrees
            // 
            degrees.AutoSize = true;
            degrees.Location = new System.Drawing.Point(121, 9);
            degrees.Name = "degrees";
            degrees.Size = new System.Drawing.Size(66, 19);
            degrees.TabIndex = 27;
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
            radians.TabIndex = 28;
            radians.TabStop = true;
            radians.Text = "radians";
            radians.UseVisualStyleBackColor = true;
            radians.CheckedChanged += radians_CheckedChanged;
            // 
            // type
            // 
            type.Location = new System.Drawing.Point(183, 12);
            type.Name = "type";
            type.ReadOnly = true;
            type.Size = new System.Drawing.Size(100, 23);
            type.TabIndex = 3;
            // 
            // typeLabel
            // 
            typeLabel.AutoSize = true;
            typeLabel.Location = new System.Drawing.Point(150, 16);
            typeLabel.Name = "typeLabel";
            typeLabel.Size = new System.Drawing.Size(30, 15);
            typeLabel.TabIndex = 2;
            typeLabel.Text = "type";
            // 
            // arcProperties
            // 
            arcProperties.Controls.Add(degradPanel);
            arcProperties.Controls.Add(eaLabel);
            arcProperties.Controls.Add(saLabel);
            arcProperties.Controls.Add(ea);
            arcProperties.Controls.Add(sa);
            arcProperties.Controls.Add(dirLabel);
            arcProperties.Controls.Add(radLabel);
            arcProperties.Controls.Add(dir);
            arcProperties.Controls.Add(rad);
            arcProperties.Controls.Add(ycLabel);
            arcProperties.Controls.Add(xcLabel);
            arcProperties.Controls.Add(yc);
            arcProperties.Controls.Add(xc);
            arcProperties.Location = new System.Drawing.Point(5, 102);
            arcProperties.Name = "arcProperties";
            arcProperties.Size = new System.Drawing.Size(294, 130);
            arcProperties.TabIndex = 13;
            // 
            // eaLabel
            // 
            eaLabel.AutoSize = true;
            eaLabel.Location = new System.Drawing.Point(157, 65);
            eaLabel.Name = "eaLabel";
            eaLabel.Size = new System.Drawing.Size(19, 15);
            eaLabel.TabIndex = 24;
            eaLabel.Text = "ae";
            // 
            // saLabel
            // 
            saLabel.AutoSize = true;
            saLabel.Location = new System.Drawing.Point(19, 65);
            saLabel.Name = "saLabel";
            saLabel.Size = new System.Drawing.Size(18, 15);
            saLabel.TabIndex = 22;
            saLabel.Text = "as";
            // 
            // ea
            // 
            ea.Location = new System.Drawing.Point(178, 61);
            ea.Name = "ea";
            ea.ReadOnly = true;
            ea.Size = new System.Drawing.Size(100, 23);
            ea.TabIndex = 25;
            // 
            // sa
            // 
            sa.Location = new System.Drawing.Point(39, 61);
            sa.Name = "sa";
            sa.ReadOnly = true;
            sa.Size = new System.Drawing.Size(100, 23);
            sa.TabIndex = 23;
            // 
            // dirLabel
            // 
            dirLabel.AutoSize = true;
            dirLabel.Location = new System.Drawing.Point(155, 36);
            dirLabel.Name = "dirLabel";
            dirLabel.Size = new System.Drawing.Size(21, 15);
            dirLabel.TabIndex = 20;
            dirLabel.Text = "dir";
            // 
            // radLabel
            // 
            radLabel.AutoSize = true;
            radLabel.Location = new System.Drawing.Point(13, 36);
            radLabel.Name = "radLabel";
            radLabel.Size = new System.Drawing.Size(24, 15);
            radLabel.TabIndex = 18;
            radLabel.Text = "rad";
            // 
            // dir
            // 
            dir.Location = new System.Drawing.Point(178, 32);
            dir.Name = "dir";
            dir.ReadOnly = true;
            dir.Size = new System.Drawing.Size(100, 23);
            dir.TabIndex = 21;
            // 
            // rad
            // 
            rad.Location = new System.Drawing.Point(39, 32);
            rad.Name = "rad";
            rad.ReadOnly = true;
            rad.Size = new System.Drawing.Size(100, 23);
            rad.TabIndex = 19;
            // 
            // ycLabel
            // 
            ycLabel.AutoSize = true;
            ycLabel.Location = new System.Drawing.Point(158, 7);
            ycLabel.Name = "ycLabel";
            ycLabel.Size = new System.Drawing.Size(19, 15);
            ycLabel.TabIndex = 16;
            ycLabel.Text = "yc";
            // 
            // xcLabel
            // 
            xcLabel.AutoSize = true;
            xcLabel.Location = new System.Drawing.Point(19, 7);
            xcLabel.Name = "xcLabel";
            xcLabel.Size = new System.Drawing.Size(19, 15);
            xcLabel.TabIndex = 14;
            xcLabel.Text = "xc";
            // 
            // yc
            // 
            yc.Location = new System.Drawing.Point(178, 3);
            yc.Name = "yc";
            yc.ReadOnly = true;
            yc.Size = new System.Drawing.Size(100, 23);
            yc.TabIndex = 17;
            // 
            // xc
            // 
            xc.Location = new System.Drawing.Point(39, 3);
            xc.Name = "xc";
            xc.ReadOnly = true;
            xc.Size = new System.Drawing.Size(100, 23);
            xc.TabIndex = 15;
            // 
            // idLabel
            // 
            idLabel.AutoSize = true;
            idLabel.Location = new System.Drawing.Point(24, 16);
            idLabel.Name = "idLabel";
            idLabel.Size = new System.Drawing.Size(18, 15);
            idLabel.TabIndex = 0;
            idLabel.Text = "ID";
            // 
            // id
            // 
            id.Location = new System.Drawing.Point(44, 12);
            id.Name = "id";
            id.ReadOnly = true;
            id.Size = new System.Drawing.Size(100, 23);
            id.TabIndex = 1;
            // 
            // close
            // 
            close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            close.Location = new System.Drawing.Point(116, 238);
            close.Name = "close";
            close.Size = new System.Drawing.Size(75, 23);
            close.TabIndex = 29;
            close.Text = "Close";
            close.UseVisualStyleBackColor = true;
            close.Click += close_Click;
            // 
            // PropertiesDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            ClientSize = new System.Drawing.Size(306, 267);
            Controls.Add(close);
            Controls.Add(idLabel);
            Controls.Add(id);
            Controls.Add(typeLabel);
            Controls.Add(type);
            Controls.Add(endptProperties);
            Controls.Add(arcProperties);
            Name = "PropertiesDialog";
            ShowInTaskbar = false;
            Text = "Properties";
            Load += PropertiesDialog_Load;
            endptProperties.ResumeLayout(false);
            endptProperties.PerformLayout();
            degradPanel.ResumeLayout(false);
            degradPanel.PerformLayout();
            arcProperties.ResumeLayout(false);
            arcProperties.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label idLabel;
        private System.Windows.Forms.TextBox id;
        private System.Windows.Forms.Label typeLabel;
        private System.Windows.Forms.TextBox type;
        private System.Windows.Forms.Label xsLabel;
        private System.Windows.Forms.TextBox xs;
        private System.Windows.Forms.Label ysLabel;
        private System.Windows.Forms.TextBox ys;
        private System.Windows.Forms.Label xeLabel;
        private System.Windows.Forms.TextBox xe;
        private System.Windows.Forms.Label yeLabel;
        private System.Windows.Forms.TextBox ye;
        private System.Windows.Forms.Panel endptProperties;
        private System.Windows.Forms.Panel arcProperties;
        private System.Windows.Forms.Panel degradPanel;
        private System.Windows.Forms.RadioButton degrees;
        private System.Windows.Forms.RadioButton radians;
        private System.Windows.Forms.Label eaLabel;
        private System.Windows.Forms.Label saLabel;
        private System.Windows.Forms.TextBox ea;
        private System.Windows.Forms.TextBox sa;
        private System.Windows.Forms.Label dirLabel;
        private System.Windows.Forms.Label radLabel;
        private System.Windows.Forms.TextBox dir;
        private System.Windows.Forms.TextBox rad;
        private System.Windows.Forms.Label ycLabel;
        private System.Windows.Forms.Label xcLabel;
        private System.Windows.Forms.TextBox yc;
        private System.Windows.Forms.TextBox xc;
        private System.Windows.Forms.Button close;
    }
}