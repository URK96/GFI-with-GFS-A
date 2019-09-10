namespace GFD_W
{
    partial class ProductTimeTable
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProductTimeTable));
            this.ProductTimeTable_ListView = new System.Windows.Forms.ListView();
            this.ProductTimeTable_ProductTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ProductTimeTable_Margin = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ProductTimeTable_Name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // ProductTimeTable_ListView
            // 
            this.ProductTimeTable_ListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ProductTimeTable_ListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ProductTimeTable_Margin,
            this.ProductTimeTable_ProductTime,
            this.ProductTimeTable_Name});
            this.ProductTimeTable_ListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProductTimeTable_ListView.Font = new System.Drawing.Font("굴림", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ProductTimeTable_ListView.FullRowSelect = true;
            this.ProductTimeTable_ListView.GridLines = true;
            this.ProductTimeTable_ListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ProductTimeTable_ListView.HideSelection = false;
            this.ProductTimeTable_ListView.Location = new System.Drawing.Point(0, 0);
            this.ProductTimeTable_ListView.MultiSelect = false;
            this.ProductTimeTable_ListView.Name = "ProductTimeTable_ListView";
            this.ProductTimeTable_ListView.Size = new System.Drawing.Size(398, 596);
            this.ProductTimeTable_ListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.ProductTimeTable_ListView.TabIndex = 0;
            this.ProductTimeTable_ListView.UseCompatibleStateImageBehavior = false;
            this.ProductTimeTable_ListView.View = System.Windows.Forms.View.Details;
            // 
            // ProductTimeTable_ProductTime
            // 
            this.ProductTimeTable_ProductTime.Tag = "Time";
            this.ProductTimeTable_ProductTime.Text = "제조 시간";
            this.ProductTimeTable_ProductTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ProductTimeTable_ProductTime.Width = 140;
            // 
            // ProductTimeTable_Margin
            // 
            this.ProductTimeTable_Margin.Text = "";
            this.ProductTimeTable_Margin.Width = 0;
            // 
            // ProductTimeTable_Name
            // 
            this.ProductTimeTable_Name.Text = "이름";
            this.ProductTimeTable_Name.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ProductTimeTable_Name.Width = 240;
            // 
            // ProductTimeTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(398, 596);
            this.Controls.Add(this.ProductTimeTable_ListView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ProductTimeTable";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "제조 시간표";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView ProductTimeTable_ListView;
        private System.Windows.Forms.ColumnHeader ProductTimeTable_Margin;
        private System.Windows.Forms.ColumnHeader ProductTimeTable_ProductTime;
        private System.Windows.Forms.ColumnHeader ProductTimeTable_Name;
    }
}