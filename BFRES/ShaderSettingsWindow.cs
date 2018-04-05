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
    public partial class ShaderSettingsWindow : Form
    {
        public ShaderSettingsWindow()
        {
            InitializeComponent();
        }
        public Dictionary<string, ushort> Mapping;
        public Dictionary<string, bool> Colli;
        public ShaderSettingsWindow(string[] names)
        {
            InitializeComponent();
           
            for (int i = 0; i <= names.Length; i++)
            {
              //  dataGridView1.Rows.Add(names[i], 1);
                
            }
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void ShaderSettingsWindow_Load(object sender, EventArgs e)
        {


        }
    }
}
