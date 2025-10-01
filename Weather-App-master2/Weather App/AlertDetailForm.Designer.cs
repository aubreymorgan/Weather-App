namespace Weather_App
{
    partial class AlertDetailForm
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
            this.HeadlineLabel = new System.Windows.Forms.Label();
            this.SeverityLabel = new System.Windows.Forms.Label();
            this.EffectiveLabel = new System.Windows.Forms.Label();
            this.DescriptionrichTextBox = new System.Windows.Forms.RichTextBox();
            this.endsLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // HeadlineLabel
            // 
            this.HeadlineLabel.AutoSize = true;
            this.HeadlineLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HeadlineLabel.Location = new System.Drawing.Point(3, 32);
            this.HeadlineLabel.Name = "HeadlineLabel";
            this.HeadlineLabel.Size = new System.Drawing.Size(44, 16);
            this.HeadlineLabel.TabIndex = 0;
            this.HeadlineLabel.Text = "label1";
            // 
            // SeverityLabel
            // 
            this.SeverityLabel.AutoSize = true;
            this.SeverityLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SeverityLabel.ForeColor = System.Drawing.SystemColors.WindowText;
            this.SeverityLabel.Location = new System.Drawing.Point(2, 78);
            this.SeverityLabel.Name = "SeverityLabel";
            this.SeverityLabel.Size = new System.Drawing.Size(51, 20);
            this.SeverityLabel.TabIndex = 1;
            this.SeverityLabel.Text = "label2";
            // 
            // EffectiveLabel
            // 
            this.EffectiveLabel.AutoSize = true;
            this.EffectiveLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EffectiveLabel.Location = new System.Drawing.Point(46, 402);
            this.EffectiveLabel.Name = "EffectiveLabel";
            this.EffectiveLabel.Size = new System.Drawing.Size(51, 20);
            this.EffectiveLabel.TabIndex = 2;
            this.EffectiveLabel.Text = "label3";
            // 
            // DescriptionrichTextBox
            // 
            this.DescriptionrichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DescriptionrichTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.DescriptionrichTextBox.Location = new System.Drawing.Point(25, 125);
            this.DescriptionrichTextBox.Name = "DescriptionrichTextBox";
            this.DescriptionrichTextBox.Size = new System.Drawing.Size(668, 250);
            this.DescriptionrichTextBox.TabIndex = 6;
            this.DescriptionrichTextBox.Text = "";
            // 
            // endsLabel
            // 
            this.endsLabel.AutoSize = true;
            this.endsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.endsLabel.Location = new System.Drawing.Point(421, 402);
            this.endsLabel.Name = "endsLabel";
            this.endsLabel.Size = new System.Drawing.Size(51, 20);
            this.endsLabel.TabIndex = 7;
            this.endsLabel.Text = "label1";
            // 
            // AlertDetailForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.endsLabel);
            this.Controls.Add(this.DescriptionrichTextBox);
            this.Controls.Add(this.EffectiveLabel);
            this.Controls.Add(this.SeverityLabel);
            this.Controls.Add(this.HeadlineLabel);
            this.Name = "AlertDetailForm";
            this.Text = "AlertDetailForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label HeadlineLabel;
        private System.Windows.Forms.Label SeverityLabel;
        private System.Windows.Forms.Label EffectiveLabel;
        private System.Windows.Forms.RichTextBox DescriptionrichTextBox;
        private System.Windows.Forms.Label endsLabel;
    }
}