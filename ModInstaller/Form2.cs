using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ModInstaller
{
    public partial class Form2 : Form
    {
        private static void Download(Uri uri,string path)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile(uri, path);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to install the modding API?", "Install confirmation", MessageBoxButtons.YesNo);
            if (result != DialogResult.Yes) return;
            Download(new Uri(apilink), $@"{Properties.Settings.Default.installFolder}\API.zip");
            InstallApi($@"{Properties.Settings.Default.installFolder}\API.zip", Properties.Settings.Default.temp);
            File.Delete($@"{Properties.Settings.Default.installFolder}\API.zip");
            MessageBox.Show("Modding API successfully installed!");
        }

        private void PopulateCheckBox(CheckedListBox modlist, CheckedListBox installList, string Folder, string FileType, CheckState check)
        {
            DirectoryInfo dinfo = new DirectoryInfo(Folder);
            FileInfo[] Files = dinfo.GetFiles(FileType);
            foreach (FileInfo file in Files)
            {
                if (!installedMods.Contains(file.Name))
                {
                    installedMods.Add(file.Name);
                    modlist.Items.Add(Path.GetFileNameWithoutExtension(file.Name), check);
                    installList.Items.Add("Installed", downloadList.ContainsKey(Path.GetFileNameWithoutExtension(file.Name)) ? CheckState.Checked : CheckState.Indeterminate);
                }
                else
                    File.Delete(Folder + $@"/{file.Name}");
            }
        }

        private void PopulateCheckBoxLink(CheckedListBox modlist, CheckedListBox installList, Dictionary<string, string> downloadList)
        {
            foreach (KeyValuePair<string, string> kvp in downloadList)
            {
                if (installedMods.Contains($@"{kvp.Key}.dll")) continue;
                modlist.Items.Add(kvp.Key, CheckState.Indeterminate);
                installedMods.Add($@"{kvp.Key}.dll");
                installList.Items.Add("Check to install");
            }
        }

        private void GetDownloadLinks()
        {
            XDocument dllist = XDocument.Load("https://drive.google.com/uc?export=download&id=1HN5P35vvpFcjcYQ72XvZr35QxD09GUwh");
            XElement[] mods = dllist.Element("ModLinks")?.Element("ModList")?.Elements("ModLink").ToArray();
            foreach (XElement mod in mods)
            {
                if (!mod.Element("Dependencies").IsEmpty)
                {
                    downloadList.Add(Regex.Replace(mod.Element("Name")?.Value, @"\s|\\|\n|_", ""), mod.Element("Link")?.Value);
                    dependencies.Add(Regex.Replace(mod.Element("Name")?.Value, @"\s|\\|\n|_", ""), mod.Element("Dependencies")?.Elements("string").Select(dependency => dependency.Value).ToList());
                }
                else if (mod.Element("Name")?.Value == "Modding API")
                {
                    apilink = mod.Element("Link")?.Value;
                }
            }
            //https://drive.google.com/uc?export=download&id=1HN5P35vvpFcjcYQ72XvZr35QxD09GUwh

            //downloadList.Add("https://drive.google.com/uc?export=download&id=0BzihlMHqh5UpTGEwVHJXc05NeGM", "RandomizerMod");
            //downloadList.Add("https://drive.google.com/uc?export=download&id=0BzihlMHqh5UpWXhoS2hyZ2JUMEU", "DebugMod");
            //downloadList.Add("https://drive.google.com/uc?export=download&id=0BzihlMHqh5UpUVQ5ZGlEdlcxOXM", "CharmNotchMod");
            //downloadList.Add("https://drive.google.com/uc?export=download&id=0B1-JBoX3q-gVYkUwSjNjZFNwTXM", "NightmareGodGrimm");
            //downloadList.Add("https://drive.google.com/uc?export=download&id=1yr832lq_jSCvX8Ve5qVIWlIl4kVVAmPD", "MoreSaves");
            //downloadList.Add("https://drive.google.com/uc?export=download&id=1isjE6W0LcaoOqxKELrr_vgDDvnEL3-oa", "HPBar");
            //downloadList.Add("https://drive.google.com/uc?export=download&id=1beVdRrkgaE0X0VZUMCklYI9CfOkQqbh1", "BossRush");
            //downloadList.Add("https://drive.google.com/uc?export=download&id=1YsthSD5-k8vVtK4orQBz_mZIF4_w-I4P", "BonfireMod");
            //downloadList.Add("https://drive.google.com/uc?export=download&id=11u4QTDUeq_09t8DjXrMY0qIyKaWGz7Gz", "Blackmoth");
            //downloadList.Add("https://drive.google.com/uc?export=download&id=1BeFOhGulOCOzbuSdHgOUpnKZ8oRFvxeJ", "EnemyHPBar"); 
            //downloadList.Add("https://drive.google.com/uc?export=download&id=1_VkTWanS5Tx8H50RAc2S3zEX_QhADJuV", "HellMod");
            //downloadList.Add("https://drive.google.com/uc?export=download&id=1LG4gnSiSPZWbLM-6e5DM0ACC0zf_jZX9", "EnemyRandomizer");
            //downloadList.Add("https://drive.google.com/uc?export=download&id=1mZgGfNDpR4QyTfQ0qPP9vkMw8900iTPM", "Mantis_Gods");
        }

        public Form2()
        {
            InitializeComponent();
        }

        private void CenterControl(Control control)
        {
            control.Location = new Point((Width - control.Width) / 2, control.Location.Y);
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            FillDefaultPaths();
            //Finding the local installation path for Hollow Knight
            if (Properties.Settings.Default.installFolder == "")
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();

                foreach (DriveInfo d in allDrives)
                {
                    if (d.DriveFormat == "NTFS")
                    {
                        foreach (string path in defaultPaths)
                        {
                            if (Directory.Exists(path: $@"{d.Name}{path}"))
                            {
                                SetDefaultPath(path: $@"{d.Name}{path}");
                                if (Directory.Exists($@"{d.Name}temp"))
                                    Properties.Settings.Default.temp = $@"{d.Name}tempMods";
                                else
                                    Properties.Settings.Default.temp = $@"{d.Name}temp";
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
                    Hide();
                    form3.FormClosed += form3_FormClosed;
                    form3.Show();
                }
                else
                {
                    Properties.Settings.Default.APIFolder = $@"{Properties.Settings.Default.installFolder}\hollow_knight_data\Managed";
                    Properties.Settings.Default.modFolder = $@"{Properties.Settings.Default.APIFolder}\Mods";
                    Properties.Settings.Default.Save();
                }
            }
            if (!Directory.Exists(Properties.Settings.Default.modFolder))
            {
                Directory.CreateDirectory(Properties.Settings.Default.modFolder);
            }
            installedMods = new List<string>();
            GetDownloadLinks();
            FillModManager();
            groupBox1.AutoSize = true;
            groupBox1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            InstallList.AutoSize = true;
            InstalledMods.AutoSize = true;
            button1.Size = new Size(groupBox1.Width, 23);
            button2.Size = new Size(groupBox1.Width, 23);

            CenterControl(groupBox1);
            CenterControl(button1);
            CenterControl(button2);
            groupBox1.Top = 3;
            button1.Top = InstallList.Bottom + 9;
            button2.Top = button1.Bottom;

            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        private void InstalledMods_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue != CheckState.Checked)
            {
                if (e.NewValue != CheckState.Unchecked) return;
                if (File.Exists($@"{Properties.Settings.Default.modFolder}\{installedMods[e.Index]}"))
                {
                    File.Move(Properties.Settings.Default.modFolder + @"\" + installedMods[e.Index],
                        Properties.Settings.Default.modFolder + @"\Disabled\" + installedMods[e.Index]);
                }
            }
            else
            {
                if (File.Exists(Properties.Settings.Default.modFolder + @"\Disabled\" + installedMods[e.Index]) &&
                    !File.Exists(Properties.Settings.Default.modFolder + @"\" + installedMods[e.Index]))
                {
                    File.Move(Properties.Settings.Default.modFolder + @"\Disabled\" + installedMods[e.Index],
                        Properties.Settings.Default.modFolder + @"\" + installedMods[e.Index]);
                }
            }
        }

        private void InstallList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.CurrentValue == CheckState.Indeterminate)
            {
                e.NewValue = CheckState.Indeterminate;
            }
            else if (InstallList.Items[e.Index].ToString() != "Installed" && e.NewValue == CheckState.Checked)
            {         
                foreach (KeyValuePair<string, string> kvp in downloadList)
                {
                    if (kvp.Key != Path.GetFileNameWithoutExtension(installedMods[e.Index])) continue;
                    DialogResult result = MessageBox.Show(text: $@"Do you want to install {kvp.Key}?", caption: "Confirm installation", buttons: MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        foreach (string dependency in dependencies[kvp.Key])
                        {
                            if (dependency == "Modding API")
                            {
                                if (Properties.Settings.Default.apiInstalled) continue;
                                Download(new Uri(apilink),
                                    $@"{Properties.Settings.Default.installFolder}\{dependency}.zip");
                                InstallApi($@"{Properties.Settings.Default.installFolder}\{dependency}.zip",
                                    Properties.Settings.Default.temp);
                                File.Delete($@"{Properties.Settings.Default.installFolder}\{dependency}.zip");
                            }
                            else
                            {
                                if (installedMods.Contains(dependency + ".dll")) continue;
                                DialogResult depInstall = MessageBox.Show(
                                    text: $"Dependency {dependency} not found.\nDo you want to install {dependency}?",
                                    caption: "Confirm installation", buttons: MessageBoxButtons.YesNo);
                                if (depInstall != DialogResult.Yes) continue;
                                Download(new Uri(downloadList[dependency]),
                                    $@"{Properties.Settings.Default.modFolder}\{dependency}.zip");
                                InstallMods($@"{Properties.Settings.Default.modFolder}\{dependency}.zip",
                                    Properties.Settings.Default.temp);
                                File.Delete($@"{Properties.Settings.Default.modFolder}\{dependency}.zip");
                                InstallList.Items[InstalledMods.Items.IndexOf(dependency)] = "Installed";
                                InstallList.SetItemChecked(InstalledMods.Items.IndexOf(dependency), true);
                                InstalledMods.SetItemChecked(InstalledMods.Items.IndexOf(dependency), true);
                            }

                            MessageBox.Show($@"{dependency} successfully installed!");
                            installedDependencies.Add(dependency);
                        }
                        Download(new Uri(kvp.Value), $@"{Properties.Settings.Default.modFolder}\{kvp.Key}.zip");
                        InstallMods($@"{Properties.Settings.Default.modFolder}\{kvp.Key}.zip", Properties.Settings.Default.temp);                            
                        File.Delete($@"{Properties.Settings.Default.modFolder}\{kvp.Key}.zip");
                        MessageBox.Show($@"{kvp.Key} successfully installed!");
                        InstallList.Items[e.Index] = "Installed";
                        InstallList.SetItemChecked(e.Index, true);
                        InstalledMods.SetItemChecked(e.Index, true);
                    }
                    else
                        e.NewValue = CheckState.Unchecked;
                }
            }
            else if (e.NewValue == CheckState.Unchecked)
            {
                DialogResult result = MessageBox.Show(text: $@"Do you want to remove {Path.GetFileNameWithoutExtension(installedMods[e.Index])} from your computer?", caption: "Confirm removal", buttons: MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    List<string> mods = Directory.EnumerateFiles(Properties.Settings.Default.modFolder).ToList();
                    foreach (string mod in mods)
                    {
                        if (Regex.Replace(mod, @"\s|\\|\n|_", "") == Regex.Replace($@"{Properties.Settings.Default.modFolder}\{installedMods[e.Index]}", @"\s|\\|\n|_", ""))
                        {
                            File.Delete(mod);
                        }
                    }
                    MessageBox.Show($@"{Path.GetFileNameWithoutExtension(installedMods[e.Index])} successfully uninstalled!");
                    InstallList.Items[e.Index] = "Check to install";
                    InstalledMods.SetItemChecked(e.Index, false);
                }
                else
                    e.NewValue = CheckState.Unchecked;
            }                
        }

        private static void InstallApi(string api, string tempFolder)
        {
            ZipFile.ExtractToDirectory(sourceArchiveFileName: api, destinationDirectoryName: tempFolder);
            IEnumerable<string> mods = Directory.EnumerateDirectories(tempFolder);
            IEnumerable<string> res = Directory.EnumerateFiles(tempFolder);
            if (!res.Any(f => f.Contains(".dll")))
            {
                string[] modDll = Directory.GetFiles(tempFolder, "*.dll", SearchOption.AllDirectories);
                foreach (string dll in modDll)
                    File.Copy(dll, $@"{Properties.Settings.Default.APIFolder}\{Path.GetFileName(dll)}", true);
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
                    File.Copy(Res, $@"{Properties.Settings.Default.installFolder}\{Path.GetFileNameWithoutExtension(Res)}({Path.GetFileNameWithoutExtension(api)}){Path.GetExtension(Res)}", true);
                    File.Delete(Res);
                }
                Directory.Delete(tempFolder, true);
            }
            else
            {
                foreach (string Res in res)
                {
                    File.Copy(Res,
                        Res.Contains("*.txt")
                            ? $@"{Properties.Settings.Default.installFolder}\{Path.GetFileNameWithoutExtension(Res)}({
                                    Path.GetFileNameWithoutExtension(api)
                                }){Path.GetExtension(Res)}"
                            : $@"{Properties.Settings.Default.modFolder}\{Path.GetFileName(Res)}", true);
                    File.Delete(Res);
                }
                Directory.Delete(tempFolder, true);
            }
            Properties.Settings.Default.apiInstalled = true;
        }

        private static void InstallMods(string mod, string tempFolder)
        {
            if (Directory.Exists(Properties.Settings.Default.temp))
                Directory.Delete(tempFolder, true);
            if (!Directory.Exists(Properties.Settings.Default.modFolder)) Directory.CreateDirectory(Properties.Settings.Default.modFolder);
            {
                ZipFile.ExtractToDirectory(sourceArchiveFileName: mod, destinationDirectoryName: tempFolder);
                IEnumerable<string> mods = Directory.EnumerateDirectories(tempFolder);
                IEnumerable<string> res = Directory.EnumerateFiles(tempFolder);

                //foreach (string folder in mods)
                //{
                //    if (folder == "Mods")
                //    {
                //        MoveDirectory(folder, Properties.Settings.Default.modFolder);
                //    }
                //    else if (folder == "Managed")
                //    {
                //        MoveDirectory(folder, Properties.Settings.Default.APIFolder);
                //    }
                //    else
                //    {
                //        MoveDirectory(folder, $@"{Properties.Settings.Default.installFolder}\{Path.GetFileName(folder)}");
                //    }
                //}
                //foreach (string Res in res)
                //{
                //    if (Res.Contains(".txt") || Res.Contains(".md"))
                //    {
                //        File.Copy(Res, $@"{Properties.Settings.Default.installFolder}\{Path.GetFileNameWithoutExtension(Res)}({Path.GetFileNameWithoutExtension(mod)}){Path.GetExtension(Res)}", true);
                //    }
                //    else
                //    {
                //        File.Copy(Res, $@"{Properties.Settings.Default.modFolder}\{Path.GetFileName(Res)}", true);
                //    }

                //    File.Delete(Res);
                //}
                if (!res.Any(f => f.Contains(".dll")))
                {
                    string[] modDll = Directory.GetFiles(tempFolder, "*.dll", SearchOption.AllDirectories);
                    foreach (string dll in modDll)
                    {
                        File.Copy(dll, $@"{Properties.Settings.Default.modFolder}\{Path.GetFileName(dll)}", true);
                    }
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
                }
                else
                {
                    foreach (string Res in res)
                    {
                        File.Copy(Res,
                            Res.Contains("*.txt")
                                ? $@"{Properties.Settings.Default.installFolder}\{
                                        Path.GetFileNameWithoutExtension(Res)
                                    }({Path.GetFileNameWithoutExtension(mod)}){Path.GetExtension(Res)}"
                                : $@"{Properties.Settings.Default.modFolder}\{Path.GetFileName(Res)}", true);
                        File.Delete(Res);
                    }
                }
                Directory.Delete(tempFolder, true);
            }
        }

        private static void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                                 .GroupBy(Path.GetDirectoryName);
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

        private void FillModManager()
        {
            PopulateCheckBox(InstalledMods, InstallList, Properties.Settings.Default.modFolder, "*.dll", CheckState.Checked);
            if (!Directory.Exists(Properties.Settings.Default.modFolder + @"\Disabled"))
                Directory.CreateDirectory(Properties.Settings.Default.modFolder + @"\Disabled");
            PopulateCheckBox(InstalledMods, InstallList, Properties.Settings.Default.modFolder + @"\Disabled", "*.dll", CheckState.Unchecked);
            PopulateCheckBoxLink(InstalledMods, InstallList, downloadList);        
        }

        private void form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            button1.Enabled = (Directory.GetFiles(Properties.Settings.Default.installFolder, "*.vanilla", SearchOption.AllDirectories)).Length > 0;
        }

        private void form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            Show();
            Properties.Settings.Default.temp = Directory.Exists($@"{Path.GetPathRoot(Properties.Settings.Default.installFolder)}temp") ? $@"{Path.GetPathRoot(Properties.Settings.Default.installFolder)}tempMods" : $@"{Path.GetPathRoot(Properties.Settings.Default.installFolder)}temp";
            Properties.Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1(this);
            form1.FormClosed += form1_FormClosed;
            form1.Show();
        }

        private void FillDefaultPaths()
        {
            defaultPaths.Add($@"Program Files (x86)\Steam\steamapps\Common\Hollow Knight");
            defaultPaths.Add($@"Program Files\Steam\steamapps\Common\Hollow Knight");
            defaultPaths.Add($@"Steam\steamapps\common\Hollow Knight");
        }


        private static void SetDefaultPath(string path)
        {
            DialogResult dialogResult = MessageBox.Show(text: "Is this your Hollow Knight installation path?\n" + path, caption: "Path confirmation", buttons: MessageBoxButtons.YesNo);
            if (dialogResult != DialogResult.Yes) return;
            Properties.Settings.Default.installFolder = path;
            Properties.Settings.Default.Save();
        }


        private List<string> defaultPaths = new List<string>();
        private List<string> installedMods = new List<string>();
        private Dictionary<string,string> downloadList = new Dictionary<string, string>();
        private Dictionary<string,List<string>> dependencies = new Dictionary<string, List<string>>();
        private List<string> installedDependencies = new List<string>();
        string apilink;
    }
}
