namespace Toxy
{
    partial class frmMain
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
            this.listFriends = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txtConversation = new System.Windows.Forms.RichTextBox();
            this.txtToSend = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.infoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAddFriend = new System.Windows.Forms.ToolStripMenuItem();
            this.btnCopyID = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblCurrFriend = new System.Windows.Forms.Label();
            this.lblCurrFriendStatus = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listFriends
            // 
            this.listFriends.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listFriends.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listFriends.FullRowSelect = true;
            this.listFriends.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listFriends.HideSelection = false;
            this.listFriends.Location = new System.Drawing.Point(12, 27);
            this.listFriends.MultiSelect = false;
            this.listFriends.Name = "listFriends";
            this.listFriends.Size = new System.Drawing.Size(246, 408);
            this.listFriends.TabIndex = 0;
            this.listFriends.UseCompatibleStateImageBehavior = false;
            this.listFriends.View = System.Windows.Forms.View.Tile;
            this.listFriends.SelectedIndexChanged += new System.EventHandler(this.listFriends_SelectedIndexChanged);
            this.listFriends.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.listFriends_KeyPress);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Username";
            this.columnHeader1.Width = 143;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Status";
            this.columnHeader2.Width = 99;
            // 
            // txtConversation
            // 
            this.txtConversation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConversation.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConversation.Location = new System.Drawing.Point(264, 60);
            this.txtConversation.Name = "txtConversation";
            this.txtConversation.ReadOnly = true;
            this.txtConversation.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtConversation.Size = new System.Drawing.Size(536, 349);
            this.txtConversation.TabIndex = 1;
            this.txtConversation.Text = "";
            // 
            // txtToSend
            // 
            this.txtToSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtToSend.Location = new System.Drawing.Point(264, 415);
            this.txtToSend.Name = "txtToSend";
            this.txtToSend.Size = new System.Drawing.Size(536, 20);
            this.txtToSend.TabIndex = 2;
            this.txtToSend.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtToSend_KeyPress);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.infoToolStripMenuItem,
            this.optionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(808, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // infoToolStripMenuItem
            // 
            this.infoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnAddFriend,
            this.btnCopyID,
            this.btnExit});
            this.infoToolStripMenuItem.Name = "infoToolStripMenuItem";
            this.infoToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.infoToolStripMenuItem.Text = "File";
            // 
            // btnAddFriend
            // 
            this.btnAddFriend.Name = "btnAddFriend";
            this.btnAddFriend.Size = new System.Drawing.Size(183, 22);
            this.btnAddFriend.Text = "Add friend";
            this.btnAddFriend.Click += new System.EventHandler(this.btnAddFriend_Click);
            // 
            // btnCopyID
            // 
            this.btnCopyID.Name = "btnCopyID";
            this.btnCopyID.Size = new System.Drawing.Size(183, 22);
            this.btnCopyID.Text = "Copy ID to clipboard";
            this.btnCopyID.Click += new System.EventHandler(this.btnCopyID_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // lblCurrFriend
            // 
            this.lblCurrFriend.AutoSize = true;
            this.lblCurrFriend.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrFriend.Location = new System.Drawing.Point(264, 27);
            this.lblCurrFriend.Name = "lblCurrFriend";
            this.lblCurrFriend.Size = new System.Drawing.Size(0, 16);
            this.lblCurrFriend.TabIndex = 5;
            // 
            // lblCurrFriendStatus
            // 
            this.lblCurrFriendStatus.AutoSize = true;
            this.lblCurrFriendStatus.Location = new System.Drawing.Point(264, 44);
            this.lblCurrFriendStatus.Name = "lblCurrFriendStatus";
            this.lblCurrFriendStatus.Size = new System.Drawing.Size(0, 13);
            this.lblCurrFriendStatus.TabIndex = 6;
            // 
            // btnExit
            // 
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(183, 22);
            this.btnExit.Text = "Exit";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(808, 442);
            this.Controls.Add(this.lblCurrFriendStatus);
            this.Controls.Add(this.lblCurrFriend);
            this.Controls.Add(this.txtToSend);
            this.Controls.Add(this.txtConversation);
            this.Controls.Add(this.listFriends);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(824, 481);
            this.Name = "frmMain";
            this.Text = "Toxy";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listFriends;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.RichTextBox txtConversation;
        private System.Windows.Forms.TextBox txtToSend;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem infoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem btnCopyID;
        private System.Windows.Forms.ToolStripMenuItem btnAddFriend;
        private System.Windows.Forms.Label lblCurrFriend;
        private System.Windows.Forms.Label lblCurrFriendStatus;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem btnExit;

    }
}

