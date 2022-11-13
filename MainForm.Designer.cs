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
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadButton = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.connectButton = new System.Windows.Forms.Button();
            this.reconnectTimer = new System.Windows.Forms.Timer(this.components);
            this.trayContextMenu.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // devicesListView
            // 
            this.devicesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.devicesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.NameColumn});
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
            this.NameColumn.Width = 378;
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.trayContextMenu;
            this.notifyIcon.Visible = true;
            // 
            // trayContextMenu
            // 
            this.trayContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showContextMenuItem,
            this.exitContextMenuItem});
            this.trayContextMenu.Name = "trayContextMenu";
            this.trayContextMenu.Size = new System.Drawing.Size(104, 48);
            // 
            // showContextMenuItem
            // 
            this.showContextMenuItem.Name = "showContextMenuItem";
            this.showContextMenuItem.Size = new System.Drawing.Size(103, 22);
            this.showContextMenuItem.Text = "Show";
            this.showContextMenuItem.Click += new System.EventHandler(this.showContextMenuItem_Click);
            // 
            // exitContextMenuItem
            // 
            this.exitContextMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.exitContextMenuItem.Name = "exitContextMenuItem";
            this.exitContextMenuItem.Size = new System.Drawing.Size(103, 22);
            this.exitContextMenuItem.Text = "E&xit";
            this.exitContextMenuItem.Click += new System.EventHandler(this.exitContextMenuItem_Click);
            // 
            // reloadButton
            // 
            this.reloadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.reloadButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.reloadButton.Location = new System.Drawing.Point(407, 12);
            this.reloadButton.Name = "reloadButton";
            this.reloadButton.Size = new System.Drawing.Size(158, 32);
            this.reloadButton.TabIndex = 3;
            this.reloadButton.Text = "Refresh";
            this.reloadButton.UseVisualStyleBackColor = true;
            this.reloadButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.reloadButton_MouseClick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 384);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(577, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(39, 17);
            this.statusLabel.Text = "Ready";
            // 
            // connectButton
            // 
            this.connectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.connectButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.connectButton.Location = new System.Drawing.Point(407, 50);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(158, 32);
            this.connectButton.TabIndex = 5;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // reconnectTimer
            // 
            this.reconnectTimer.Interval = 1000;
            this.reconnectTimer.Tick += new System.EventHandler(this.reconnectTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(577, 406);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.reloadButton);
            this.Controls.Add(this.devicesListView);
            this.Name = "MainForm";
            this.Text = "ZMK Split Battery Status";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.trayContextMenu.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView devicesListView;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Button reloadButton;
        private System.Windows.Forms.ColumnHeader NameColumn;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.ContextMenuStrip trayContextMenu;
        private System.Windows.Forms.ToolStripMenuItem exitContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showContextMenuItem;
        private System.Windows.Forms.Timer reconnectTimer;
    }
}
