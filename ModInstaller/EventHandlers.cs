using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
// ReSharper disable LocalizableElement

namespace ModInstaller
{
    public partial class ModManager
    {
        private void EnableApiClick(object sender, EventArgs e)
        {
            if (!_vanillaEnabled)
            {
                DialogResult result = MessageBox.Show
                (
                    "Do you want to disable the modding api/revert to vanilla?",
                    "Confirmation dialogue",
                    MessageBoxButtons.YesNo
                );
                if (result != DialogResult.Yes) return;
                if (File.Exists($"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.vanilla"))
                {
                    File.Copy
                    (
                        $"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.dll",
                        $"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.mod",
                        true
                    );
                    File.Copy
                    (
                        $"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.vanilla",
                        $"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.dll",
                        true
                    );
                    _assemblyIsAPI = false;
                    MessageBox.Show("Successfully disabled all installed mods!");
                }
                else
                {
                    MessageBox.Show("Unable to locate vanilla Hollow Knight.\nPlease verify integrity of game files and relaunch this installer.");
                    Application.Exit();
                    Close();
                }
            }
            else
            {
                DialogResult result = MessageBox.Show
                (
                    "Do you want to enable the Modding API?",
                    "Confirmation dialogue",
                    MessageBoxButtons.YesNo
                );
                if (result != DialogResult.Yes) return;
                if (File.Exists($"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.mod"))
                {
                    File.Copy
                    (
                        $"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.dll",
                        $"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.vanilla",
                        true
                    );
                    File.Copy
                    (
                        $"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.mod",
                        $"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.dll",
                        true
                    );
                    _assemblyIsAPI = true;
                    MessageBox.Show("Successfully enabled all installed mods!");
                }
                else
                {
                    MessageBox.Show("Unable to locate vanilla Hollow Knight. Please verify integrity of game files.");
                }
            }

            _vanillaEnabled = !_vanillaEnabled;
            button1.Text = _vanillaEnabled
                ? "Enable Modding API"
                : "Revert Back To Unmodded";
        }

        private void ManualInstallClick(object sender, EventArgs e)
        {
            openFileDialog.Reset();
            openFileDialog.Filter = "Mod files|*.zip; *.dll|All files|*.*";
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Select the mods you wish to install";
            openFileDialog.ShowDialog();
        }

        private void ChangePathClick(object sender, EventArgs e)
        {
            folderBrowserDialog1.Reset();
            folderBrowserDialog1.ShowDialog();
            
            if (string.IsNullOrEmpty(folderBrowserDialog1.SelectedPath)) return;
            
            if (PathCheck(OS, folderBrowserDialog1))
            {
                Properties.Settings.Default.installFolder = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.APIFolder = OSPath(OS);
                Properties.Settings.Default.modFolder = $"{Properties.Settings.Default.APIFolder}/Mods";
                Properties.Settings.Default.Save();
                if (!Directory.Exists(Properties.Settings.Default.modFolder))
                    Directory.CreateDirectory(Properties.Settings.Default.modFolder);
                MessageBox.Show($"Hollow Knight installation path:\n{Properties.Settings.Default.installFolder}");

                Application.Restart();
                Close();
            }
            else
            {
                MessageBox.Show
                (
                    "Invalid path selected.\nPlease select the correct installation path for Hollow Knight."
                );
                ChangePathClick(new object(), EventArgs.Empty);
            }
        }
        
        private void DoManualInstall(object sender, EventArgs e)
        {
            if (openFileDialog.FileNames.Length < 1) return;
            foreach (string mod in openFileDialog.FileNames)
            {
                if (Path.GetExtension(mod) == ".zip")
                {
                    InstallMods(mod, Properties.Settings.Default.temp, true);
                }
                else
                {
                    File.Copy(mod, $"{Properties.Settings.Default.modFolder}/{Path.GetFileName(mod)}", true);
                }

                MessageBox.Show($"{Path.GetFileName(mod)} successfully installed!");
            }
        }

        private void ManualPathClosed(object sender, FormClosedEventArgs e)
        {
            Show();
            if (Directory.Exists("/tmp"))
            {
                if (Directory.Exists("/tmp/HKmodinstaller"))
                {
                    DeleteDirectory("/tmp/HKmodinstaller");
                }

                Directory.CreateDirectory("/tmp/HKmodinstaller");
                Properties.Settings.Default.temp = "/tmp/HKmodinstaller";
            }
            else
            {
                Properties.Settings.Default.temp =
                    Directory.Exists($"{Path.GetPathRoot(Properties.Settings.Default.installFolder)}temp")
                        ? $"{Path.GetPathRoot(Properties.Settings.Default.installFolder)}tempMods"
                        : $"{Path.GetPathRoot(Properties.Settings.Default.installFolder)}temp";
            }

            Properties.Settings.Default.Save();
        }

        private void DonateButtonClick(object sender, EventArgs e)
        {
            Process.Start
            (
                "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=G5KYSS3ULQFY6&lc=US&item_name=HK%20ModInstaller&item_number=HKMI&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted"
            );
        }

        private void SearchOnGotFocus(object sender, EventArgs e)
        {
            search.Text = "";
            search.ForeColor = Color.Black;
        }

        private void SearchOnLostFocus(object sender, EventArgs e)
        {
            if (search.Text == "")
            {
                search.Text = "Search...";
                search.ForeColor = Color.Gray;
            }
        }

        private void SearchOnKeyUp(object sender, KeyEventArgs e)
        {
            Controls.Remove(panel);
            FillPanel();
            ResizeUI();
        }
    }
}