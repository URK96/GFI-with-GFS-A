using System;
using System.Drawing;
using System.Windows.Forms;

namespace GFD_W
{
    public partial class GFDWInfo : Form
    {
        public GFDWInfo()
        {
            InitializeComponent();
        }

        private void Button1_MouseEnter(object sender, EventArgs e)
        {
            (sender as Button).BackColor = Color.OrangeRed;
        }

        private void Button1_MouseDown(object sender, MouseEventArgs e)
        {
            (sender as Button).BackColor = Color.Red;
        }

        private void Button1_MouseLeave(object sender, EventArgs e)
        {
            (sender as Button).BackColor = Color.Black;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
