using System;
using System.Windows.Forms;

using SharpTox;

namespace Toxy
{
    public partial class frmOptions : Form
    {
        Tox Tox;
        public frmOptions(Tox tox)
        {
            Tox = tox;

            InitializeComponent();

            txtName.Text = Tox.GetSelfName();
            txtStatus.Text = Tox.GetSelfStatusMessage();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Tox.SetName(txtName.Text);
            Tox.SetStatusMessage(txtStatus.Text);

            Close();
        }

        private void frmOptions_Load(object sender, EventArgs e)
        {

        }
    }
}
