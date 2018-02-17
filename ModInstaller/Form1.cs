using System;
using System.IO;
using System.IO.Compression;
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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "")
            label2.Text = $"Selected file:\n{openFileDialog1.FileName}";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
            if (openFileDialog2.FileName != "")
            {
                List<string> newMods = new List<string>();
                foreach (string mod in openFileDialog2.FileNames)
                {
                    newMods.Add(item: Path.GetFileNameWithoutExtension(mod));
                }
                label3.Text = "Selected file(s):\n " + String.Join(separator: "\n", value: newMods.ToArray());
                button4.Enabled = (openFileDialog2.FileName != "");
            }
        }

        public static void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                                 .GroupBy(s => Path.GetDirectoryName(s));
            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile);
                }
            }
            Directory.Delete(source, true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (string mod in openFileDialog2.FileNames)
            {
                ZipFile.ExtractToDirectory(sourceArchiveFileName: mod,destinationDirectoryName: $@"C:\temp");
                IEnumerable<string> mods = Directory.EnumerateDirectories($@"C:\temp");
                IEnumerable<string> res = Directory.EnumerateFiles($@"C:\temp");
                foreach (string Mod in mods)
                {
                    MoveDirectory(Mod, $@"{Properties.Settings.Default.installFolder}\{Path.GetFileName(Mod)}\");
                }
                foreach (string Res in res)
                {
                    File.Copy(Res, $@"{Properties.Settings.Default.installFolder}\{Path.GetFileName(Res)}", true);
                }
                Directory.Delete($@"C:\temp", true);
            }
            if (openFileDialog1.FileName == "")
            MessageBox.Show(text: "Succesfully installed mods!");
            else
            {
                File.Copy(sourceFileName: openFileDialog1.FileName, destFileName: Path.Combine(path1: Properties.Settings.Default.APIFolder, path2: Path.GetFileName(openFileDialog1.FileName)), overwrite: true);
                MessageBox.Show(text: "Succesfully installed API and mods!");
            }
            label3.Text = "Mods to install:";

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Finding the local installation path for Hollow Knight
            if (Properties.Settings.Default.installFolder == "")
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();

                foreach (DriveInfo d in allDrives)
                {
                    if (d.DriveFormat == "NTFS")
                    {
                        if (Directory.Exists(path: $@"{d.Name}Program Files (x86)\Steam\steamapps\common\Hollow Knight"))
                        {
                            DialogResult dialogResult = MessageBox.Show(text: "Is this your Hollow Knight installation path?\n" + $@"{d.Name}Program Files (x86)\Steam\steamapps\common\Hollow Knight", caption: "Path confirmation", buttons: MessageBoxButtons.YesNo);
                            if (dialogResult == DialogResult.Yes)
                            {
                                Properties.Settings.Default.installFolder = $@"{d.Name}Program Files (x86)\Steam\steamapps\common\Hollow Knight";
                                Properties.Settings.Default.Save();
                            }
                        }
                        else if (Directory.Exists(path: $@"{d.Name}Program Files\Steam\steamapps\common\Hollow Knight"))
                        {
                            DialogResult dialogResult = MessageBox.Show(text: "Is this your Hollow Knight installation path?\n" + $@"{d.Name}Program Files\Steam\steamapps\common\Hollow Knight", caption: "Path confirmation", buttons: MessageBoxButtons.YesNo);
                            if (dialogResult == DialogResult.Yes)
                            {
                                Properties.Settings.Default.installFolder = $@"{d.Name}Program Files\Steam\steamapps\common\Hollow Knight";
                                Properties.Settings.Default.Save();
                            }
                        }
                        else if (Directory.Exists(path: $@"{d.Name}Steam\steamapps\common\Hollow Knight"))
                        {
                            DialogResult dialogResult = MessageBox.Show(text: "Is this your Hollow Knight installation path?\n" + $@"{d.Name}Steam\steamapps\common\Hollow Knight", caption: "Path confirmation", buttons: MessageBoxButtons.YesNo);
                            if (dialogResult == DialogResult.Yes)
                            {
                                Properties.Settings.Default.installFolder = $@"{d.Name}Steam\steamapps\common\Hollow Knight";
                                Properties.Settings.Default.Save();
                            }
                        }
                    }
                    if (Properties.Settings.Default.installFolder != "")
                        break;
                }
                if (Properties.Settings.Default.installFolder == "")
                {
                    Form3 form3 = new Form3();
                    this.Hide();
                    form3.FormClosed += new FormClosedEventHandler(form3_FormClosed);
                    form3.Show();
                }
                else
                {
                    Properties.Settings.Default.APIFolder = $@"{Properties.Settings.Default.installFolder}\hollow_knight_data\managed";
                    Properties.Settings.Default.modFolder = $@"{Properties.Settings.Default.APIFolder}\Mods";
                    Properties.Settings.Default.Save();
                }
            }
                
        }

        void label_Paint(object sender, PaintEventArgs e)
        {
            //To show long paths for API
            Label label = (Label)sender;
            using (SolidBrush b = new SolidBrush(label.BackColor))
                e.Graphics.FillRectangle(b, label.ClientRectangle);
            TextRenderer.DrawText(
                e.Graphics,
                label.Text,
                label.Font,
                label.ClientRectangle,
                label.ForeColor,
                TextFormatFlags.PathEllipsis);
        }

        void form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Show();
        }
        public Settings settings;

        private void button5_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2(this);
            form2.Show();
        }
    }
}
