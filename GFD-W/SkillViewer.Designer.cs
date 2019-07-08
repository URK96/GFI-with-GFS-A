namespace GFD_W
{
    partial class SkillViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SkillViewer));
            this.SkillViewer_SkillIconBox = new System.Windows.Forms.PictureBox();
            this.SkillViewer_SkillName = new System.Windows.Forms.Label();
            this.SkillViewer_SkillExplain = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.SkillViewer_SkillIconBox)).BeginInit();
            this.SuspendLayout();
            // 
            // SkillViewer_SkillIconBox
            // 
            this.SkillViewer_SkillIconBox.Location = new System.Drawing.Point(12, 22);
            this.SkillViewer_SkillIconBox.Name = "SkillViewer_SkillIconBox";
            this.SkillViewer_SkillIconBox.Size = new System.Drawing.Size(100, 100);
            this.SkillViewer_SkillIconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.SkillViewer_SkillIconBox.TabIndex = 0;
            this.SkillViewer_SkillIconBox.TabStop = false;
            // 
            // SkillViewer_SkillName
            // 
            this.SkillViewer_SkillName.Font = new System.Drawing.Font("굴림", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.SkillViewer_SkillName.Location = new System.Drawing.Point(143, 22);
            this.SkillViewer_SkillName.Name = "SkillViewer_SkillName";
            this.SkillViewer_SkillName.Size = new System.Drawing.Size(546, 100);
            this.SkillViewer_SkillName.TabIndex = 1;
            this.SkillViewer_SkillName.Text = "label1";
            this.SkillViewer_SkillName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SkillViewer_SkillExplain
            // 
            this.SkillViewer_SkillExplain.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.SkillViewer_SkillExplain.Location = new System.Drawing.Point(12, 148);
            this.SkillViewer_SkillExplain.Name = "SkillViewer_SkillExplain";
            this.SkillViewer_SkillExplain.Size = new System.Drawing.Size(688, 80);
            this.SkillViewer_SkillExplain.TabIndex = 2;
            this.SkillViewer_SkillExplain.Text = "label2";
            this.SkillViewer_SkillExplain.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SkillViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(712, 374);
            this.Controls.Add(this.SkillViewer_SkillExplain);
            this.Controls.Add(this.SkillViewer_SkillName);
            this.Controls.Add(this.SkillViewer_SkillIconBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SkillViewer";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "스킬 정보";
            ((System.ComponentModel.ISupportInitialize)(this.SkillViewer_SkillIconBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox SkillViewer_SkillIconBox;
        private System.Windows.Forms.Label SkillViewer_SkillName;
        private System.Windows.Forms.Label SkillViewer_SkillExplain;
    }
}