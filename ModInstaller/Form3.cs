using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModInstaller
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            if (folderBrowserDialog1.SelectedPath != "")
            {
                Properties.Settings.Default.installFolder = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.APIFolder = Properties.Settings.Default.installFolder + @"\hollow_knight_data\managed";
                Properties.Settings.Default.modFolder = Properties.Settings.Default.APIFolder + @"\Mods";
                Properties.Settings.Default.Save();
                MessageBox.Show("Hollow Knight installation path:\n" + Properties.Settings.Default.installFolder);
                this.Close();
            }
            else
                MessageBox.Show("Please select your installation folder to proceed.");
        }
    }
}
