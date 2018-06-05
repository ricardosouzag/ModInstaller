using System;
using System.IO;
using System.Windows.Forms;

namespace ModInstaller
{
    public partial class ManualPathLocation : Form
    {
        public ManualPathLocation()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            if (folderBrowserDialog1.SelectedPath != "")
            {
                Properties.Settings.Default.installFolder = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.APIFolder = $@"{Properties.Settings.Default.installFolder}\hollow_knight_data\Managed";
                Properties.Settings.Default.modFolder = $@"{Properties.Settings.Default.APIFolder}\Mods";
                Properties.Settings.Default.Save();
                if (!Directory.Exists(Properties.Settings.Default.modFolder)) Directory.CreateDirectory(Properties.Settings.Default.modFolder);
                MessageBox.Show(text: $"Hollow Knight installation path:\n{Properties.Settings.Default.installFolder}");
                Close();
            }
            else
                MessageBox.Show(text: "Please select your installation folder to proceed.");
        }
    }
}
