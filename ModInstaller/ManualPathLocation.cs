using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;

namespace ModInstaller
{
    public partial class ManualPathLocation : Form
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")] private readonly string currOS;
        
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
            _openFileDialog.Reset();
            folderBrowserDialog1.Reset();
            if (currOS == "MacOS")
            {
                _openFileDialog.ShowDialog();
                _openFileDialog.FileOk += (o, args) =>
                {
                    if (!String.IsNullOrEmpty(_openFileDialog.FileName))
                    {
                        if (File.Exists(_openFileDialog.FileName +
                                        @"/Contents/Resources/Data/Managed/Assembly-CSharp.dll") &&
                            Path.GetFileName(_openFileDialog.FileName) == "hollow_knight.app")
                        {
                            Properties.Settings.Default.installFolder = _openFileDialog.FileName;
                            Properties.Settings.Default.APIFolder =
                                $@"{Properties.Settings.Default.installFolder}/Contents/Resources/Data/Managed/";
                            Properties.Settings.Default.modFolder = $@"{Properties.Settings.Default.APIFolder}/Mods";
                            Properties.Settings.Default.Save();
                            if (!Directory.Exists(Properties.Settings.Default.modFolder))
                                Directory.CreateDirectory(Properties.Settings.Default.modFolder);
                            MessageBox.Show(
                                $"Hollow Knight installation path:\n{Properties.Settings.Default.installFolder}");
                            Close();
                        }
                        else
                        {
                            MessageBox.Show(@"Invalid path selected.
Please select the correct installation path for Hollow Knight.");
                            button1_Click(new object(), EventArgs.Empty);
                        }
                    }
                    else
                        MessageBox.Show("Please select your installation folder to proceed.");
                };
            }
            else
            {
                folderBrowserDialog1.ShowDialog();
                if (!String.IsNullOrEmpty(folderBrowserDialog1.SelectedPath))
                {
                    if (File.Exists(folderBrowserDialog1.SelectedPath +
                                    @"/hollow_knight_Data/Managed/Assembly-CSharp.dll") &&
                        Path.GetFileName(folderBrowserDialog1.SelectedPath) == "Hollow Knight")
                    {
                        Properties.Settings.Default.installFolder = folderBrowserDialog1.SelectedPath;
                        Properties.Settings.Default.APIFolder =
                            $@"{Properties.Settings.Default.installFolder}/hollow_knight_Data/Managed";
                        Properties.Settings.Default.modFolder = $@"{Properties.Settings.Default.APIFolder}/Mods";
                        Properties.Settings.Default.Save();
                        if (!Directory.Exists(Properties.Settings.Default.modFolder))
                            Directory.CreateDirectory(Properties.Settings.Default.modFolder);
                        MessageBox.Show($"Hollow Knight installation path:\n{Properties.Settings.Default.installFolder}");
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(@"Invalid path selected.
Please select the correct installation path for Hollow Knight.");
                        button1_Click(new object(), EventArgs.Empty);
                    }
                }
                else
                    MessageBox.Show("Please select your installation folder to proceed.");
            }
        }
    }
}
