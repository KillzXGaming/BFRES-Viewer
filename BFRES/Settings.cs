using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFRES
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();

            comboBox1.Items.Add("Default Theme");
            comboBox1.Items.Add("Dark Theme");
            comboBox1.SelectedIndex = comboBox1.FindStringExact("Default Theme");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var f1 = new Form1();

            if (comboBox1.SelectedItem == "Dark Theme")
            {

                this.BackColor = Color.FromArgb(67, 67, 67);
                this.comboBox1.BackColor = Color.FromArgb(67, 67, 67);
                this.label1.ForeColor = Color.White;
                this.comboBox1.ForeColor = Color.White;
            }
            else if (comboBox1.SelectedItem == "Default Theme")
            {
                this.BackColor = Color.White;
                this.label1.ForeColor = Color.Black;
                this.comboBox1.BackColor = Color.White;
                this.comboBox1.ForeColor = Color.Black;

            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
