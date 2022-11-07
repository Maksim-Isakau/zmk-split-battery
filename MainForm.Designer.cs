namespace ZMKSplit
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.devicesListView = new System.Windows.Forms.ListView();
            this.NameColumn = new System.Windows.Forms.ColumnHeader();
            this.StatusColumn = new System.Windows.Forms.ColumnHeader();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.refreshIntervalEditBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.reloadButton = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // devicesListView
            // 
            this.devicesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.devicesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.NameColumn,
            this.StatusColumn});
            this.devicesListView.FullRowSelect = true;
            this.devicesListView.GridLines = true;
            this.devicesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.devicesListView.HideSelection = false;
            this.devicesListView.Location = new System.Drawing.Point(12, 12);
            this.devicesListView.MultiSelect = false;
            this.devicesListView.Name = "devicesListView";
            this.devicesListView.ShowGroups = false;
            this.devicesListView.Size = new System.Drawing.Size(382, 369);
            this.devicesListView.TabIndex = 0;
            this.devicesListView.UseCompatibleStateImageBehavior = false;
            this.devicesListView.View = System.Windows.Forms.View.Details;
            this.devicesListView.DoubleClick += new System.EventHandler(this.devicesListView_DoubleClick);
            // 
            // NameColumn
            // 
            this.NameColumn.Text = "Name";
            this.NameColumn.Width = 300;
            // 
            // StatusColumn
            // 
            this.StatusColumn.Text = "Status";
            this.StatusColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.StatusColumn.Width = 78;
            // 
            // notifyIcon
            // 
            this.notifyIcon.Text = "notifyIcon";
            this.notifyIcon.Visible = true;
            // 
            // refreshIntervalEditBox
            // 
            this.refreshIntervalEditBox.AcceptsReturn = true;
            this.refreshIntervalEditBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.refreshIntervalEditBox.Location = new System.Drawing.Point(538, 90);
            this.refreshIntervalEditBox.MaxLength = 5;
            this.refreshIntervalEditBox.Name = "refreshIntervalEditBox";
            this.refreshIntervalEditBox.Size = new System.Drawing.Size(66, 23);
            this.refreshIntervalEditBox.TabIndex = 1;
            this.refreshIntervalEditBox.Text = "3600";
            this.refreshIntervalEditBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.refreshIntervalEditBox.TextChanged += new System.EventHandler(this.MainForm_Load);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(411, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(193, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Refresh battery status interval (sec):";
            // 
            // reloadButton
            // 
            this.reloadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.reloadButton.Location = new System.Drawing.Point(411, 12);
            this.reloadButton.Name = "reloadButton";
            this.reloadButton.Size = new System.Drawing.Size(193, 36);
            this.reloadButton.TabIndex = 3;
            this.reloadButton.Text = "Reload devices";
            this.reloadButton.UseVisualStyleBackColor = true;
            this.reloadButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.reloadButton_MouseClick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 384);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(616, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(39, 17);
            this.statusLabel.Text = "Ready";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(616, 406);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.reloadButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.refreshIntervalEditBox);
            this.Controls.Add(this.devicesListView);
            this.Name = "MainForm";
            this.Text = "ZMK Split Battery Status";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView devicesListView;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.TextBox refreshIntervalEditBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button reloadButton;
        private System.Windows.Forms.ColumnHeader NameColumn;
        private System.Windows.Forms.ColumnHeader StatusColumn;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    }
}
