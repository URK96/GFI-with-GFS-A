namespace GFD_W
{
    partial class TextViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextViewer));
            this.TextViewer_ControlPanel = new System.Windows.Forms.Panel();
            this.TextViewer_NowStatus = new System.Windows.Forms.Label();
            this.TextViewer_NextButton = new System.Windows.Forms.Button();
            this.TextViewer_PreviousButton = new System.Windows.Forms.Button();
            this.TextViewer_TextField = new System.Windows.Forms.RichTextBox();
            this.TextViewer_ControlPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // TextViewer_ControlPanel
            // 
            this.TextViewer_ControlPanel.Controls.Add(this.TextViewer_NowStatus);
            this.TextViewer_ControlPanel.Controls.Add(this.TextViewer_NextButton);
            this.TextViewer_ControlPanel.Controls.Add(this.TextViewer_PreviousButton);
            this.TextViewer_ControlPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TextViewer_ControlPanel.Location = new System.Drawing.Point(0, 0);
            this.TextViewer_ControlPanel.Name = "TextViewer_ControlPanel";
            this.TextViewer_ControlPanel.Size = new System.Drawing.Size(530, 30);
            this.TextViewer_ControlPanel.TabIndex = 0;
            // 
            // TextViewer_NowStatus
            // 
            this.TextViewer_NowStatus.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.TextViewer_NowStatus.Location = new System.Drawing.Point(156, 0);
            this.TextViewer_NowStatus.Name = "TextViewer_NowStatus";
            this.TextViewer_NowStatus.Size = new System.Drawing.Size(218, 30);
            this.TextViewer_NowStatus.TabIndex = 2;
            this.TextViewer_NowStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextViewer_NextButton
            // 
            this.TextViewer_NextButton.Location = new System.Drawing.Point(380, 0);
            this.TextViewer_NextButton.Name = "TextViewer_NextButton";
            this.TextViewer_NextButton.Size = new System.Drawing.Size(80, 30);
            this.TextViewer_NextButton.TabIndex = 1;
            this.TextViewer_NextButton.Tag = "N";
            this.TextViewer_NextButton.Text = "다음";
            this.TextViewer_NextButton.UseVisualStyleBackColor = true;
            this.TextViewer_NextButton.Click += new System.EventHandler(this.TextViewer_ControlButton_Click);
            // 
            // TextViewer_PreviousButton
            // 
            this.TextViewer_PreviousButton.Location = new System.Drawing.Point(70, 0);
            this.TextViewer_PreviousButton.Name = "TextViewer_PreviousButton";
            this.TextViewer_PreviousButton.Size = new System.Drawing.Size(80, 30);
            this.TextViewer_PreviousButton.TabIndex = 0;
            this.TextViewer_PreviousButton.Tag = "P";
            this.TextViewer_PreviousButton.Text = "이전";
            this.TextViewer_PreviousButton.UseVisualStyleBackColor = true;
            this.TextViewer_PreviousButton.Click += new System.EventHandler(this.TextViewer_ControlButton_Click);
            // 
            // TextViewer_TextField
            // 
            this.TextViewer_TextField.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextViewer_TextField.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextViewer_TextField.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.TextViewer_TextField.Location = new System.Drawing.Point(0, 30);
            this.TextViewer_TextField.Name = "TextViewer_TextField";
            this.TextViewer_TextField.ReadOnly = true;
            this.TextViewer_TextField.Size = new System.Drawing.Size(530, 642);
            this.TextViewer_TextField.TabIndex = 1;
            this.TextViewer_TextField.Text = "";
            // 
            // TextViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(530, 672);
            this.Controls.Add(this.TextViewer_TextField);
            this.Controls.Add(this.TextViewer_ControlPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "TextViewer";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "텍스트 뷰어";
            this.TextViewer_ControlPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel TextViewer_ControlPanel;
        private System.Windows.Forms.Label TextViewer_NowStatus;
        private System.Windows.Forms.Button TextViewer_NextButton;
        private System.Windows.Forms.Button TextViewer_PreviousButton;
        private System.Windows.Forms.RichTextBox TextViewer_TextField;
    }
}