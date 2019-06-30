namespace GFD_W
{
    partial class SplashForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashForm));
            this.SplashProgressLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // SplashProgressLabel
            // 
            this.SplashProgressLabel.BackColor = System.Drawing.Color.Transparent;
            this.SplashProgressLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.SplashProgressLabel.ForeColor = System.Drawing.Color.Green;
            this.SplashProgressLabel.Location = new System.Drawing.Point(0, 379);
            this.SplashProgressLabel.Name = "SplashProgressLabel";
            this.SplashProgressLabel.Size = new System.Drawing.Size(750, 21);
            this.SplashProgressLabel.TabIndex = 0;
            this.SplashProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SplashForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::GFD_W.Properties.Resources.GFD_W_Splash;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(750, 400);
            this.Controls.Add(this.SplashProgressLabel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplashForm";
            this.Opacity = 0D;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label SplashProgressLabel;
    }
}

