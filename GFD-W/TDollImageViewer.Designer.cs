namespace GFD_W
{
    partial class TDollImageViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TDollImageViewer));
            this.TDollDic_ImageViewer_MainPanel = new System.Windows.Forms.Panel();
            this.TDollDic_ImageViewer_CostumeSelector = new System.Windows.Forms.ComboBox();
            this.TDollDic_ImageViewer_UtilButtonPanel = new System.Windows.Forms.Panel();
            this.TDollDic_ImageViewer_DamageToggle = new System.Windows.Forms.CheckBox();
            this.TDollDic_ImageViewer_CensoredToggle = new System.Windows.Forms.CheckBox();
            this.TDollDic_ImageViewer_RefreshButton = new System.Windows.Forms.Button();
            this.TDollDic_ImageViewer_ImageView = new System.Windows.Forms.PictureBox();
            this.TDollDic_ImageViewer_ImageStatus = new System.Windows.Forms.Label();
            this.TDollDic_ImageViewer_MainPanel.SuspendLayout();
            this.TDollDic_ImageViewer_UtilButtonPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TDollDic_ImageViewer_ImageView)).BeginInit();
            this.SuspendLayout();
            // 
            // TDollDic_ImageViewer_MainPanel
            // 
            this.TDollDic_ImageViewer_MainPanel.Controls.Add(this.TDollDic_ImageViewer_ImageStatus);
            this.TDollDic_ImageViewer_MainPanel.Controls.Add(this.TDollDic_ImageViewer_ImageView);
            this.TDollDic_ImageViewer_MainPanel.Controls.Add(this.TDollDic_ImageViewer_UtilButtonPanel);
            this.TDollDic_ImageViewer_MainPanel.Controls.Add(this.TDollDic_ImageViewer_CostumeSelector);
            this.TDollDic_ImageViewer_MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TDollDic_ImageViewer_MainPanel.Location = new System.Drawing.Point(0, 0);
            this.TDollDic_ImageViewer_MainPanel.Name = "TDollDic_ImageViewer_MainPanel";
            this.TDollDic_ImageViewer_MainPanel.Size = new System.Drawing.Size(560, 720);
            this.TDollDic_ImageViewer_MainPanel.TabIndex = 0;
            // 
            // TDollDic_ImageViewer_CostumeSelector
            // 
            this.TDollDic_ImageViewer_CostumeSelector.Dock = System.Windows.Forms.DockStyle.Top;
            this.TDollDic_ImageViewer_CostumeSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TDollDic_ImageViewer_CostumeSelector.FormattingEnabled = true;
            this.TDollDic_ImageViewer_CostumeSelector.Location = new System.Drawing.Point(0, 0);
            this.TDollDic_ImageViewer_CostumeSelector.Name = "TDollDic_ImageViewer_CostumeSelector";
            this.TDollDic_ImageViewer_CostumeSelector.Size = new System.Drawing.Size(560, 20);
            this.TDollDic_ImageViewer_CostumeSelector.TabIndex = 0;
            this.TDollDic_ImageViewer_CostumeSelector.SelectedIndexChanged += new System.EventHandler(this.TDollDic_ImageViewer_CostumeSelector_SelectedIndexChanged);
            // 
            // TDollDic_ImageViewer_UtilButtonPanel
            // 
            this.TDollDic_ImageViewer_UtilButtonPanel.Controls.Add(this.TDollDic_ImageViewer_RefreshButton);
            this.TDollDic_ImageViewer_UtilButtonPanel.Controls.Add(this.TDollDic_ImageViewer_CensoredToggle);
            this.TDollDic_ImageViewer_UtilButtonPanel.Controls.Add(this.TDollDic_ImageViewer_DamageToggle);
            this.TDollDic_ImageViewer_UtilButtonPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TDollDic_ImageViewer_UtilButtonPanel.Location = new System.Drawing.Point(0, 20);
            this.TDollDic_ImageViewer_UtilButtonPanel.Name = "TDollDic_ImageViewer_UtilButtonPanel";
            this.TDollDic_ImageViewer_UtilButtonPanel.Size = new System.Drawing.Size(560, 26);
            this.TDollDic_ImageViewer_UtilButtonPanel.TabIndex = 2;
            // 
            // TDollDic_ImageViewer_DamageToggle
            // 
            this.TDollDic_ImageViewer_DamageToggle.Appearance = System.Windows.Forms.Appearance.Button;
            this.TDollDic_ImageViewer_DamageToggle.Dock = System.Windows.Forms.DockStyle.Left;
            this.TDollDic_ImageViewer_DamageToggle.Location = new System.Drawing.Point(0, 0);
            this.TDollDic_ImageViewer_DamageToggle.Name = "TDollDic_ImageViewer_DamageToggle";
            this.TDollDic_ImageViewer_DamageToggle.Size = new System.Drawing.Size(177, 26);
            this.TDollDic_ImageViewer_DamageToggle.TabIndex = 0;
            this.TDollDic_ImageViewer_DamageToggle.Text = "정상";
            this.TDollDic_ImageViewer_DamageToggle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.TDollDic_ImageViewer_DamageToggle.UseVisualStyleBackColor = true;
            this.TDollDic_ImageViewer_DamageToggle.CheckedChanged += new System.EventHandler(this.TDollDic_ImageViewer_DamageToggle_CheckedChanged);
            // 
            // TDollDic_ImageViewer_CensoredToggle
            // 
            this.TDollDic_ImageViewer_CensoredToggle.Appearance = System.Windows.Forms.Appearance.Button;
            this.TDollDic_ImageViewer_CensoredToggle.Dock = System.Windows.Forms.DockStyle.Right;
            this.TDollDic_ImageViewer_CensoredToggle.Enabled = false;
            this.TDollDic_ImageViewer_CensoredToggle.Location = new System.Drawing.Point(379, 0);
            this.TDollDic_ImageViewer_CensoredToggle.Name = "TDollDic_ImageViewer_CensoredToggle";
            this.TDollDic_ImageViewer_CensoredToggle.Size = new System.Drawing.Size(181, 26);
            this.TDollDic_ImageViewer_CensoredToggle.TabIndex = 2;
            this.TDollDic_ImageViewer_CensoredToggle.Text = "????";
            this.TDollDic_ImageViewer_CensoredToggle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.TDollDic_ImageViewer_CensoredToggle.UseVisualStyleBackColor = true;
            // 
            // TDollDic_ImageViewer_RefreshButton
            // 
            this.TDollDic_ImageViewer_RefreshButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.TDollDic_ImageViewer_RefreshButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TDollDic_ImageViewer_RefreshButton.Location = new System.Drawing.Point(177, 0);
            this.TDollDic_ImageViewer_RefreshButton.Name = "TDollDic_ImageViewer_RefreshButton";
            this.TDollDic_ImageViewer_RefreshButton.Size = new System.Drawing.Size(202, 26);
            this.TDollDic_ImageViewer_RefreshButton.TabIndex = 3;
            this.TDollDic_ImageViewer_RefreshButton.Text = "새로 고침";
            this.TDollDic_ImageViewer_RefreshButton.UseVisualStyleBackColor = true;
            this.TDollDic_ImageViewer_RefreshButton.Click += new System.EventHandler(this.TDollDic_ImageViewer_RefreshButton_Click);
            // 
            // TDollDic_ImageViewer_ImageView
            // 
            this.TDollDic_ImageViewer_ImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TDollDic_ImageViewer_ImageView.Location = new System.Drawing.Point(0, 46);
            this.TDollDic_ImageViewer_ImageView.Name = "TDollDic_ImageViewer_ImageView";
            this.TDollDic_ImageViewer_ImageView.Size = new System.Drawing.Size(560, 674);
            this.TDollDic_ImageViewer_ImageView.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.TDollDic_ImageViewer_ImageView.TabIndex = 3;
            this.TDollDic_ImageViewer_ImageView.TabStop = false;
            // 
            // TDollDic_ImageViewer_ImageStatus
            // 
            this.TDollDic_ImageViewer_ImageStatus.AutoSize = true;
            this.TDollDic_ImageViewer_ImageStatus.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.TDollDic_ImageViewer_ImageStatus.Location = new System.Drawing.Point(3, 699);
            this.TDollDic_ImageViewer_ImageStatus.Name = "TDollDic_ImageViewer_ImageStatus";
            this.TDollDic_ImageViewer_ImageStatus.Size = new System.Drawing.Size(0, 11);
            this.TDollDic_ImageViewer_ImageStatus.TabIndex = 4;
            // 
            // TDollImageViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 720);
            this.Controls.Add(this.TDollDic_ImageViewer_MainPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "TDollImageViewer";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "인형 이미지 뷰어";
            this.TDollDic_ImageViewer_MainPanel.ResumeLayout(false);
            this.TDollDic_ImageViewer_MainPanel.PerformLayout();
            this.TDollDic_ImageViewer_UtilButtonPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TDollDic_ImageViewer_ImageView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel TDollDic_ImageViewer_MainPanel;
        private System.Windows.Forms.ComboBox TDollDic_ImageViewer_CostumeSelector;
        private System.Windows.Forms.Label TDollDic_ImageViewer_ImageStatus;
        private System.Windows.Forms.PictureBox TDollDic_ImageViewer_ImageView;
        private System.Windows.Forms.Panel TDollDic_ImageViewer_UtilButtonPanel;
        private System.Windows.Forms.Button TDollDic_ImageViewer_RefreshButton;
        private System.Windows.Forms.CheckBox TDollDic_ImageViewer_CensoredToggle;
        private System.Windows.Forms.CheckBox TDollDic_ImageViewer_DamageToggle;
    }
}