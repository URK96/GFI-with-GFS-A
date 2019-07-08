namespace GFD_W_Updater
{
    partial class Updater
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Updater));
            this.Updater_UpdateAnimationBox = new System.Windows.Forms.PictureBox();
            this.Updater_DownloadProgressBar = new System.Windows.Forms.ProgressBar();
            this.Updater_VersionLabel = new System.Windows.Forms.Label();
            this.Updater_StatusLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.Updater_UpdateAnimationBox)).BeginInit();
            this.SuspendLayout();
            // 
            // Updater_UpdateAnimationBox
            // 
            this.Updater_UpdateAnimationBox.BackColor = System.Drawing.Color.Transparent;
            this.Updater_UpdateAnimationBox.Image = global::GFD_W_Updater.Properties.Resources.K7_move;
            this.Updater_UpdateAnimationBox.Location = new System.Drawing.Point(311, -8);
            this.Updater_UpdateAnimationBox.Name = "Updater_UpdateAnimationBox";
            this.Updater_UpdateAnimationBox.Size = new System.Drawing.Size(128, 128);
            this.Updater_UpdateAnimationBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Updater_UpdateAnimationBox.TabIndex = 0;
            this.Updater_UpdateAnimationBox.TabStop = false;
            this.Updater_UpdateAnimationBox.Visible = false;
            // 
            // Updater_DownloadProgressBar
            // 
            this.Updater_DownloadProgressBar.BackColor = System.Drawing.Color.Black;
            this.Updater_DownloadProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Updater_DownloadProgressBar.Location = new System.Drawing.Point(0, 390);
            this.Updater_DownloadProgressBar.MarqueeAnimationSpeed = 10;
            this.Updater_DownloadProgressBar.Name = "Updater_DownloadProgressBar";
            this.Updater_DownloadProgressBar.Size = new System.Drawing.Size(750, 10);
            this.Updater_DownloadProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.Updater_DownloadProgressBar.TabIndex = 2;
            // 
            // Updater_VersionLabel
            // 
            this.Updater_VersionLabel.AutoSize = true;
            this.Updater_VersionLabel.BackColor = System.Drawing.Color.Transparent;
            this.Updater_VersionLabel.ForeColor = System.Drawing.Color.LimeGreen;
            this.Updater_VersionLabel.Location = new System.Drawing.Point(12, 9);
            this.Updater_VersionLabel.Name = "Updater_VersionLabel";
            this.Updater_VersionLabel.Size = new System.Drawing.Size(26, 12);
            this.Updater_VersionLabel.TabIndex = 3;
            this.Updater_VersionLabel.Text = "ver.";
            // 
            // Updater_StatusLabel
            // 
            this.Updater_StatusLabel.BackColor = System.Drawing.Color.Transparent;
            this.Updater_StatusLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Updater_StatusLabel.ForeColor = System.Drawing.Color.SlateGray;
            this.Updater_StatusLabel.Image = ((System.Drawing.Image)(resources.GetObject("Updater_StatusLabel.Image")));
            this.Updater_StatusLabel.Location = new System.Drawing.Point(0, 364);
            this.Updater_StatusLabel.Name = "Updater_StatusLabel";
            this.Updater_StatusLabel.Size = new System.Drawing.Size(750, 26);
            this.Updater_StatusLabel.TabIndex = 4;
            this.Updater_StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Updater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::GFD_W_Updater.Properties.Resources.GFD_W_Splash;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(750, 400);
            this.Controls.Add(this.Updater_StatusLabel);
            this.Controls.Add(this.Updater_VersionLabel);
            this.Controls.Add(this.Updater_DownloadProgressBar);
            this.Controls.Add(this.Updater_UpdateAnimationBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Updater";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "소전사전 업데이터";
            ((System.ComponentModel.ISupportInitialize)(this.Updater_UpdateAnimationBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox Updater_UpdateAnimationBox;
        private System.Windows.Forms.ProgressBar Updater_DownloadProgressBar;
        private System.Windows.Forms.Label Updater_VersionLabel;
        private System.Windows.Forms.Label Updater_StatusLabel;
    }
}

