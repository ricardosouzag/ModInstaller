using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;

// ReSharper disable LocalizableElement
// ReSharper disable BuiltInTypeReferenceStyle

namespace ModInstaller
{
    public partial class ManualPathLocation : Form
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private readonly string currOS;

        public ManualPathLocation()
        {
            InitializeComponent();
        }

        public ManualPathLocation(string os)
        {
            currOS = os;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Reset();
            folderBrowserDialog1.ShowDialog();
            
            if (!string.IsNullOrEmpty(folderBrowserDialog1.SelectedPath))
            {
                if (ModManager.PathCheck(currOS, folderBrowserDialog1))
                {
                    Properties.Settings.Default.installFolder = folderBrowserDialog1.SelectedPath;
                    Properties.Settings.Default.APIFolder = ModManager.OSPath(currOS);
                    Properties.Settings.Default.modFolder = $@"{Properties.Settings.Default.APIFolder}/Mods";
                    Properties.Settings.Default.Save();
                    if (!Directory.Exists(Properties.Settings.Default.modFolder))
                        Directory.CreateDirectory(Properties.Settings.Default.modFolder);
                    MessageBox.Show($"Hollow Knight installation path:\n{Properties.Settings.Default.installFolder}");
                    Close();
                }
                else
                {
                    MessageBox.Show("Invalid path selected.\nPlease select the correct installation path for Hollow Knight.");
                    button1_Click(null, EventArgs.Empty);
                }
            }
            else
                MessageBox.Show("Please select your installation folder to proceed.");
        }
    }
}