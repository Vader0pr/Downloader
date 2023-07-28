﻿
namespace Downloader
{
    partial class MainForm
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
            AddItemTextbox = new TextBox();
            AddToQueueButton = new Button();
            StartDownloadButton = new Button();
            DownloadQueueListbox = new ListBox();
            DeleteSelectedButton = new Button();
            CurrentDownloadInfoTextbox = new TextBox();
            CurrentDownloadLabel = new Label();
            SuspendLayout();
            // 
            // AddItemTextbox
            // 
            AddItemTextbox.BackColor = Color.FromArgb(25, 25, 25);
            AddItemTextbox.BorderStyle = BorderStyle.FixedSingle;
            AddItemTextbox.ForeColor = SystemColors.Window;
            AddItemTextbox.Location = new Point(12, 12);
            AddItemTextbox.Name = "AddItemTextbox";
            AddItemTextbox.Size = new Size(574, 23);
            AddItemTextbox.TabIndex = 0;
            AddItemTextbox.KeyPress += AddItemTextbox_KeyPress;
            // 
            // AddToQueueButton
            // 
            AddToQueueButton.BackColor = Color.FromArgb(0, 192, 0);
            AddToQueueButton.Cursor = Cursors.Hand;
            AddToQueueButton.FlatAppearance.BorderSize = 0;
            AddToQueueButton.FlatStyle = FlatStyle.Flat;
            AddToQueueButton.Location = new Point(592, 12);
            AddToQueueButton.Name = "AddToQueueButton";
            AddToQueueButton.Size = new Size(95, 23);
            AddToQueueButton.TabIndex = 1;
            AddToQueueButton.Text = "Add to queue";
            AddToQueueButton.UseVisualStyleBackColor = false;
            AddToQueueButton.Click += AddToQueueButton_Click;
            // 
            // StartDownloadButton
            // 
            StartDownloadButton.BackColor = Color.FromArgb(0, 192, 0);
            StartDownloadButton.Cursor = Cursors.Hand;
            StartDownloadButton.FlatAppearance.BorderSize = 0;
            StartDownloadButton.FlatStyle = FlatStyle.Flat;
            StartDownloadButton.Location = new Point(693, 12);
            StartDownloadButton.Name = "StartDownloadButton";
            StartDownloadButton.Size = new Size(95, 23);
            StartDownloadButton.TabIndex = 2;
            StartDownloadButton.Text = "Start download";
            StartDownloadButton.UseVisualStyleBackColor = false;
            StartDownloadButton.Click += StartDownloadButton_Click;
            // 
            // DownloadQueueListbox
            // 
            DownloadQueueListbox.BackColor = Color.FromArgb(25, 25, 25);
            DownloadQueueListbox.BorderStyle = BorderStyle.FixedSingle;
            DownloadQueueListbox.ForeColor = SystemColors.Window;
            DownloadQueueListbox.FormattingEnabled = true;
            DownloadQueueListbox.ItemHeight = 15;
            DownloadQueueListbox.Location = new Point(12, 41);
            DownloadQueueListbox.Name = "DownloadQueueListbox";
            DownloadQueueListbox.SelectionMode = SelectionMode.MultiExtended;
            DownloadQueueListbox.Size = new Size(574, 362);
            DownloadQueueListbox.TabIndex = 3;
            // 
            // DeleteSelectedButton
            // 
            DeleteSelectedButton.BackColor = Color.Red;
            DeleteSelectedButton.Cursor = Cursors.Hand;
            DeleteSelectedButton.FlatAppearance.BorderSize = 0;
            DeleteSelectedButton.FlatStyle = FlatStyle.Flat;
            DeleteSelectedButton.Location = new Point(592, 41);
            DeleteSelectedButton.Name = "DeleteSelectedButton";
            DeleteSelectedButton.Size = new Size(95, 23);
            DeleteSelectedButton.TabIndex = 4;
            DeleteSelectedButton.Text = "Delete selected";
            DeleteSelectedButton.UseVisualStyleBackColor = false;
            DeleteSelectedButton.Click += DeleteSelectedButton_Click;
            // 
            // CurrentDownloadInfoTextbox
            // 
            CurrentDownloadInfoTextbox.BackColor = Color.FromArgb(25, 25, 25);
            CurrentDownloadInfoTextbox.BorderStyle = BorderStyle.FixedSingle;
            CurrentDownloadInfoTextbox.ForeColor = SystemColors.Window;
            CurrentDownloadInfoTextbox.Location = new Point(12, 415);
            CurrentDownloadInfoTextbox.Name = "CurrentDownloadInfoTextbox";
            CurrentDownloadInfoTextbox.ReadOnly = true;
            CurrentDownloadInfoTextbox.Size = new Size(574, 23);
            CurrentDownloadInfoTextbox.TabIndex = 5;
            // 
            // CurrentDownloadLabel
            // 
            CurrentDownloadLabel.ForeColor = SystemColors.Control;
            CurrentDownloadLabel.Location = new Point(592, 407);
            CurrentDownloadLabel.Name = "CurrentDownloadLabel";
            CurrentDownloadLabel.Size = new Size(196, 34);
            CurrentDownloadLabel.TabIndex = 6;
            CurrentDownloadLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(15, 15, 15);
            ClientSize = new Size(800, 450);
            Controls.Add(CurrentDownloadLabel);
            Controls.Add(CurrentDownloadInfoTextbox);
            Controls.Add(DeleteSelectedButton);
            Controls.Add(DownloadQueueListbox);
            Controls.Add(StartDownloadButton);
            Controls.Add(AddToQueueButton);
            Controls.Add(AddItemTextbox);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MainForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox AddItemTextbox;
        private Button AddToQueueButton;
        private Button StartDownloadButton;
        private ListBox DownloadQueueListbox;
        private Button DeleteSelectedButton;
        private TextBox CurrentDownloadInfoTextbox;
        private Label CurrentDownloadLabel;
    }
}