using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Toxy
{
    public partial class frmID : Form
    {
        public frmID(string id)
        {
            InitializeComponent();

            txtID.Text = id;
        }

        private void frmID_Load(object sender, EventArgs e)
        {

        }
    }
}
