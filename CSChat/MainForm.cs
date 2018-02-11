using System;
using System.Windows.Forms;

namespace CSChat
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var cm=new ConnectionManager();
            cm.Port = 2333;
        }
    }
}
