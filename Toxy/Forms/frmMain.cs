using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using SharpTox;

namespace Toxy
{
    public partial class frmMain : Form
    {
        private Tox tox;
        private string ID;

        public frmMain()
        {
            InitializeComponent();

            tox = new Tox(false);
            tox.OnFriendRequest += OnFriendRequest;
            tox.OnFriendMessage += OnFriendMessage;
            tox.OnFriendAction += OnFriendAction;
            if (!tox.Load("data"))
            {
                MessageBox.Show("Could not load tox data, this program will now exit.");
                Close();
            }
            tox.Start();

            ID = tox.GetAddress();

            foreach(int friendnumber in tox.GetFriendlist())
            {
                ListViewItem item = new ListViewItem(tox.GetName(friendnumber));
                listFriends.Items.Add(item);
            }
        }

        private void OnFriendAction(int friendnumber, string action)
        {
            BeginInvoke(((Action)(() =>
                {
                    txtConversation.AppendText(" * " + tox.GetName(friendnumber) + " " + action);
                    txtConversation.AppendText(Environment.NewLine);
                })));
        }

        private void OnFriendMessage(int friendnumber, string message)
        {
            BeginInvoke(((Action)(() =>
                {
                    txtConversation.AppendText("<" + tox.GetName(friendnumber) + "> " + message);
                    txtConversation.AppendText(Environment.NewLine);
                })));
        }

        private void OnFriendRequest(string id, string message)
        {
            tox.AddFriend(id);
        }

        private void Main_Load(object sender, EventArgs e) { }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            tox.Save("data");
            tox.Kill();
        }

        private void btnViewID_Click(object sender, EventArgs e)
        {
            frmID form = new frmID(ID);
            form.ShowDialog();
        }

        private void btnAddFriend_Click(object sender, EventArgs e)
        {
            frmAddFriend form = new frmAddFriend();
            form.ShowDialog();

            tox.AddFriend(form.ID, "Hey, I would like to add you as a friend!");
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            tox.SendMessage(0, txtToSend.Text);

            txtConversation.AppendText("<" + tox.GetSelfName() + "> " + txtToSend.Text);
            txtConversation.AppendText(Environment.NewLine);
            txtToSend.Text = "";
        }
    }
}
