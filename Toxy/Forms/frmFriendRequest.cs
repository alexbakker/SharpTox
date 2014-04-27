using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Toxy
{
    public partial class frmFriendRequest : Form
    {
        private string ID;
        private string Message;

        public frmFriendRequest(string id, string message)
        {
            InitializeComponent();

            ID = id;
            Message = message;

            lblRequest.Text = "Someone with address: " + ID + "would like to add you as a friend!";
            lblMessage.Text = Message;

            //this.Width = lblRequest.Width + 20;
        }

        private void frmFriendRequest_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
        }

        private void btnDecline_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }
    }
}
