using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModInstaller
{
    public partial class Form2 : Form
    {


        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to install the modding API?", "Install confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile("https://drive.google.com/uc?export=download&id=1PUulDJDeHEfIEl1hAithE1XLT8AXqOZ4", $@"{Properties.Settings.Default.installFolder}\API.zip");
                installMods($@"{Properties.Settings.Default.installFolder}\API.zip", Properties.Settings.Default.temp);
                //ZipFile.ExtractToDirectory(sourceArchiveFileName: $@"{Properties.Settings.Default.installFolder}\API.zip", destinationDirectoryName: Properties.Settings.Default.temp);
                //IEnumerable<string> mods = Directory.EnumerateDirectories(Properties.Settings.Default.temp);
                //IEnumerable<string> res = Directory.EnumerateFiles(Properties.Settings.Default.temp);
                //MoveDirectory(mods.ElementAt<string>(0), $@"{Properties.Settings.Default.installFolder}\hollow_knight_data\");
                //foreach (string Res in res)
                //{
                //    MessageBox.Show($@"Installing {Res}");
                //    File.Copy(Res, $@"{Properties.Settings.Default.installFolder}\{Path.GetFileNameWithoutExtension(Res)}({Path.GetFileNameWithoutExtension($@"{Properties.Settings.Default.installFolder}\API.zip")}){Path.GetExtension(Res)}", true);
                //    MessageBox.Show($@"Installing {Res}");
                //    if (File.Exists(Res))
                //    File.Delete(Res.ToString());
                //    MessageBox.Show($@"Installing {Res}");
                //}                              
                //Directory.Delete($@"{Properties.Settings.Default.installFolder}\API.zip");
                //Directory.Delete(Properties.Settings.Default.temp, true);
                MessageBox.Show("Modding API successfully installed!");
            }
        }

        private void PopulateCheckBox(CheckedListBox modlist, CheckedListBox installList, string Folder, string FileType)
        {
            DirectoryInfo dinfo = new DirectoryInfo(path: Folder);
            FileInfo[] Files = dinfo.GetFiles(searchPattern: FileType);
            foreach (FileInfo file in Files)
            {
                if (!installedMods.Contains(item: file.Name))
                {
                    modlist.Items.Add(item: Path.GetFileNameWithoutExtension(file.Name));
                    installedMods.Add(item: file.Name);
                    installList.Items.Add("Installed");
                }
                else
                    File.Delete(path: Folder + $@"/{file.Name}");
            }
        }

        private void PopulateCheckBoxLink(CheckedListBox modlist, CheckedListBox installList, Dictionary<string, string> downloadList)
        {
            foreach (KeyValuePair<string, string> kvp in downloadList)
            {
                if (!installedMods.Contains(item: $@"{kvp.Value}.dll"))
                {
                    modlist.Items.Add(item: kvp.Value);
                    installedMods.Add(item: $@"{kvp.Value}.dll");
                    installList.Items.Add("Not Installed");
                }
            }
        }

        private void GetDownloadLinks()
        {
            downloadList.Add("https://drive.google.com/uc?export=download&id=0BzihlMHqh5UpTGEwVHJXc05NeGM", "RandomizerMod");
            downloadList.Add("https://drive.google.com/uc?export=download&id=0BzihlMHqh5UpWXhoS2hyZ2JUMEU", "DebugMod");
            downloadList.Add("https://drive.google.com/uc?export=download&id=0BzihlMHqh5UpUVQ5ZGlEdlcxOXM", "CharmNotchMod");
            downloadList.Add("https://drive.google.com/uc?export=download&id=0B1-JBoX3q-gVYkUwSjNjZFNwTXM", "NightmareGodGrimm");
            downloadList.Add("https://drive.google.com/uc?export=download&id=1yr832lq_jSCvX8Ve5qVIWlIl4kVVAmPD", "MoreSaves");
            downloadList.Add("https://drive.google.com/uc?export=download&id=1isjE6W0LcaoOqxKELrr_vgDDvnEL3-oa", "HPBar");
            downloadList.Add("https://drive.google.com/uc?export=download&id=1beVdRrkgaE0X0VZUMCklYI9CfOkQqbh1", "BossRush");
            downloadList.Add("https://drive.google.com/uc?export=download&id=1YsthSD5-k8vVtK4orQBz_mZIF4_w-I4P", "BonfireMod");
            downloadList.Add("https://drive.google.com/uc?export=download&id=11u4QTDUeq_09t8DjXrMY0qIyKaWGz7Gz", "Blackmoth");
            downloadList.Add("https://drive.google.com/uc?export=download&id=1_VkTWanS5Tx8H50RAc2S3zEX_QhADJuV", "HellMod");
        }

        public Form2()
        {
            InitializeComponent();
        }

        private Form1 mainForm = null;

        public Form2(Form callingForm)
        {
            mainForm = callingForm as Form1;
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            GetDownloadLinks();
            fillModManager();
        }

        private void InstalledMods_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                if (File.Exists(path: Properties.Settings.Default.modFolder + @"\Disabled\" + installedMods[e.Index]) && !File.Exists(path: Properties.Settings.Default.modFolder + @"\" + installedMods[e.Index]))
                {
                    File.Move(Properties.Settings.Default.modFolder + @"\Disabled\" + installedMods[e.Index], Properties.Settings.Default.modFolder + @"\" + installedMods[e.Index]);
                }
            }
            else if (File.Exists(path: $@"{Properties.Settings.Default.modFolder}\{installedMods[e.Index]}"))
            {
                File.Move(Properties.Settings.Default.modFolder + @"\" + installedMods[e.Index], Properties.Settings.Default.modFolder + @"\Disabled\" + installedMods[e.Index]);
            }
        }

        private void InstallList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (InstallList.Items[e.Index].ToString() != "Installed" && e.NewValue == CheckState.Checked)
            {
                WebClient webClient = new WebClient();
                foreach (KeyValuePair<string, string> kvp in downloadList)
                {
                    if (kvp.Value == Path.GetFileNameWithoutExtension(installedMods[e.Index]))
                    {
                        DialogResult result = MessageBox.Show(text: $@"Do you want to install {kvp.Value}?", caption: "Confirm installation", buttons: MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            webClient.DownloadFile(kvp.Key, $@"{Properties.Settings.Default.modFolder}\{kvp.Value}.zip");
                            installMods($@"{Properties.Settings.Default.modFolder}\{kvp.Value}.zip", Properties.Settings.Default.temp);
                            File.Delete($@"{Properties.Settings.Default.modFolder}\{kvp.Value}.zip");
                            MessageBox.Show($@"{kvp.Value} successfully installed!");
                            InstallList.Items[e.Index] = "Installed";
                            InstallList.SetItemChecked(e.Index, true);
                            InstalledMods.SetItemChecked(e.Index, true);
                        }
                        else
                            e.NewValue = CheckState.Unchecked;
                    }
                }
            }
            else if (e.NewValue == CheckState.Unchecked)
            {
                DialogResult result = MessageBox.Show(text: $@"Do you want to remove {Path.GetFileNameWithoutExtension(installedMods[e.Index])} from your computer?", caption: "Confirm removal", buttons: MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    File.Delete($@"{Properties.Settings.Default.modFolder}\{installedMods[e.Index]}");
                    MessageBox.Show($@"{Path.GetFileNameWithoutExtension(installedMods[e.Index])} successfully uninstalled!");
                    InstallList.Items[e.Index] = "Not Installed";
                    InstalledMods.SetItemChecked(e.Index, false);
                }
                else
                    e.NewValue = CheckState.Unchecked;
            }
                
        }

        public void installMods(string mod, string tempFolder)
        {
            if (Path.GetExtension(mod) == ".zip")
            {
                ZipFile.ExtractToDirectory(sourceArchiveFileName: mod, destinationDirectoryName: tempFolder);
                IEnumerable<string> mods = Directory.EnumerateDirectories(tempFolder);
                IEnumerable<string> res = Directory.EnumerateFiles(tempFolder);                                
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
                    File.Copy(mod, Properties.Settings.Default.modFolder);
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
                        if (!File.Exists($@"{targetFolder}\{Path.GetFileName(targetFile)}.vanilla"))
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

        private void fillModManager()
        {
            PopulateCheckBox(InstalledMods, InstallList, Properties.Settings.Default.modFolder, "*.dll");
            for (int i = 0; i < InstalledMods.Items.Count; i++)
            {
                InstalledMods.SetItemChecked(i, true);
                InstallList.SetItemChecked(i, true);
            }
            if (!Directory.Exists(Properties.Settings.Default.modFolder + @"\Disabled"))
                Directory.CreateDirectory(Properties.Settings.Default.modFolder + @"\Disabled");
            PopulateCheckBox(InstalledMods, InstallList, Properties.Settings.Default.modFolder + @"\Disabled", "*.dll");
            PopulateCheckBoxLink(InstalledMods, InstallList, downloadList);
            for (int i = 0; i < InstallList.Items.Count ; i++)
            {
                InstallList.SetItemChecked(i, InstallList.Items[i].ToString() == "Installed");
            }           
        }

        private List<string> installedMods = new List<string>();
        private Dictionary<string,string> downloadList = new Dictionary<string, string>();
    }
}
