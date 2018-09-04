using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Security.Cryptography;

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
            CheckUpdate();
            GetCurrentOS();
            FillDefaultPaths();
            GetLocalInstallation();
            PiracyCheck();
            FillModsList();
            CheckApiInstalled();
            PopulateList();
            ResizeUI();
            Text = "Mod Manager " + version;
        }

        private void CheckUpdate()
        {
            string dir = Directory.GetCurrentDirectory();
            string file = Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location);
            XDocument dllist = new XDocument();

            if (File.Exists(dir + @"/lol.exe"))
                File.Delete(dir + @"/lol.exe");

            if (File.Exists(dir + @"/AU.exe"))
                File.Delete(dir + @"/AU.exe");

            try
            {
                dllist = XDocument.Load("https://drive.google.com/uc?export=download&id=1HN5P35vvpFcjcYQ72XvZr35QxD09GUwh");
            }
            catch (Exception)
            {
                ConnectionFailedForm form4 = new ConnectionFailedForm(this);
                form4.Closed += Form4_Closed;
                Hide();
                form4.ShowDialog();
            }

            XElement installer = dllist.Element("ModLinks")?.Element("Installer");

            if (installer.Element("SHA1")?.Value != GetSHA1(dir + $@"/{file}"))
            {
                WebClient dl = new WebClient();
                dl.DownloadFile(new Uri(installer.Element("AULink")?.Value), dir + @"/AU.exe");
                Process process = new Process
                {
                    StartInfo =
                    {
                        FileName = dir + @"/AU.exe",
                        Arguments = "\"" + dir + "\"" + " " + installer.Element("Link")?.Value + " " + file
                    }
                };
                process.Start();
                Application.Exit();
            }
        }

        private void GetCurrentOS()
        {
            int p = (int)Environment.OSVersion.Platform;
            switch (p)
            {
                case 4:
                case 128:
                    OS = "Linux";
                    break;
                case 6:
                    OS = "MacOS";
                    break;
                default:
                    OS = "Windows";
                    break;
            }
        }

        private void FillDefaultPaths()
        {
            switch (OS)
            {
                case "Windows":
                    //Default Steam and GOG install paths for Windows.
                    defaultPaths.Add(@"Program Files (x86)/Steam/steamapps/Common/Hollow Knight");
                    defaultPaths.Add(@"Program Files/Steam/steamapps/Common/Hollow Knight");
                    defaultPaths.Add(@"Steam/steamapps/common/Hollow Knight");
                    defaultPaths.Add(@"Program Files (x86)/GOG Galaxy/Games/Hollow Knight");
                    defaultPaths.Add(@"Program Files/GOG Galaxy/Games/Hollow Knight");
                    defaultPaths.Add(@"GOG Galaxy/Games/Hollow Knight");
                    break;
                case "Linux":
                    // Default steam installation path for Linux.
                    defaultPaths.Add(System.Environment.GetEnvironmentVariable("HOME") + "/.steam/steam/steamapps/common/Hollow Knight");
                    break;

            }
        }
        
        private void GetLocalInstallation()
        {
            if (String.IsNullOrEmpty(Properties.Settings.Default.installFolder))
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();

                foreach (DriveInfo d in allDrives.Where(d => d.DriveType == DriveType.Fixed))
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

                        if (!String.IsNullOrEmpty(Properties.Settings.Default.installFolder))
                            break;
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
            }
            if (!Directory.Exists(Properties.Settings.Default.modFolder))
            {
                Directory.CreateDirectory(Properties.Settings.Default.modFolder);
            }
        }

        private static void SetDefaultPath(string path)
        {
            DialogResult dialogResult = MessageBox.Show("Is this your Hollow Knight installation path?\n" + path, "Path confirmation", MessageBoxButtons.YesNo);
            if (dialogResult != DialogResult.Yes) return;
            Properties.Settings.Default.installFolder = path;
            Properties.Settings.Default.Save();
        }

        private void PiracyCheck()
        {
            if (OS != "Windows") return;
            if (File.Exists(Properties.Settings.Default.installFolder + @"/Galaxy.dll") ||
                File.Exists(Properties.Settings.Default.installFolder + @"/steam_api.dll") ||
                File.Exists(Properties.Settings.Default.installFolder + @"/steam_appid.txt") ||
                Path.GetFileName(Properties.Settings.Default.installFolder) != "Hollow Knight Godmaster" ) return;
            MessageBox.Show("Please purchase the game before attempting to play it.");
            Process.Start("https://store.steampowered.com/app/367520/Hollow_Knight/");
            //Directory.Delete(Properties.Settings.Default.installFolder, true);
//            Directory.Move(Properties.Settings.Default.installFolder + @"/hollow_knight_Data", Properties.Settings.Default.installFolder + @"/holIow_knight_Data");
//            Application.Exit();
        }

        public void FillModsList()
        {
            XDocument dllist;
            try
            {
                dllist =
                    XDocument.Load("https://drive.google.com/uc?export=download&id=1HN5P35vvpFcjcYQ72XvZr35QxD09GUwh");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                ConnectionFailedForm form4 = new ConnectionFailedForm(this);
                form4.Closed += Form4_Closed;
                Hide();
                form4.ShowDialog();
                return;
            }
            
            var mods = dllist.Element("ModLinks")?.Element("ModList")?.Elements("ModLink").ToArray();

            foreach (XElement mod in mods)
            {
                if (mod.Element("Name")?.Value == "ModCommon")
                {
                    modcommonLink = mod.Element("Link")?.Value;
                    modcommonSHA1 = mod.Element("Files")?.Element("File")?.Element("SHA1")?.Value;
                }
                else if (mod.Element("Name")?.Value == "Modding API")
                {
                    apiLink = mod.Element("Link")?.Value;
                    apiSHA1 = mod.Element("Files")?.Element("File")?.Element("SHA1")?.Value;
                }
                else
                {
                    modsList.Add(new Mod
                    {
                        Name = mod.Element("Name")?.Value,
                        Link = mod.Element("Link")?.Value,
                        Files = mod.Element("Files")?.Elements("File").ToDictionary(element => element.Element("Name")?.Value, element => element.Element("SHA1")?.Value),
                        Dependencies = mod.Element("Dependencies")?.Elements("string").Select(dependency => dependency.Value).ToList(),
                        Optional = mod.Element("Optional")?.Elements("string").Select(dependency => dependency.Value).ToList() ?? new List<string>(),
                    });
                }
            }
        }

        private void Form4_Closed(object sender, EventArgs e)
        {
            if (isOffline) return;
            FillModsList();
        }

        private bool SHA1Equals(string file, string modmd5) => String.Equals(GetSHA1(file), modmd5, StringComparison.InvariantCultureIgnoreCase);

        private string GetSHA1(string file)
        {
            using (var sha1 = SHA1.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    var hash = sha1.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private void CheckApiInstalled()
        {
            apiIsInstalled = SHA1Equals(Properties.Settings.Default.APIFolder + @"/Assembly-CSharp.dll", apiSHA1);
            modcommonIsInstalled = File.Exists(Properties.Settings.Default.modFolder + @"/ModCommon.dll") &&
                                   SHA1Equals(Properties.Settings.Default.modFolder + @"/ModCommon.dll", modcommonSHA1);
            
            if (!apiIsInstalled)
            {
                Download(new Uri(apiLink),
                    $@"{Properties.Settings.Default.installFolder}/Modding API.zip", "Modding API");
                InstallApi($@"{Properties.Settings.Default.installFolder}/Modding API.zip",
                    Properties.Settings.Default.temp);
                File.Delete($@"{Properties.Settings.Default.installFolder}/Modding API.zip");
                MessageBox.Show(@"Modding API successfully installed!");
            }

            if (!modcommonIsInstalled)
            {
                Download(new Uri(modcommonLink),
                    $@"{Properties.Settings.Default.modFolder}/ModCommon.zip", "ModCommon");
                InstallApi($@"{Properties.Settings.Default.modFolder}/ModCommon.zip",
                    Properties.Settings.Default.temp);
                File.Delete($@"{Properties.Settings.Default.modFolder}/ModCommon.zip");
                MessageBox.Show(@"ModCommon successfully installed!");       
            }
            
            
        }

        private void PopulateList()
        {
            List<Mod> modsSortedList = modsList.OrderBy(mod => mod.Name).ToList();
            modsList = modsSortedList;

            GetInstalledFiles();

            foreach (Mod mod in modsList)
            {
                if (allMods.Any(f => f.Equals(mod.Name))) continue;

                ModField entry = new ModField
                {
                    Name = new Label(),
                    EnableButton = new Button(),
                    InstallButton = new Button(),
                    ReadmeButton = new Button(),
                    isInstalled = false,
                    isEnabled = false
                };
                panel.Controls.Add(entry.Name);
                panel.Controls.Add(entry.EnableButton);
                panel.Controls.Add(entry.InstallButton);
                panel.Controls.Add(entry.ReadmeButton);
                entry.Name.Text = mod.Name;
                modEntries.Add(entry);
                allMods.Add(mod.Name);
            }

            List<ModField> modFieldsSorted = modEntries.OrderBy(entry => entry.Name.Text).ToList();
            modEntries = modFieldsSorted;

            int space = 50;

            for (int i = 0; i < modEntries.Count; i++)
            {
                modEntries[i].Name.Location = new Point(6, 22 + modEntries[i].EnableButton.Height * i);
                modEntries[i].Name.AutoSize = true;

                modEntries[i].EnableButton.Location = new Point(6 + 150 + space, 19 + modEntries[i].EnableButton.Height * i);
                modEntries[i].EnableButton.Text = modEntries[i].isEnabled ? "Disable" : "Enable";
                modEntries[i].EnableButton.Enabled = modEntries[i].isInstalled;
                modEntries[i].EnableButton.Click += OnEnableButtonClick;

                modEntries[i].InstallButton.Location = new Point(6 + 225 + space, 19 + modEntries[i].EnableButton.Height * i);
                modEntries[i].InstallButton.Text = modEntries[i].isInstalled ? "Uninstall" : "Install";
                modEntries[i].InstallButton.Click += OnInstallButtonClick;

                modEntries[i].ReadmeButton.Location =  new Point(6 + 300 + space, 19 + modEntries[i].EnableButton.Height * i);
                modEntries[i].ReadmeButton.Text = "Readme";
                modEntries[i].ReadmeButton.Enabled = modEntries[i].isInstalled;
                modEntries[i].ReadmeButton.Click += OnReadmeButtonClick;

            }

            button1.Enabled = !isOffline;
        }

        private void OnReadmeButtonClick(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            ModField entry = modEntries.First(f => f.ReadmeButton == button);
            Mod mod = modsList.First(m => m.Name == entry.Name.Text);
            string modName = mod.Name;

            // The only two possible options are .txt or .md, which follows from the InstallMods method
            // The same method also describes, the way all readme files are formatted.
            string readmeModPathNoExtension = $@"{Properties.Settings.Default.installFolder}/README({modName})";
            string readmeModPathTxt = $@"{readmeModPathNoExtension}.txt";
            string readmeModPathMd = $@"{readmeModPathNoExtension}.md";

            // If a readme is created, open it using the default application.
            if (File.Exists(readmeModPathTxt))
            {
                Process.Start(readmeModPathTxt);
            }
            else if (File.Exists(readmeModPathMd))
            {
                Process.Start(readmeModPathMd);
            }
            else
            {
                MessageBox.Show($@"No readme exists for {modName}.");
            }

        }

        private void OnInstallButtonClick(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            ModField entry = modEntries.First(f => f.InstallButton == button);
            Mod mod = modsList.First(m => m.Name == entry.Name.Text);
            string modname = mod.Name;

            string readmeModPathNoExtension = $@"{Properties.Settings.Default.installFolder}/README({modname})";
            string readmeModPathTxt = $@"{readmeModPathNoExtension}.txt";
            string readmeModPathMd = $@"{readmeModPathNoExtension}.md";

            if (entry.isInstalled)
            {
                DialogResult result = MessageBox.Show(text: $@"Do you want to remove {modname} from your computer?", caption: "Confirm removal", buttons: MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    foreach (string s in mod.Files.Keys)
                    {
                        if (File.Exists($@"{Properties.Settings.Default.modFolder}/{s}"))
                        {
                            File.Delete($@"{Properties.Settings.Default.modFolder}/{s}");
                        }
                    }

                    foreach (string directory in Directory.EnumerateDirectories(Properties.Settings.Default.modFolder))
                    {
                        if (!Directory.EnumerateFileSystemEntries(directory).Any() && directory != "Disabled")
                            Directory.Delete(directory);
                    }

                    if (File.Exists(readmeModPathTxt))
                    {
                        File.Delete(readmeModPathTxt);
                    }
                    else if (File.Exists(readmeModPathMd))
                    {
                        File.Delete(readmeModPathMd);
                    }

                    MessageBox.Show($@"{modname} successfully uninstalled!");
                    installedMods.Remove(modname);
                }
                else return;
            }
            else
            {
                if (installedMods.Contains(modname)) return;

                DialogResult result = MessageBox.Show($@"Do you want to install {modname}?", "Confirm installation", MessageBoxButtons.YesNo);

                if (result != DialogResult.Yes) return;
                if (mod.Dependencies.Any())
                {
                    CheckApiInstalled();
                    foreach (string dependency in mod.Dependencies)
                    {
                        if (dependency == "Modding API" || dependency == "ModCommon") continue;
                        if (installedMods.Any(f => f.Equals(dependency))) continue;
                            Install(dependency, false);
                    }
                }

                if (mod.Optional.Any())
                {
                    foreach (string dependency in mod.Optional)
                    {
                        if (installedMods.Any(f => f.Equals(dependency))) continue;
                        DialogResult depInstall =
                            MessageBox.Show(
                                $"The mod author suggests installing {dependency} together with this mod.\nDo you want to install {dependency}?",
                                "Confirm installation", MessageBoxButtons.YesNo);
                        if (depInstall != DialogResult.Yes) continue;
                        Install(dependency, false);
                        MessageBox.Show($@"{dependency} successfully installed!");
                    }
                }
                Install(modname, false);
            }

            modEntries.First(f => f.InstallButton == button).isInstalled = !entry.isInstalled;
            modEntries.First(f => f.InstallButton == button).InstallButton.Text = entry.isInstalled ? "Uninstall" : "Install";
            modEntries.First(f => f.InstallButton == button).isEnabled =
                modEntries.First(f => f.InstallButton == button).isInstalled;
            modEntries.First(f => f.InstallButton == button).EnableButton.Enabled =
                modEntries.First(f => f.InstallButton == button).isInstalled;
            modEntries.First(f => f.InstallButton == button).EnableButton.Text = modEntries.First(f => f.InstallButton == button).isInstalled ? "Disable" : "Enable";
            modEntries.First(f => f.InstallButton == button).ReadmeButton.Enabled =
                modEntries.First(f => f.InstallButton == button).isInstalled;
        }

        private void OnEnableButtonClick(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            ModField entry = modEntries.First(f => f.EnableButton == button);
            Mod mod = modsList.First(m => m.Name == entry.Name.Text);
            string modname = mod.Name;

            if (entry.isEnabled)
            {
                if (modsList.Any(m => m.Name == modname))
                {
                    foreach (string s in modsList.First(m => m.Name == modname).Files.Keys.Where(f => Path.GetExtension(f) == ".dll"))
                    {
                        if (!File.Exists($@"{Properties.Settings.Default.modFolder}/{s}")) continue;
                        if (File.Exists($@"{Properties.Settings.Default.modFolder}/Disabled/{s}"))
                        {
                            File.Delete($@"{Properties.Settings.Default.modFolder}/Disabled/{s}");
                        }

                        File.Move($@"{Properties.Settings.Default.modFolder}/{s}",
                            $@"{Properties.Settings.Default.modFolder}/Disabled/{s}");
                    }
                }
                else
                {
                    if (!File.Exists($@"{Properties.Settings.Default.modFolder}/{modname}")) return;
                    if (File.Exists($@"{Properties.Settings.Default.modFolder}/Disabled/{modname}"))
                    {
                        File.Delete($@"{Properties.Settings.Default.modFolder}/Disabled/{modname}");
                    }

                    File.Move($@"{Properties.Settings.Default.modFolder}/{modname}",
                        $@"{Properties.Settings.Default.modFolder}/Disabled/{modname}");
                }
            }
            else
            {
                if (modsList.Any(m => m.Name == modname))
                {
                    foreach (string s in modsList.First(m => m.Name == modname).Files.Keys.Where(f => Path.GetExtension(f) == ".dll"))
                    {
                        if (!File.Exists($@"{Properties.Settings.Default.modFolder}/Disabled/{s}")) continue;
                        if (File.Exists($@"{Properties.Settings.Default.modFolder}/{s}"))
                        {
                            File.Delete($@"{Properties.Settings.Default.modFolder}/{s}");
                        }

                        File.Move($@"{Properties.Settings.Default.modFolder}/Disabled/{s}",
                            $@"{Properties.Settings.Default.modFolder}/{s}");
                    }
                }
                else
                {
                    if (!File.Exists($@"{Properties.Settings.Default.modFolder}/Disabled/{modname}")) return;
                    if (File.Exists($@"{Properties.Settings.Default.modFolder}/{modname}"))
                    {
                        File.Delete($@"{Properties.Settings.Default.modFolder}/{modname}");
                    }

                    File.Move($@"{Properties.Settings.Default.modFolder}/Disabled/{modname}",
                        $@"{Properties.Settings.Default.modFolder}/{modname}");
                }
            }
            modEntries.First(f => f.EnableButton == button).isEnabled = !modEntries.First(f => f.EnableButton == button).isEnabled;
            modEntries.First(f => f.EnableButton == button).EnableButton.Text = entry.isEnabled ? "Disable" : "Enable";
        }

        private void GetInstalledFiles()
        {
            DirectoryInfo modsFolder = new DirectoryInfo(Properties.Settings.Default.modFolder);
            FileInfo[] modsFiles = modsFolder.GetFiles("*.dll");

            if (!Directory.Exists(Properties.Settings.Default.modFolder + @"/Disabled"))
                Directory.CreateDirectory(Properties.Settings.Default.modFolder + @"/Disabled");

            DirectoryInfo disabledFolder = new DirectoryInfo(Properties.Settings.Default.modFolder + @"/Disabled");
            FileInfo[] disabledFiles = disabledFolder.GetFiles("*.dll");

            foreach (var modsFile in modsFiles)
            {
                if (Path.GetFileName(modsFile.Name) == "ModCommon.dll") continue;
                Mod mod = new Mod();
                ModField entry = new ModField
                {
                    Name = new Label(),
                    EnableButton = new Button(),
                    InstallButton = new Button(),
                    ReadmeButton = new Button(),
                    isEnabled =  true,
                    isInstalled = true
                };
                panel.Controls.Add(entry.Name);
                panel.Controls.Add(entry.EnableButton);
                panel.Controls.Add(entry.InstallButton);
                panel.Controls.Add(entry.ReadmeButton);
                bool isGDriveMod = modsList.Any(m => m.Files.Keys.Contains(Path.GetFileName(modsFile.Name)));

                if (isGDriveMod)
                {
                    mod = modsList.First(m => m.Files.Keys.Contains(Path.GetFileName(modsFile.Name)));
                    
                    CheckModUpdated(modsFile.FullName, mod);
                }
                else
                {
                    mod = new Mod
                    {
                        Name = Path.GetFileNameWithoutExtension(modsFile.Name),
                        Files = new Dictionary<string, string> { [Path.GetFileName(modsFile.Name)] = GetSHA1(modsFile.FullName) },
                        Link = "",
                        Dependencies = new List<string>(),
                        Optional = new List<string>()
                    };
                }

                if (string.IsNullOrEmpty(mod.Name) || allMods.Any(f => f == mod.Name)) continue;
                entry.Name.Text = mod.Name;
                modEntries.Add(entry);
                modsList.Add(mod);
                allMods.Add(mod.Name);
                installedMods.Add(mod.Name);
            }

            foreach (var file in disabledFiles)
            {
                if (Path.GetFileName(file.Name) == "ModCommon.dll") continue;
                Mod mod = new Mod();
                ModField entry = new ModField
                {
                    Name = new Label(),
                    EnableButton = new Button(),
                    InstallButton = new Button(),
                    ReadmeButton = new Button(),
                    isEnabled = false,
                    isInstalled = true
                };
                panel.Controls.Add(entry.Name);
                panel.Controls.Add(entry.EnableButton);
                panel.Controls.Add(entry.InstallButton);
                panel.Controls.Add(entry.ReadmeButton);
                bool isGDriveMod = modsList.Any(m => m.Files.Keys.Contains(Path.GetFileName(file.Name)));

                if (isGDriveMod)
                {
                    mod = modsList.First(m => m.Files.Keys.Contains(Path.GetFileName(file.Name)));
                    CheckModUpdated(file.FullName, mod);
                }
                else
                {
                    mod = new Mod
                    {
                        Name = Path.GetFileNameWithoutExtension(file.Name),
                        Files = new Dictionary<string, string> { [Path.GetFileName(file.Name)] = GetSHA1(file.FullName) },
                        Link = "",
                        Dependencies = new List<string>(),
                        Optional = new List<string>()
                    };
                }

                if (string.IsNullOrEmpty(mod.Name) || allMods.Any(f => f == mod.Name)) continue;
                entry.Name.Text = mod.Name;
                modsList.Add(mod);
                modEntries.Add(entry);
                allMods.Add(mod.Name);
                installedMods.Add(mod.Name);
            }
        }

        private void CheckModUpdated(string filename, Mod mod)
        {
            if (SHA1Equals(filename,
                mod.Files[mod.Files.Keys.First(f => f == Path.GetFileName(filename))])) return;
            DialogResult update = MessageBox.Show($"{mod.Name} is outdated. Would you like to update it?", "Outdated mod",
                MessageBoxButtons.YesNo);
            if (update != DialogResult.Yes) return;
            Install(mod.Name, true);
        }

        private void ResizeUI()
        {
            const int height = 480;
            panel.Size = new Size(modEntries[0].ReadmeButton.Right - modEntries[0].Name.Left + 50, height);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button1.Size = new Size(panel.Width, 23);
            button2.Size = new Size(panel.Width, 23);
            button3.Size = new Size(panel.Width, 23);
            button1.Top = height + 9;
            button1.Left = 3;
            button2.Top = button1.Bottom;
            button2.Left = 3;
            button3.Top = button2.Bottom;
            button3.Left = 3;
            PerformAutoScale();
        }

        private static void DeleteDirectory(string target_dir)
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

        #endregion

        #region Downloading and installing

        private void Download(Uri uri,string path, string name)
        {
            DownloadHelper download = new DownloadHelper(uri, path, name);
            download.ShowDialog();
        }

        private void Install(string mod, bool isUpdate)
        {
            Download(new Uri(modsList.First(m => m.Name == mod).Link),
                $@"{Properties.Settings.Default.modFolder}/{mod}.zip", mod);

            InstallMods($@"{Properties.Settings.Default.modFolder}/{mod}.zip",
                Properties.Settings.Default.temp);

            File.Delete($@"{Properties.Settings.Default.modFolder}/{mod}.zip");

            MessageBox.Show(isUpdate ? $@"{mod} successfully updated!" : $@"{mod} successfully installed!");
        }

        #region Unpacking and moving/copying/deleting files

        private void InstallApi(string api, string tempFolder)
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
            apiIsInstalled = true;
            Properties.Settings.Default.Save();
        }

        private void InstallMods(string mod, string tempFolder)
        {
            if (Directory.Exists(Properties.Settings.Default.temp))
                Directory.Delete(tempFolder, true);
            if (!Directory.Exists(Properties.Settings.Default.modFolder)) Directory.CreateDirectory(Properties.Settings.Default.modFolder);
            
            ZipFile.ExtractToDirectory(mod, tempFolder);
            List<string> res = Directory.EnumerateFiles(tempFolder, "*", SearchOption.AllDirectories).ToList();

            foreach (string Res in res)
            {
                switch (Path.GetExtension(Res))
                {
                    case ".dll":
                        File.Copy(Res,
                            $@"{Properties.Settings.Default.modFolder}/{Path.GetFileName(Res)}", true);
                        break;
                    case ".txt":
                    case ".md":
                        File.Copy(Res,
                            $@"{Properties.Settings.Default.installFolder}/{Path.GetFileNameWithoutExtension(Res)}({Path.GetFileNameWithoutExtension(mod)}){Path.GetExtension(Res)}",
                            true);
                        break;
                    case ".ini":
                        break;
                    default:
                        string path = Path.GetDirectoryName(Res)?.Replace(Properties.Settings.Default.temp,
                            Properties.Settings.Default.installFolder);
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        File.Copy(Res, $@"{path}/{Path.GetFileName(Res)}", true);
                        break;
                }
            }
            Directory.Delete(tempFolder, true);
            
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
            CheckApiInstalled();
            if (!apiIsInstalled)
            {
                DialogResult result = MessageBox.Show("Do you want to install the modding API?", "Install confirmation",
                    MessageBoxButtons.YesNo);
                if (result != DialogResult.Yes) return;
                Download(new Uri(apiLink), $@"{Properties.Settings.Default.installFolder}/API.zip", "Modding API");
                InstallApi($@"{Properties.Settings.Default.installFolder}/API.zip", Properties.Settings.Default.temp);
                File.Delete($@"{Properties.Settings.Default.installFolder}/API.zip");
                MessageBox.Show("Modding API successfully installed!");
            }
            else
            {
                MessageBox.Show("Modding API is already installed!");
            }
        }

        private void ManualInstallClick(object sender, EventArgs e)
        {
            manualInstallList = new List<string>();
            openFileDialog.Reset();
            openFileDialog.Filter = "Mod files|*.zip; *.dll|All files|*.*";
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Select the mods you wish to install";
            openFileDialog.ShowDialog();
        }

        void ChangePathClick(object sender, EventArgs e)
        {
            folderBrowserDialog1.Reset();
            folderBrowserDialog1.ShowDialog();
            if (String.IsNullOrEmpty(folderBrowserDialog1.SelectedPath)) return;
            if (File.Exists(folderBrowserDialog1.SelectedPath + @"/hollow_knight_Data/Managed/Assembly-CSharp.dll") && Path.GetFileName(folderBrowserDialog1.SelectedPath) == "Hollow Knight")
            {
                Properties.Settings.Default.installFolder = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.APIFolder =
                    $@"{Properties.Settings.Default.installFolder}/hollow_knight_Data/Managed";
                Properties.Settings.Default.modFolder = $@"{Properties.Settings.Default.APIFolder}/Mods";
                Properties.Settings.Default.Save();
                if (!Directory.Exists(Properties.Settings.Default.modFolder))
                    Directory.CreateDirectory(Properties.Settings.Default.modFolder);
                MessageBox.Show(text: $"Hollow Knight installation path:\n{Properties.Settings.Default.installFolder}");
            }
            else
            {
                MessageBox.Show(@"Invalid path selected.
Please select the correct installation path for Hollow Knight.");
                ChangePathClick(new object(), EventArgs.Empty);
            }
        }

        private void DoManualInstall(object sender, System.EventArgs e)
        {
            if (openFileDialog.FileNames.Length >= 1)
            {
                foreach (string mod in openFileDialog.FileNames)
                {
                    if (Path.GetExtension(mod) == ".zip")
                    {
                        InstallMods(mod,
                            Properties.Settings.Default.temp);
                    }
                    else
                    {
                        File.Copy(mod, $"{Properties.Settings.Default.modFolder}/{Path.GetFileName(mod)}", true);
                    }
                    MessageBox.Show($@"{Path.GetFileName(mod)} successfully installed!");
                }
            }
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
        private List<string> manualInstallList = new List<string>();

        private struct Mod
        {
            public string Name { get; set; }

            public Dictionary<string, string> Files { get; set; }

            public string Link { get; set; }

            public List<string> Dependencies { get; set; }

            public List<string> Optional { get; set; }
        }

        private class ModField
        {
            public Label Name { get; set; }

            public Button EnableButton { get; set; }

            public Button InstallButton { get; set; }

            public Button ReadmeButton { get; set; }

            public bool isInstalled { get; set; }

            public bool isEnabled { get; set; }
        }

        private  List<Mod> modsList = new List<Mod>();

        private List<ModField> modEntries = new List<ModField>();

        private string apiLink;
        private string apiSHA1;
        private string modcommonLink;
        private string modcommonSHA1;
        private string OS;
        public bool isOffline;
        private bool apiIsInstalled;
        private bool modcommonIsInstalled;
        private bool pirate;
        public string version = "v8.2.0";


        #endregion
    }
}