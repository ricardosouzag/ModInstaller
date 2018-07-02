using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ModInstaller
{
    public partial class ModManager : Form
    {
        public ModManager()
        {
            InitializeComponent();
        }

        #region Loading and building the mod manager

        private void Form2_Load(object sender, EventArgs e)
        {
            FillDefaultPaths();
            GetLocalInstallation();
            FillModsList();
            PopulateList();
            ResizeUI();
        }

        private void FillDefaultPaths()
        {
            defaultPaths.Add($@"Program Files (x86)/Steam/steamapps/Common/Hollow Knight");
            defaultPaths.Add($@"Program Files/Steam/steamapps/Common/Hollow Knight");
            defaultPaths.Add($@"Steam/steamapps/common/Hollow Knight");
            // Default steam installation path for Linux.
            defaultPaths.Add(System.Environment.GetEnvironmentVariable("HOME") + "/.steam/steam/steamapps/common/Hollow Knight");
        }

        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        private void GetLocalInstallation()
        {
            if (String.IsNullOrEmpty(Properties.Settings.Default.installFolder))
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();

                foreach (DriveInfo d in allDrives.Where(d => d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Removable))
                {
                    foreach (string path in defaultPaths)
                    {
                        if (!Directory.Exists($@"{d.Name}{path}")) continue;
                        SetDefaultPath($@"{d.Name}{path}");

                        // If user is on sane operating system with a /tmp folder, put temp files here.
                        // Reasoning:
                        // 1) /tmp usually has normal user write permissions. C:\temp might not.
                        // 2) /tmp is usually on a ramdisk. Less disk writing is always better.
                        if (Directory.Exists($@"{d.Name}tmp"))
                        {
                            if (Directory.Exists($@"{d.Name}tmp/HKmodinstaller"))
                            {
                                DeleteDirectory($@"{d.Name}tmp/HKmodinstaller");
                            }

                            Directory.CreateDirectory($@"{d.Name}tmp/HKmodinstaller");
                            Properties.Settings.Default.temp = $@"{d.Name}tmp/HKmodinstaller";
                        }
                        else
                        {
                            Properties.Settings.Default.temp = Directory.Exists($@"{d.Name}temp")
                                ? $@"{d.Name}tempMods" : $@"{d.Name}temp";
                        }

                        Properties.Settings.Default.Save();
                    }

                    if (!String.IsNullOrEmpty(Properties.Settings.Default.installFolder))
                        break;
                }
                if (String.IsNullOrEmpty(Properties.Settings.Default.installFolder))
                {
                    ManualPathLocation form3 = new ManualPathLocation();
                    Hide();
                    form3.FormClosed += ManualPathClosed;
                    form3.ShowDialog();
                }
                else
                {
                    Properties.Settings.Default.APIFolder = $@"{Properties.Settings.Default.installFolder}/hollow_knight_Data/Managed";
                    Properties.Settings.Default.modFolder = $@"{Properties.Settings.Default.APIFolder}/Mods";
                    Properties.Settings.Default.Save();
                }
                if (!Directory.Exists(Properties.Settings.Default.modFolder))
                {
                    Directory.CreateDirectory(Properties.Settings.Default.modFolder);
                }
            }
            
        }

        private static void SetDefaultPath(string path)
        {
            DialogResult dialogResult = MessageBox.Show("Is this your Hollow Knight installation path?\n" + path, "Path confirmation", MessageBoxButtons.YesNo);
            if (dialogResult != DialogResult.Yes) return;
            Properties.Settings.Default.installFolder = path;
            Properties.Settings.Default.Save();
        }

        private void FillModsList()
        {
            XDocument dllist = XDocument.Load("https://drive.google.com/uc?export=download&id=1HN5P35vvpFcjcYQ72XvZr35QxD09GUwh");
            XElement[] mods = dllist.Element("ModLinks")?.Element("ModList")?.Elements("ModLink").ToArray();
            foreach (XElement mod in mods)
            {
                if (!mod.Element("Dependencies").IsEmpty)
                {
                    modsList.Add(new Mod
                    {
                        Name = mod.Element("Name")?.Value,
                        Link = mod.Element("Link")?.Value,
                        Filename = mod.Element("Filename")?.Elements("string").Select(filename => filename.Value).ToList(),
                        Dependencies = mod.Element("Dependencies")?.Elements("string").Select(dependency => dependency.Value).ToList(),
                        Optional = mod.Element("Optional")?.Elements("string").Select(dependency => dependency.Value).ToList() ?? new List<string>()
                    });
                }
                else if (mod.Element("Name")?.Value == "Modding API")
                {
                    apilink = mod.Element("Link")?.Value;
                }
            }
        }

        private void PopulateList()
        {
            List<Mod> modsSortedList = modsList.OrderBy(mod => mod.Name).ToList();
            modsList = modsSortedList;

            DirectoryInfo modsFolder = new DirectoryInfo(Properties.Settings.Default.modFolder);
            FileInfo[] modsFiles = modsFolder.GetFiles("*.dll");

            if (!Directory.Exists(Properties.Settings.Default.modFolder + @"/Disabled"))
                Directory.CreateDirectory(Properties.Settings.Default.modFolder + @"/Disabled");

            DirectoryInfo disabledFolder = new DirectoryInfo(Properties.Settings.Default.modFolder + @"/Disabled");
            FileInfo[] disabledFiles = disabledFolder.GetFiles("*.dll");

            foreach (var modsFile in modsFiles)
            {
                Mod mod = modsList.Single(m => m.Filename.Contains(Path.GetFileNameWithoutExtension(modsFile.Name)));

                if (string.IsNullOrEmpty(mod.Name) || allMods.Any(f => f == mod.Name)) continue;
                allMods.Add(mod.Name);
                installedMods.Add(mod.Name);
                InstalledMods.Items.Add(mod.Name, CheckState.Checked);
                InstallList.Items.Add("Installed", CheckState.Checked);
            }

            foreach (var file in disabledFiles)
            {
                Mod mod = modsList.Single(m => m.Filename.Contains(Path.GetFileNameWithoutExtension(file.Name)));

                if (string.IsNullOrEmpty(mod.Name) || allMods.Any(f => f == mod.Name)) continue;
                allMods.Add(mod.Name);
                installedMods.Add(mod.Name);
                InstalledMods.Items.Add(mod.Name, CheckState.Unchecked);
                InstallList.Items.Add("Installed", CheckState.Checked);
            }

            foreach (Mod mod in modsList)
            {
                if (installedMods.Any(f => f.Equals(mod.Name))) continue;
                InstalledMods.Items.Add(mod.Name, CheckState.Indeterminate);
                InstallList.Items.Add("Check to install", CheckState.Unchecked);
                allMods.Add(mod.Name);
            }
        }

        private void ResizeUI()
        {
            groupBox1.AutoSize = true;
            groupBox1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            InstallList.AutoSize = true;
            InstalledMods.AutoSize = true;
            button1.Size = new Size(groupBox1.Width, 23);
            button2.Size = new Size(groupBox1.Width, 23);
            groupBox1.Top = 3;
            groupBox1.Left = 3;
            button1.Top = InstallList.Bottom + 9;
            button1.Left = 3;
            button2.Top = button1.Bottom;
            button2.Left = 3;

            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        #endregion

        #region Handling the left checkbox for enabling/disabling mods

        private void InstalledMods_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.CurrentValue == CheckState.Indeterminate)
                e.NewValue = InstallList.GetItemCheckState(e.Index) == CheckState.Checked
                    ? e.NewValue
                    : CheckState.Indeterminate;
            if (e.NewValue != CheckState.Checked) DisableMod(e);
            else EnableMod(e);
        }

        private void DisableMod(ItemCheckEventArgs e)
        {
            if (e.NewValue != CheckState.Unchecked) return;

            string modname = InstalledMods.Items[e.Index].ToString();

            foreach (string s in modsList.Single(m => m.Name == modname).Filename)
            {
                if (File.Exists($@"{Properties.Settings.Default.modFolder}/{s}.dll"))
                {
                    File.Move($@"{Properties.Settings.Default.modFolder}/{s}.dll",
                        $@"{Properties.Settings.Default.modFolder}/Disabled/{s}.dll");
                }
            }
        }

        private void EnableMod(ItemCheckEventArgs e)
        {
            string modname = InstalledMods.Items[e.Index].ToString();

            foreach (string s in modsList.Single(m => m.Name == modname).Filename)
            {
                if (File.Exists($@"{Properties.Settings.Default.modFolder}/Disabled/{s}.dll") &&
                    !File.Exists($@"{Properties.Settings.Default.modFolder}/{s}.dll"))
                {
                    File.Move($@"{Properties.Settings.Default.modFolder}/Disabled/{s}.dll",
                        $@"{Properties.Settings.Default.modFolder}/{s}.dll");
                }
            }
        }

        #endregion

        #region Handling the right checkbox for installing/uninstalling mods

        private void InstallList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.CurrentValue == CheckState.Indeterminate)
            {
                e.NewValue = CheckState.Indeterminate;
            }
            else if (InstallList.Items[e.Index].ToString() != "Installed" && e.NewValue == CheckState.Checked)
            {
                DownloadAndInstallMod(e);
            }
            else if (e.NewValue == CheckState.Unchecked)
            {
                UninstallMod(e);
            }
        }

        private void DownloadAndInstallMod(ItemCheckEventArgs e)
        {
            if (installedMods.Contains(InstalledMods.Items[e.Index])) return;
            string modName = InstalledMods.Items[e.Index].ToString();
            Mod mod = modsList.Single(m => m.Name == modName);

                DialogResult result = MessageBox.Show(text: $@"Do you want to install {modName}?", caption: "Confirm installation", buttons: MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    if (mod.Dependencies.Any())
                    {
                        foreach (string dependency in mod.Dependencies)
                        {
                            if (dependency == "Modding API")
                            {
                                if (Properties.Settings.Default.apiInstalled) continue;
                                Download(new Uri(apilink),
                                    $@"{Properties.Settings.Default.installFolder}/{dependency}.zip");
                                InstallApi($@"{Properties.Settings.Default.installFolder}/{dependency}.zip",
                                    Properties.Settings.Default.temp);
                                File.Delete($@"{Properties.Settings.Default.installFolder}/{dependency}.zip");
                                MessageBox.Show($@"{dependency} successfully installed!");
                            }
                            else
                            {
                                if (installedMods.Any(f => f.Equals(dependency))) continue;
                                Install(dependency);
                            }
                        }
                    }

                    if (mod.Optional.Any())
                    {
                        foreach (string dependency in mod.Optional)
                        {
                            if (installedMods.Any(f => f.Equals(dependency))) continue;
                            DialogResult depInstall = MessageBox.Show($"The mod author suggests installing {dependency} together with this mod.\nDo you want to install {dependency}?", "Confirm installation", MessageBoxButtons.YesNo);
                            if (depInstall != DialogResult.Yes) continue;
                            Install(dependency);
                            MessageBox.Show($@"{dependency} successfully installed!");
                        }
                    }
                    Install(modName);
                }
                else
                    e.NewValue = CheckState.Unchecked;
            
        }

        private static void Download(Uri uri,string path)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile(uri, path);
        }

        private void Install(string mod)
        {
            Download(new Uri(modsList.Single(m => m.Name == mod).Link),
                $@"{Properties.Settings.Default.modFolder}/{mod}.zip");
            InstallMods($@"{Properties.Settings.Default.modFolder}/{mod}.zip",
                Properties.Settings.Default.temp);
            File.Delete($@"{Properties.Settings.Default.modFolder}/{mod}.zip");
            InstallList.Items[InstalledMods.Items.IndexOf(mod)] = "Installed";
            InstallList.SetItemChecked(InstalledMods.Items.IndexOf(mod), true);
            InstalledMods.SetItemChecked(InstalledMods.Items.IndexOf(mod), true);
            MessageBox.Show($@"{mod} successfully installed!");
        }

        private void UninstallMod(ItemCheckEventArgs e)
        {
            if (e.NewValue != CheckState.Unchecked) return;

            string modname = InstalledMods.Items[e.Index].ToString();
            Mod mod = modsList.Single(m => m.Name == modname);

            DialogResult result = MessageBox.Show(text: $@"Do you want to remove {modname} from your computer?", caption: "Confirm removal", buttons: MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                foreach (string s in mod.Filename)
                {
                    if (File.Exists($@"{Properties.Settings.Default.modFolder}/{s}.dll"))
                    {
                        File.Delete($@"{Properties.Settings.Default.modFolder}/{s}.dll");
                    }
                }

                MessageBox.Show($@"{modname} successfully uninstalled!");
                InstallList.Items[e.Index] = "Check to install";
                InstalledMods.SetItemCheckState(e.Index, CheckState.Indeterminate);
                installedMods.Remove(modname);
            }
            else
                e.NewValue = CheckState.Checked;
        }

        #region Unpacking and moving/copying/deleting files

        private static void InstallApi(string api, string tempFolder)
        {
            ZipFile.ExtractToDirectory(api, tempFolder);
            IEnumerable<string> mods = Directory.EnumerateDirectories(tempFolder);
            IEnumerable<string> res = Directory.EnumerateFiles(tempFolder);
            if (!res.Any(f => f.Contains(".dll")))
            {
                string[] modDll = Directory.GetFiles(tempFolder, "*.dll", SearchOption.AllDirectories);
                foreach (string dll in modDll)
                    File.Copy(dll, $@"{Properties.Settings.Default.APIFolder}/{Path.GetFileName(dll)}", true);
                foreach (string Mod in mods)
                {
                    string[] Dll = Directory.GetFiles(Mod, "*.dll", SearchOption.AllDirectories);
                    if (Dll.Length == 0)
                    {
                        MoveDirectory(Mod, $@"{Properties.Settings.Default.installFolder}/{Path.GetFileName(Mod)}/");
                    }
                }
                foreach (string Res in res)
                {
                    File.Copy(Res, $@"{Properties.Settings.Default.installFolder}/{Path.GetFileNameWithoutExtension(Res)}({Path.GetFileNameWithoutExtension(api)}){Path.GetExtension(Res)}", true);
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
                            ? $@"{Properties.Settings.Default.installFolder}/{Path.GetFileNameWithoutExtension(Res)}({
                                    Path.GetFileNameWithoutExtension(api)
                                }){Path.GetExtension(Res)}"
                            : $@"{Properties.Settings.Default.modFolder}/{Path.GetFileName(Res)}", true);
                    File.Delete(Res);
                }
                Directory.Delete(tempFolder, true);
            }
            Properties.Settings.Default.apiInstalled = true;
            Properties.Settings.Default.Save();
        }

        private void InstallMods(string mod, string tempFolder)
        {
            if (Directory.Exists(Properties.Settings.Default.temp))
                Directory.Delete(tempFolder, true);
            if (!Directory.Exists(Properties.Settings.Default.modFolder)) Directory.CreateDirectory(Properties.Settings.Default.modFolder);
            {
                ZipFile.ExtractToDirectory(mod, tempFolder);
                IEnumerable<string> mods = Directory.EnumerateDirectories(tempFolder);
                IEnumerable<string> res = Directory.EnumerateFiles(tempFolder);

                if (!res.Any(f => f.Contains(".dll")))
                {
                    string[] modDll = Directory.GetFiles(tempFolder, "*.dll", SearchOption.AllDirectories);
                    foreach (string dll in modDll)
                    {
                        File.Copy(dll, $@"{Properties.Settings.Default.modFolder}/{Path.GetFileName(dll)}", true);
                    }
                    foreach (string Mod in mods)
                    {
                        string[] Dll = Directory.GetFiles(Mod, "*.dll", SearchOption.AllDirectories);
                        if (Dll.Length == 0)
                        {
                            MoveDirectory(Mod, $@"{Properties.Settings.Default.installFolder}/{Path.GetFileName(Mod)}/");
                        }
                    }
                    foreach (string Res in res)
                    {
                        File.Copy(Res, $@"{Properties.Settings.Default.installFolder}/{Path.GetFileNameWithoutExtension(Res)}({Path.GetFileNameWithoutExtension(mod)}){Path.GetExtension(Res)}", true);
                        File.Delete(Res);
                    }
                }
                else
                {
                    foreach (string Res in res)
                    {
                        File.Copy(Res,
                            Res.Contains("*.txt")
                                ? $@"{Properties.Settings.Default.installFolder}/{
                                        Path.GetFileNameWithoutExtension(Res)
                                    }({Path.GetFileNameWithoutExtension(mod)}){Path.GetExtension(Res)}"
                                : $@"{Properties.Settings.Default.modFolder}/{Path.GetFileName(Res)}", true);
                        File.Delete(Res);
                    }
                }
                Directory.Delete(tempFolder, true);
            }
            installedMods.Add(mod);
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
                        if (!File.Exists($@"{targetFolder}/{Path.GetFileName(targetFile)}.vanilla"))
                        {
                            File.Move(targetFile, $@"{targetFolder}/{Path.GetFileName(targetFile)}.vanilla");
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

        #endregion

        #endregion

        #region Event listeners

        private void InstallApiClick(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to install the modding API?", "Install confirmation", MessageBoxButtons.YesNo);
            if (result != DialogResult.Yes) return;
            Download(new Uri(apilink), $@"{Properties.Settings.Default.installFolder}/API.zip");
            InstallApi($@"{Properties.Settings.Default.installFolder}/API.zip", Properties.Settings.Default.temp);
            File.Delete($@"{Properties.Settings.Default.installFolder}/API.zip");
            MessageBox.Show("Modding API successfully installed!");
        }

        private void ManualInstallClick(object sender, EventArgs e)
        {
            ManualInstall form1 = new ManualInstall(this);
            form1.FormClosed += ManualInstallClosed;
            form1.Show();
        }

        private void ManualInstallClosed(object sender, FormClosedEventArgs e)
        {
            button1.Enabled = (Directory.GetFiles(Properties.Settings.Default.installFolder, "*.vanilla", SearchOption.AllDirectories)).Length > 0;
        }

        private void ManualPathClosed(object sender, FormClosedEventArgs e)
        {
            Show();
            if (Directory.Exists($@"/tmp"))
            {
                if (Directory.Exists($@"/tmp/HKmodinstaller"))
                {
                    DeleteDirectory($@"/tmp/HKmodinstaller");
                }
                Directory.CreateDirectory($@"/tmp/HKmodinstaller");
                Properties.Settings.Default.temp = $@"/tmp/HKmodinstaller";
            }
            else
            {
                Properties.Settings.Default.temp =
                    Directory.Exists($@"{Path.GetPathRoot(Properties.Settings.Default.installFolder)}temp")
                        ? $@"{Path.GetPathRoot(Properties.Settings.Default.installFolder)}tempMods"
                        : $@"{Path.GetPathRoot(Properties.Settings.Default.installFolder)}temp";
            }

            Properties.Settings.Default.Save();
        }

        #endregion

        #region Setting up default fields

        private List<string> defaultPaths = new List<string>();
        private List<string> allMods = new List<string>();
        private List<string> installedMods = new List<string>();
        private struct Mod
        {
            public string Name { get; set; }

            public List<string> Filename { get; set; }

            public string Link { get; set; }

            public List<string> Dependencies { get; set; }

            public List<string> Optional { get; set; }
        }
        private  List<Mod> modsList = new List<Mod>();
        string apilink;

        #endregion
    }
}
