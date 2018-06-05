using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace ModInstaller
{
    public partial class ManualInstall : Form
    {
        public ManualInstall()
        {
            InitializeComponent();
        }

        private ModManager mainForm;

        public ManualInstall(Form callingForm)
        {
            mainForm = callingForm as ModManager;
            InitializeComponent();
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
                    if (File.Exists(targetFile))
                    {
                        if(!File.Exists($@"{targetFolder}\{Path.GetFileName(targetFile)}.vanilla"))
                        {
                            File.Move(targetFile, $@"{targetFolder}\{Path.GetFileName(targetFile)}.vanilla");                            
                        }
                        else
                        {
                            File.Delete(targetFile);
                        }

                    }
                    File.Move(file, targetFile);
                }
            }
            Directory.Delete(source, true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (string mod in openFileDialog2.FileNames)
            {
                installMods(mod, Properties.Settings.Default.temp);
            }
            if (!api)
            MessageBox.Show(text: "Succesfully installed mods!");
            else
            MessageBox.Show(text: "Succesfully installed mods and API!");
            button1.Enabled = (Directory.GetFiles(Properties.Settings.Default.installFolder, "*.vanilla", SearchOption.AllDirectories)).Length > 0;
            label3.Text = "Mods to install:";
        }

        private void Form1_Load(object sender, EventArgs e)
        {               
            button1.Enabled = (Directory.GetFiles(Properties.Settings.Default.installFolder, "*.vanilla", SearchOption.AllDirectories)).Length > 0;
        }

        public void installMods (string mod, string tempFolder)
        {
            if (Path.GetExtension(mod) == ".zip")
            {
                ZipFile.ExtractToDirectory(sourceArchiveFileName: mod, destinationDirectoryName: tempFolder);
                IEnumerable<string> mods = Directory.EnumerateDirectories(tempFolder);
                IEnumerable<string> res = Directory.EnumerateFiles(tempFolder);
                if (res.Any(file => file.Contains("Assembly")))
                {
                    api = true;
                    string[] modDll = Directory.GetFiles(tempFolder, "*.dll", SearchOption.AllDirectories);
                    foreach (string dll in modDll)
                    {
                        if (!File.Exists($@"{ Properties.Settings.Default.APIFolder}\{ Path.GetFileNameWithoutExtension(dll)}.vanilla"))
                            File.Move($@"{Properties.Settings.Default.APIFolder}\{Path.GetFileName(dll)}", $@"{ Properties.Settings.Default.APIFolder}\{ Path.GetFileNameWithoutExtension(dll)}.vanilla");
                        File.Copy(dll, $@"{Properties.Settings.Default.APIFolder}\{Path.GetFileName(dll)}", true);
                        File.Delete(dll);
                    }
                    foreach (string Mod in mods)
                    {
                        MoveDirectory(Mod, $@"{Properties.Settings.Default.installFolder}\{Path.GetFileName(Mod)}\");
                    }
                    foreach (string Res in res)
                    {
                        File.Copy(Res, $@"{Properties.Settings.Default.installFolder}\{Path.GetFileNameWithoutExtension(Res)}({Path.GetFileNameWithoutExtension(mod)}){Path.GetExtension(Res)}", true);
                        File.Delete(Res);
                    }
                    Directory.Delete(tempFolder, true);
                }
                else if (mod.Contains("753"))
                {
                    if (!res.Any(f => f.Contains(".dll")))
                    {
                        string[] modDll = Directory.GetFiles(tempFolder, "*.dll", SearchOption.AllDirectories);
                        foreach (string dll in modDll)
                        {
                            if (!File.Exists($@"{ Properties.Settings.Default.APIFolder}\{ Path.GetFileNameWithoutExtension(dll)}.vanilla"))
                                File.Move($@"{Properties.Settings.Default.APIFolder}\{Path.GetFileName(dll)}", $@"{ Properties.Settings.Default.APIFolder}\{ Path.GetFileNameWithoutExtension(dll)}.vanilla");
                            File.Copy(dll, $@"{Properties.Settings.Default.APIFolder}\{Path.GetFileName(dll)}", true);
                        }
                        foreach (string Mod in mods)
                        {
                            MoveDirectory(Mod, $@"{Properties.Settings.Default.installFolder}\{Path.GetFileName(Mod)}\");
                        }
                        foreach (string Res in res)
                        {
                            File.Copy(Res, $@"{Properties.Settings.Default.installFolder}\{Path.GetFileNameWithoutExtension(Res)}({Path.GetFileNameWithoutExtension(mod)}){Path.GetExtension(Res)}", true);
                            File.Delete(Res);
                        }
                        Directory.Delete(tempFolder, true);
                    }
                    else
                    {
                        foreach (string Res in res)
                        {
                            File.Copy(Res, $@"{Properties.Settings.Default.installFolder}\{Path.GetFileNameWithoutExtension(Res)}({Path.GetFileNameWithoutExtension(mod)}){Path.GetExtension(Res)}", true);
                            File.Delete(Res);
                        }
                        Directory.Delete(tempFolder, true);
                    }
                }
                else
                {
                    if (!res.Any(f => f.Contains(".dll")))
                    {
                        string[] modDll = Directory.GetFiles(tempFolder, "*.dll", SearchOption.AllDirectories);
                        foreach (string dll in modDll)
                            File.Copy(dll, $@"{Properties.Settings.Default.modFolder}\{Path.GetFileName(dll)}", true);
                        foreach (string Mod in mods)
                        {
                            string[] Dll = Directory.GetFiles(Mod, "*.dll", SearchOption.AllDirectories);
                            if (Dll.Length == 0)
                            {
                                MoveDirectory(Mod, $@"{Properties.Settings.Default.installFolder}\{Path.GetFileName(Mod)}\");
                            }
                        }
                        foreach (string Res in res)
                        {
                            
                            File.Copy(Res, $@"{Properties.Settings.Default.installFolder}\{Path.GetFileNameWithoutExtension(Res)}({Path.GetFileNameWithoutExtension(mod)}){Path.GetExtension(Res)}", true);
                            File.Delete(Res);
                        }
                        Directory.Delete(tempFolder, true);
                    }
                    else
                    {
                        foreach (string Res in res)
                        {
                            if (Res.Contains("*.txt"))
                                File.Copy(Res, $@"{Properties.Settings.Default.installFolder}\{Path.GetFileNameWithoutExtension(Res)}({Path.GetFileNameWithoutExtension(mod)}){Path.GetExtension(Res)}", true);
                            else
                                File.Copy(Res, $@"{Properties.Settings.Default.modFolder}\{Path.GetFileName(Res)}", true);
                            File.Delete(Res);
                        }
                        Directory.Delete(tempFolder, true);
                    }
                }
            }
            else
            {
                if (mod.Contains("Assembly"))
                {
                    if (File.Exists($@"{Properties.Settings.Default.APIFolder}\{Path.GetFileName(mod)}"))
                    {
                        if (File.Exists($@"{Properties.Settings.Default.APIFolder}\{Path.GetFileName(mod)}.vanilla"))
                            File.Delete($@"{Properties.Settings.Default.APIFolder}\{Path.GetFileName(mod)}");
                        else
                            File.Move($@"{Properties.Settings.Default.APIFolder}\{Path.GetFileName(mod)}", $@"{Properties.Settings.Default.APIFolder}\{Path.GetFileName(mod)}.vanilla");
                    }
                    File.Copy(mod, $@"{Properties.Settings.Default.APIFolder}\{Path.GetFileName(mod)}", true);
                }
                else
                    File.Copy(mod, $@"{Properties.Settings.Default.modFolder}\{Path.GetFileName(mod)}", true);
            }
                
        }

        void restoreBackups()
        {
            string[] backup = Directory.GetFiles(Properties.Settings.Default.installFolder, "*.vanilla", SearchOption.AllDirectories);
            foreach (string file in backup)
            {
                if (File.Exists($@"{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}"))
                File.Delete($@"{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}");
                File.Move(file, $@"{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}");
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

        private void button1_Click(object send, EventArgs e)
        {
            restoreBackups();
            MessageBox.Show("Backups restored successfully!");            
            button1.Enabled = false;
        }

        bool api;
    }
}
