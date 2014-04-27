namespace Toxy
{
    partial class frmFriendRequest
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
            this.lblRequest = new System.Windows.Forms.Label();
            this.btnDecline = new System.Windows.Forms.Button();
            this.btnAccept = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblRequest
            // 
            this.lblRequest.AutoSize = true;
            this.lblRequest.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRequest.Location = new System.Drawing.Point(12, 9);
            this.lblRequest.Name = "lblRequest";
            this.lblRequest.Size = new System.Drawing.Size(337, 16);
            this.lblRequest.TabIndex = 0;
            this.lblRequest.Text = "User with address: null would like to add you as a friend!";
            // 
            // btnDecline
            // 
            this.btnDecline.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDecline.Location = new System.Drawing.Point(827, 68);
            this.btnDecline.Name = "btnDecline";
            this.btnDecline.Size = new System.Drawing.Size(75, 23);
            this.btnDecline.TabIndex = 1;
            this.btnDecline.Text = "Decline";
            this.btnDecline.UseVisualStyleBackColor = true;
            this.btnDecline.Click += new System.EventHandler(this.btnDecline_Click);
            // 
            // btnAccept
            // 
            this.btnAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAccept.Location = new System.Drawing.Point(746, 68);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(75, 23);
            this.btnAccept.TabIndex = 2;
            this.btnAccept.Text = "Accept";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.button2_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.Location = new System.Drawing.Point(12, 43);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(0, 16);
            this.lblMessage.TabIndex = 3;
            // 
            // frmFriendRequest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(911, 103);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.btnDecline);
            this.Controls.Add(this.lblRequest);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmFriendRequest";
            this.Text = "You have received a new friendrequest!";
            this.Load += new System.EventHandler(this.frmFriendRequest_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblRequest;
        private System.Windows.Forms.Button btnDecline;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Label lblMessage;
    }
}