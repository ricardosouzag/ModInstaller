using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Xml.Linq;

// ReSharper disable LocalizableElement

namespace ModInstaller
{
    public partial class ModManager : Form
    {
        private const string ModLinks = "https://raw.githubusercontent.com/Ayugradow/ModInstaller/master/modlinks.xml";

        private const string Version = "v8.5.2";

        private readonly List<string> _defaultPaths = new List<string>();

        private readonly List<string> _allMods = new List<string>();

        private readonly List<string> _installedMods = new List<string>();
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

            public bool IsInstalled { get; set; }

            public bool IsEnabled { get; set; }
        }

        private List<Mod> _modsList = new List<Mod>();

        private List<ModField> _modEntries = new List<ModField>();

        private string OS
        {
            get => _os;
            set
            {
                if (value == "Windows")
                {
                    _os = value;
                    return;
                }

                using (Process proc = Process.Start
                (
                    new ProcessStartInfo
                    {
                        FileName = "/bin/sh",
                        Arguments = "-c uname",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        UserName = Environment.UserName
                    }
                ))
                {
                    if (proc == null)
                    {
                        _os = "Linux";
                        return;
                    }

                    proc.WaitForExit();

                    using (StreamReader standardOutput = proc.StandardOutput)
                    {
                        _os = standardOutput.ReadLine() == "Darwin" ? "MacOS" : "Linux";
                    }
                }
            }
        }

        private string _apiLink;

        private string _apiSha1;

        private string _currentPatch;

        private string _modcommonLink;

        private string _modcommonSha1;

        private string _os;

        public bool IsOffline;

        private bool _apiIsInstalled;

        private bool _modcommonIsInstalled;

        private bool _vanillaEnabled;

        private bool _assemblyIsAPI;

        public ModManager()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            #if !DEBUG 
            CheckUpdate();
            #endif
            GetCurrentOS();
            FillDefaultPaths();
            GetLocalInstallation();
            PiracyCheck();
            FillModsList();
            CheckApiInstalled();
            PopulateList();
            ResizeUI();
            Text = "Mod Manager " + Version + " by Gradow";
        }
        
        #region Loading and building the mod manager

        private void CheckUpdate()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            string file = Path.GetFileName(Assembly.GetEntryAssembly()?.Location);
            var dllist = new XDocument();

            if (File.Exists($"{dir}/lol.exe"))
                File.Delete($"{dir}/lol.exe");

            if (File.Exists($"{dir}/AU.exe"))
                File.Delete($"{dir}/AU.exe");

            try
            {
                dllist = XDocument.Load(ModLinks);
            }
            catch (Exception)
            {
                var form = new ConnectionFailedForm(this);
                form.Closed += Form4_Closed;
                Hide();
                form.ShowDialog();
            }

            XElement installer = dllist.Element("ModLinks")?.Element("Installer");

            if (installer == null || installer.Element("SHA1")?.Value == GetSHA1($"{dir}/{file}")) return;

            var dl = new WebClient();
            dl.DownloadFile
            (
                new Uri
                (
                    installer.Element("AULink")?.Value ?? throw new NoNullAllowedException("AULink Missing!")
                ),
                $"{dir}/AU.exe"
            );
            var process = new Process
            {
                StartInfo =
                {
                    FileName = $"{dir}/AU.exe",
                    Arguments = $"\"{dir}\" {installer.Element("Link")?.Value} {file}"
                }
            };
            process.Start();
            Application.Exit();
        }

        private void GetCurrentOS()
        {
            int p = (int) Environment.OSVersion.Platform;
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
                    _defaultPaths.Add("Program Files (x86)/Steam/steamapps/Common/Hollow Knight");
                    _defaultPaths.Add("Program Files/Steam/steamapps/Common/Hollow Knight");
                    _defaultPaths.Add("Steam/steamapps/common/Hollow Knight");
                    _defaultPaths.Add("Program Files (x86)/GOG Galaxy/Games/Hollow Knight");
                    _defaultPaths.Add("Program Files/GOG Galaxy/Games/Hollow Knight");
                    _defaultPaths.Add("GOG Galaxy/Games/Hollow Knight");
                    break;
                case "Linux":
                    // Default steam installation path for Linux.
                    _defaultPaths.Add(Environment.GetEnvironmentVariable("HOME") + "/.steam/steam/steamapps/common/Hollow Knight");
                    break;
                case "MacOS":
                    //Default steam installation path for Mac.
                    _defaultPaths.Add
                        (Environment.GetEnvironmentVariable("HOME") + "/Library/Application Support/Steam/steamapps/common/Hollow Knight/hollow_knight.app");
                    break;
            }
        }

        private void GetLocalInstallation()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.installFolder))
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();

                foreach (DriveInfo d in allDrives.Where(d => d.DriveType == DriveType.Fixed))
                {
                    foreach (string path in _defaultPaths)
                    {
                        if (!Directory.Exists($"{d.Name}{path}")) continue;
                        SetDefaultPath($"{d.Name}{path}");

                        // If user is on sane operating system with a /tmp folder, put temp files here.
                        // Reasoning:
                        // 1) /tmp usually has normal user write permissions. C:\temp might not.
                        // 2) /tmp is usually on a ramdisk. Less disk writing is always better.
                        if (Directory.Exists($"{d.Name}tmp"))
                        {
                            if (Directory.Exists($"{d.Name}tmp/HKmodinstaller"))
                            {
                                DeleteDirectory($"{d.Name}tmp/HKmodinstaller");
                            }

                            Directory.CreateDirectory($"{d.Name}tmp/HKmodinstaller");
                            Properties.Settings.Default.temp = $"{d.Name}tmp/HKmodinstaller";
                        }
                        else
                        {
                            Properties.Settings.Default.temp = Directory.Exists($"{d.Name}temp")
                                ? $"{d.Name}tempMods"
                                : $"{d.Name}temp";
                        }

                        if (!string.IsNullOrEmpty(Properties.Settings.Default.installFolder))
                            break;
                        Properties.Settings.Default.Save();
                    }

                    if (!string.IsNullOrEmpty(Properties.Settings.Default.installFolder))
                        break;
                }

                if (string.IsNullOrEmpty(Properties.Settings.Default.installFolder))
                {
                    var form = new ManualPathLocation(OS);
                    Hide();
                    form.FormClosed += ManualPathClosed;
                    form.ShowDialog();
                }
                else
                {
                    Properties.Settings.Default.APIFolder = OSPath(OS);
                    Properties.Settings.Default.modFolder = $"{Properties.Settings.Default.APIFolder}/Mods";
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
            DialogResult dialogResult = MessageBox.Show
            (
                "Is this your Hollow Knight installation path?\n" + path,
                "Path confirmation",
                MessageBoxButtons.YesNo
            );

            if (dialogResult != DialogResult.Yes) return;

            Properties.Settings.Default.installFolder = path;
            Properties.Settings.Default.Save();
        }

        private void PiracyCheck()
        {
            if
            (
                OS != "Windows"
                || File.Exists($"{Properties.Settings.Default.installFolder}/Galaxy.dll")
                || Path.GetFileName(Properties.Settings.Default.installFolder) != "Hollow Knight Godmaster"
            )
                return;

            MessageBox.Show("Please purchase the game before attempting to play it.");
            Process.Start("https://store.steampowered.com/app/367520/Hollow_Knight/");
        }

        private void FillModsList()
        {
            XDocument dllist;

            try
            {
                dllist = XDocument.Load(ModLinks);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                var form4 = new ConnectionFailedForm(this);
                form4.Closed += Form4_Closed;
                Hide();
                form4.ShowDialog();
                return;
            }

            XElement[] mods = dllist.Element("ModLinks")?.Element("ModList")?.Elements("ModLink").ToArray();

            if (mods == null) return;

            foreach (XElement mod in mods)
            {
                switch (mod.Element("Name")?.Value)
                {
                    case "ModCommon":
                        _modcommonLink = mod.Element("Link")?.Value;
                        _modcommonSha1 = mod.Element("Files")?.Element("File")?.Element("SHA1")?.Value;
                        break;
                    case "Modding API Windows":
                        if (OS == "Windows")
                        {
                            _apiLink = mod.Element("Link")?.Value;
                            _apiSha1 = mod.Element("Files")?.Element("File")?.Element("SHA1")?.Value;
                            _currentPatch = mod.Element("Files")?.Element("File")?.Element("Patch")?.Value;
                        }
                        break;
                    case "Modding API Linux":
                        if (OS == "Linux" || OS == "MacOS")
                        {
                            _apiLink = mod.Element("Link")?.Value;
                            _apiSha1 = mod.Element("Files")?.Element("File")?.Element("SHA1")?.Value;
                            _currentPatch = mod.Element("Files")?.Element("File")?.Element("Patch")?.Value;
                        }
                        break;
                    default:
                        _modsList.Add
                        (
                            new Mod
                            {
                                Name = mod.Element("Name")?.Value,
                                Link = mod.Element("Link")?.Value,
                                Files = mod.Element("Files")
                                           ?.Elements("File")
                                           .ToDictionary
                                           (
                                               element => element.Element("Name")?.Value,
                                               element => element.Element("SHA1")?.Value
                                           ),
                                Dependencies = mod.Element("Dependencies")
                                                  ?.Elements("string")
                                                  .Select(dependency => dependency.Value)
                                                  .ToList(),
                                Optional = mod.Element("Optional")
                                              ?.Elements("string")
                                              .Select(dependency => dependency.Value)
                                              .ToList()
                                    ?? new List<string>()
                            }
                        );
                        break;
                }
            }
        }

        private void Form4_Closed(object sender, EventArgs e)
        {
            if (IsOffline) return;
            FillModsList();
        }

        private static bool SHA1Equals(string file, string modmd5) => string.Equals(GetSHA1(file), modmd5, StringComparison.InvariantCultureIgnoreCase);

        private static string GetSHA1(string file)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                using (FileStream stream = File.OpenRead(file))
                {
                    byte[] hash = sha1.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private void CheckApiInstalled()
        {
            if (!Directory.Exists(Properties.Settings.Default.APIFolder))
            {
                MessageBox.Show("Folder does not exist! (Game is probably not installed) Exiting.");

                // Make sure to not ruin everything forever
                Properties.Settings.Default.installFolder = null;

                Application.Exit();
                Close();

                return;
            }


            // Check if either API is installed or if vanilla dll still exists
            if (File.Exists($"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.dll"))
            {
                byte[] bytes = File.ReadAllBytes($"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.dll");
                Assembly asm = Assembly.Load(bytes);

                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }

                Type[] nonNullTypes = types.Where(t => t != null).ToArray();

                _assemblyIsAPI = nonNullTypes.Any(type => type.Name.Contains("CanvasUtil"));

                _apiIsInstalled = _assemblyIsAPI || File.Exists($"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.mod");


                if (!File.Exists($"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.vanilla")
                    && !_apiIsInstalled
                    && (!nonNullTypes.Any(type => type.Name.Contains("Constant"))
                        || (string) nonNullTypes
                                    .First(type => type.Name.Contains("Constant") && type.GetFields().Any(f => f.Name == "GAME_VERSION"))
                                    .GetField("GAME_VERSION")
                                    .GetValue(null)
                        != _currentPatch))
                {
                    MessageBox.Show
                    (
                        "This installer requires the most recent stable version to run.\nPlease update your game to current stable patch and then try again.",
                        "Warning!"
                    );

                    // Make sure to not ruin everything forever part2

                    Application.Exit();
                    Close();

                    return;
                }
            }
            else
            {
                MessageBox.Show
                (
                    "Unable to locate game files.\nPlease make sure the game is installed and then try again.",
                    "Warning!"
                );

                // Make sure to not ruin everything forever part3

                Application.Exit();
                Close();

                return;
            }

            _modcommonIsInstalled = File.Exists($"{Properties.Settings.Default.modFolder}/ModCommon.dll")
                && SHA1Equals
                (
                    $"{Properties.Settings.Default.modFolder}/ModCommon.dll",
                    _modcommonSha1
                );

            if (!_apiIsInstalled || _assemblyIsAPI && !SHA1Equals($"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.dll", _apiSha1))
            {
                Download
                (
                    new Uri(_apiLink),
                    $"{Properties.Settings.Default.installFolder}/Modding API.zip",
                    "Modding API"
                );
                InstallApi
                (
                    $"{Properties.Settings.Default.installFolder}/Modding API.zip",
                    Properties.Settings.Default.temp
                );
                File.Delete($"{Properties.Settings.Default.installFolder}/Modding API.zip");
                MessageBox.Show("Modding API successfully installed!");
            }

            if (_modcommonIsInstalled) return;

            Download
            (
                new Uri(_modcommonLink),
                $"{Properties.Settings.Default.modFolder}/ModCommon.zip",
                "ModCommon"
            );
            InstallMods
            (
                $"{Properties.Settings.Default.modFolder}/ModCommon.zip",
                Properties.Settings.Default.temp,
                true
            );
            File.Delete($"{Properties.Settings.Default.modFolder}/ModCommon.zip");
            MessageBox.Show("ModCommon successfully installed!");
        }

        private void PopulateList()
        {
            List<Mod> modsSortedList = _modsList.OrderBy(mod => mod.Name).ToList();
            _modsList = modsSortedList;

            GetInstalledFiles();

            foreach (Mod mod in _modsList)
            {
                if (_allMods.Any(f => f.Equals(mod.Name))) continue;

                var entry = new ModField
                {
                    Name = new Label(),
                    EnableButton = new Button(),
                    InstallButton = new Button(),
                    ReadmeButton = new Button(),
                    IsInstalled = false,
                    IsEnabled = false
                };
                panel.Controls.Add(entry.Name);
                panel.Controls.Add(entry.EnableButton);
                panel.Controls.Add(entry.InstallButton);
                panel.Controls.Add(entry.ReadmeButton);
                entry.Name.Text = mod.Name;
                _modEntries.Add(entry);
                _allMods.Add(mod.Name);
            }

            List<ModField> modFieldsSorted = _modEntries.OrderBy(entry => entry.Name.Text).ToList();
            _modEntries = modFieldsSorted;

            const int space = 50;

            for (int i = 0; i < _modEntries.Count; i++)
            {
                ModField entry = _modEntries[i];

                entry.Name.Location = new Point(6, 22 + entry.EnableButton.Height * i);
                entry.Name.AutoSize = true;

                entry.EnableButton.Location = new Point(6 + 150 + space, 19 + entry.EnableButton.Height * i);
                entry.EnableButton.Text = entry.IsEnabled ? "Disable" : "Enable";
                entry.EnableButton.Enabled = entry.IsInstalled;
                entry.EnableButton.Click += OnEnableButtonClick;

                entry.InstallButton.Location = new Point(6 + 225 + space, 19 + entry.EnableButton.Height * i);
                entry.InstallButton.Text = entry.IsInstalled ? "Uninstall" : "Install";
                entry.InstallButton.Click += OnInstallButtonClick;

                entry.ReadmeButton.Location = new Point(6 + 300 + space, 19 + entry.EnableButton.Height * i);
                entry.ReadmeButton.Text = "Readme";
                entry.ReadmeButton.Enabled = entry.IsInstalled;
                entry.ReadmeButton.Click += OnReadmeButtonClick;
            }

            _vanillaEnabled = !SHA1Equals(Properties.Settings.Default.APIFolder + "/Assembly-CSharp.dll", _apiSha1);

            button1.Text = _vanillaEnabled
                ? "Enable All Installed Mods"
                : "Revert Back To Unmodded";
        }

        private void OnReadmeButtonClick(object sender, EventArgs e)
        {
            var button = (Button) sender;
            ModField entry = _modEntries.First(f => f.ReadmeButton == button);
            Mod mod = _modsList.First(m => m.Name == entry.Name.Text);
            string modName = mod.Name;

            // The only two possible options are .txt or .md, which follows from the InstallMods method
            // The same method also describes, the way all readme files are formatted.
            string readmeModPathNoExtension = $"{Properties.Settings.Default.installFolder}/README({modName})";
            string readmeModPathTxt = $"{readmeModPathNoExtension}.txt";
            string readmeModPathMd = $"{readmeModPathNoExtension}.md";

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
                MessageBox.Show($"No readme exists for {modName}.");
            }
        }

        private void OnInstallButtonClick(object sender, EventArgs e)
        {
            var button = (Button) sender;
            ModField entry = _modEntries.First(f => f.InstallButton == button);
            Mod mod = _modsList.First(m => m.Name == entry.Name.Text);
            string modname = mod.Name;

            string readmeModPathNoExtension = $"{Properties.Settings.Default.installFolder}/README({modname})";
            string readmeModPathTxt = $"{readmeModPathNoExtension}.txt";
            string readmeModPathMd = $"{readmeModPathNoExtension}.md";

            if (entry.IsInstalled)
            {
                DialogResult result = MessageBox.Show
                (
                    $"Do you want to remove {modname} from your computer?",
                    "Confirm removal",
                    MessageBoxButtons.YesNo
                );

                if (result != DialogResult.Yes)
                    return;

                foreach (string s in mod.Files.Keys)
                {
                    if (File.Exists($"{Properties.Settings.Default.modFolder}/{s}"))
                    {
                        File.Delete($"{Properties.Settings.Default.modFolder}/{s}");
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

                MessageBox.Show($"{modname} successfully uninstalled!");
                _installedMods.Remove(modname);
            }
            else
            {
                if (_installedMods.Contains(modname)) return;

                DialogResult result = MessageBox.Show
                (
                    $"Do you want to install {modname}?",
                    "Confirm installation",
                    MessageBoxButtons.YesNo
                );

                if (result != DialogResult.Yes) return;

                if (mod.Dependencies.Any())
                {
                    CheckApiInstalled();
                    foreach (string dependency in mod.Dependencies)
                    {
                        if (dependency == "Modding API" || dependency == "ModCommon") continue;
                        if (_installedMods.Any(f => f.Equals(dependency))) continue;
                        Install(dependency, false, true);
                    }
                }

                if (mod.Optional.Any())
                {
                    foreach (string dependency in mod.Optional)
                    {
                        if (_installedMods.Any(f => f.Equals(dependency))) continue;
                        DialogResult depInstall =
                            MessageBox.Show
                            (
                                $"The mod author suggests installing {dependency} together with this mod.\nDo you want to install {dependency}?",
                                "Confirm installation",
                                MessageBoxButtons.YesNo
                            );
                        if (depInstall != DialogResult.Yes) continue;
                        Install(dependency, false, true);
                        MessageBox.Show($"{dependency} successfully installed!");
                    }
                }

                Install(modname, false, true);
            }

            entry.IsInstalled = !entry.IsInstalled;
            entry.InstallButton.Text = entry.IsInstalled ? "Uninstall" : "Install";
            entry.IsEnabled = entry.IsInstalled;
            entry.EnableButton.Enabled = entry.IsInstalled;
            entry.EnableButton.Text = entry.IsInstalled ? "Disable" : "Enable";
            entry.ReadmeButton.Enabled = entry.IsInstalled;
        }

        private void OnEnableButtonClick(object sender, EventArgs e)
        {
            var button = (Button) sender;
            ModField entry = _modEntries.First(f => f.EnableButton == button);
            Mod mod = _modsList.First(m => m.Name == entry.Name.Text);
            string modname = mod.Name;

            if (entry.IsEnabled)
            {
                if (_modsList.Any(m => m.Name == modname))
                {
                    foreach (string s in _modsList.First(m => m.Name == modname)
                                                  .Files.Keys
                                                  .Where(f => Path.GetExtension(f) == ".dll"))
                    {
                        if (!File.Exists($"{Properties.Settings.Default.modFolder}/{s}")) continue;
                        if (File.Exists($"{Properties.Settings.Default.modFolder}/Disabled/{s}"))
                        {
                            File.Delete($"{Properties.Settings.Default.modFolder}/Disabled/{s}");
                        }

                        File.Move
                        (
                            $"{Properties.Settings.Default.modFolder}/{s}",
                            $"{Properties.Settings.Default.modFolder}/Disabled/{s}"
                        );
                    }
                }
                else
                {
                    if (!File.Exists($"{Properties.Settings.Default.modFolder}/{modname}")) return;
                    if (File.Exists($"{Properties.Settings.Default.modFolder}/Disabled/{modname}"))
                    {
                        File.Delete($"{Properties.Settings.Default.modFolder}/Disabled/{modname}");
                    }

                    File.Move
                    (
                        $"{Properties.Settings.Default.modFolder}/{modname}",
                        $"{Properties.Settings.Default.modFolder}/Disabled/{modname}"
                    );
                }
            }
            else
            {
                if (_modsList.Any(m => m.Name == modname))
                {
                    foreach (string s in _modsList.First(m => m.Name == modname)
                                                  .Files.Keys
                                                  .Where(f => Path.GetExtension(f) == ".dll"))
                    {
                        if (!File.Exists($"{Properties.Settings.Default.modFolder}/Disabled/{s}")) continue;
                        if (File.Exists($"{Properties.Settings.Default.modFolder}/{s}"))
                        {
                            File.Delete($"{Properties.Settings.Default.modFolder}/{s}");
                        }

                        File.Move
                        (
                            $"{Properties.Settings.Default.modFolder}/Disabled/{s}",
                            $"{Properties.Settings.Default.modFolder}/{s}"
                        );
                    }
                }
                else
                {
                    if (!File.Exists($"{Properties.Settings.Default.modFolder}/Disabled/{modname}")) return;
                    if (File.Exists($"{Properties.Settings.Default.modFolder}/{modname}"))
                    {
                        File.Delete($"{Properties.Settings.Default.modFolder}/{modname}");
                    }

                    File.Move
                    (
                        $"{Properties.Settings.Default.modFolder}/Disabled/{modname}",
                        $"{Properties.Settings.Default.modFolder}/{modname}"
                    );
                }
            }

            _modEntries.First(f => f.EnableButton == button).IsEnabled =
                !_modEntries.First(f => f.EnableButton == button).IsEnabled;
            _modEntries.First(f => f.EnableButton == button).EnableButton.Text = entry.IsEnabled ? "Disable" : "Enable";
        }

        private void GetInstalledFiles()
        {
            var modsFolder = new DirectoryInfo(Properties.Settings.Default.modFolder);
            FileInfo[] modsFiles = modsFolder.GetFiles("*.dll");

            if (!Directory.Exists($"{Properties.Settings.Default.modFolder}/Disabled"))
                Directory.CreateDirectory($"{Properties.Settings.Default.modFolder}/Disabled");

            var disabledFolder = new DirectoryInfo($"{Properties.Settings.Default.modFolder}/Disabled");
            FileInfo[] disabledFiles = disabledFolder.GetFiles("*.dll");

            foreach (FileInfo modsFile in modsFiles)
            {
                if (Path.GetFileName(modsFile.Name) == "ModCommon.dll") continue;

                Mod mod;

                var entry = new ModField
                {
                    Name = new Label(),
                    EnableButton = new Button(),
                    InstallButton = new Button(),
                    ReadmeButton = new Button(),
                    IsEnabled = true,
                    IsInstalled = true
                };
                panel.Controls.Add(entry.Name);
                panel.Controls.Add(entry.EnableButton);
                panel.Controls.Add(entry.InstallButton);
                panel.Controls.Add(entry.ReadmeButton);

                bool isGDriveMod = _modsList.Any(m => m.Files.Keys.Contains(Path.GetFileName(modsFile.Name)));

                if (isGDriveMod)
                {
                    mod = _modsList.First(m => m.Files.Keys.Contains(Path.GetFileName(modsFile.Name)));

                    CheckModUpdated(modsFile.FullName, mod, true);
                }
                else
                {
                    mod = new Mod
                    {
                        Name = Path.GetFileNameWithoutExtension(modsFile.Name),
                        Files = new Dictionary<string, string>
                        {
                            [Path.GetFileName(modsFile.Name)] = GetSHA1(modsFile.FullName)
                        },
                        Link = "",
                        Dependencies = new List<string>(),
                        Optional = new List<string>()
                    };
                }

                if (string.IsNullOrEmpty(mod.Name) || _allMods.Any(f => f == mod.Name)) continue;
                entry.Name.Text = mod.Name;
                _modEntries.Add(entry);
                _modsList.Add(mod);
                _allMods.Add(mod.Name);
                _installedMods.Add(mod.Name);
            }

            foreach (FileInfo file in disabledFiles)
            {
                if (Path.GetFileName(file.Name) == "ModCommon.dll") continue;

                Mod mod;

                var entry = new ModField
                {
                    Name = new Label(),
                    EnableButton = new Button(),
                    InstallButton = new Button(),
                    ReadmeButton = new Button(),
                    IsEnabled = false,
                    IsInstalled = true
                };

                panel.Controls.Add(entry.Name);
                panel.Controls.Add(entry.EnableButton);
                panel.Controls.Add(entry.InstallButton);
                panel.Controls.Add(entry.ReadmeButton);
                bool isGDriveMod = _modsList.Any(m => m.Files.Keys.Contains(Path.GetFileName(file.Name)));

                if (isGDriveMod)
                {
                    mod = _modsList.First(m => m.Files.Keys.Contains(Path.GetFileName(file.Name)));
                    CheckModUpdated(file.FullName, mod, false);
                }
                else
                {
                    mod = new Mod
                    {
                        Name = Path.GetFileNameWithoutExtension(file.Name),
                        Files = new Dictionary<string, string> {[Path.GetFileName(file.Name)] = GetSHA1(file.FullName)},
                        Link = "",
                        Dependencies = new List<string>(),
                        Optional = new List<string>()
                    };
                }

                if (string.IsNullOrEmpty(mod.Name) || _allMods.Any(f => f == mod.Name)) continue;
                entry.Name.Text = mod.Name;
                _modsList.Add(mod);
                _modEntries.Add(entry);
                _allMods.Add(mod.Name);
                _installedMods.Add(mod.Name);
            }
        }

        private void CheckModUpdated(string filename, Mod mod, bool isEnabled)
        {
            if (SHA1Equals
            (
                filename,
                mod.Files[mod.Files.Keys.First(f => f == Path.GetFileName(filename))]
            ))
                return;

            DialogResult update = MessageBox.Show
            (
                $"{mod.Name} is outdated. Would you like to update it?",
                "Outdated mod",
                MessageBoxButtons.YesNo
            );

            if (update != DialogResult.Yes) return;

            Install(mod.Name, true, isEnabled);
        }

        private void ResizeUI()
        {
            const int height = 480;
            panel.Size = new Size(_modEntries[0].ReadmeButton.Right - _modEntries[0].Name.Left + 50, height);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button1.Size = new Size(panel.Width, 23);
            button2.Size = new Size(panel.Width, 23);
            button3.Size = new Size(panel.Width, 23);
            _button4.Size = new Size(panel.Width, 23);
            _browser.Size = new Size(panel.Width, 23);
            button1.Top = height + 9;
            button1.Left = 3;
            button2.Top = button1.Bottom;
            button2.Left = 3;
            button3.Top = button2.Bottom;
            button3.Left = 3;
            _button4.Top = button3.Bottom;
            _button4.Left = 3;
            PerformAutoScale();
        }

        private static void DeleteDirectory(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }

        #endregion

        #region Downloading and installing

        private static void Download(Uri uri, string path, string name)
        {
            var download = new DownloadHelper(uri, path, name);
            download.ShowDialog();
        }

        private void Install(string mod, bool isUpdate, bool isEnabled)
        {
            Download
            (
                new Uri(_modsList.First(m => m.Name == mod).Link),
                $"{Properties.Settings.Default.modFolder}/{mod}.zip",
                mod
            );

            InstallMods
            (
                $"{Properties.Settings.Default.modFolder}/{mod}.zip",
                Properties.Settings.Default.temp,
                isEnabled
            );

            File.Delete($"{Properties.Settings.Default.modFolder}/{mod}.zip");

            MessageBox.Show(isUpdate ? $"{mod} successfully updated!" : $"{mod} successfully installed!");
        }

        #region Unpacking and moving/copying/deleting files

        private void InstallApi(string api, string tempFolder)
        {
            ZipFile.ExtractToDirectory(api, tempFolder);
            IEnumerable<string> mods = Directory.EnumerateDirectories(tempFolder).ToList();
            IEnumerable<string> files = Directory.EnumerateFiles(tempFolder).ToList();
            if (_assemblyIsAPI)
            {
                File.Copy
                (
                    $"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.dll",
                    $"{Properties.Settings.Default.APIFolder}/Assembly-CSharp.vanilla",
                    true
                );
            }

            if (!files.Any(f => f.Contains(".dll")))
            {
                string[] modDll = Directory.GetFiles(tempFolder, "*.dll", SearchOption.AllDirectories);
                foreach (string dll in modDll)
                    File.Copy(dll, $"{Properties.Settings.Default.APIFolder}/{Path.GetFileName(dll)}", true);
                foreach (string mod in mods)
                {
                    string[] dll = Directory.GetFiles(mod, "*.dll", SearchOption.AllDirectories);
                    if (dll.Length == 0)
                    {
                        MoveDirectory(mod, $"{Properties.Settings.Default.installFolder}/{Path.GetFileName(mod)}/");
                    }
                }

                foreach (string file in files)
                {
                    File.Copy
                    (
                        file,
                        $"{Properties.Settings.Default.installFolder}/{Path.GetFileNameWithoutExtension(file)}({Path.GetFileNameWithoutExtension(api)}){Path.GetExtension(file)}",
                        true
                    );
                    File.Delete(file);
                }

                Directory.Delete(tempFolder, true);
            }
            else
            {
                foreach (string file in files)
                {
                    File.Copy
                    (
                        file,
                        file.Contains("*.txt")
                            ? $"{Properties.Settings.Default.installFolder}/{Path.GetFileNameWithoutExtension(file)}({Path.GetFileNameWithoutExtension(api)}){Path.GetExtension(file)}"
                            : $"{Properties.Settings.Default.modFolder}/{Path.GetFileName(file)}",
                        true
                    );
                    File.Delete(file);
                }

                Directory.Delete(tempFolder, true);
            }

            _apiIsInstalled = true;
            Properties.Settings.Default.Save();
        }

        private void InstallMods(string mod, string tempFolder, bool isEnabled)
        {
            if (Directory.Exists(Properties.Settings.Default.temp))
                Directory.Delete(tempFolder, true);
            if (!Directory.Exists(Properties.Settings.Default.modFolder))
                Directory.CreateDirectory(Properties.Settings.Default.modFolder);

            ZipFile.ExtractToDirectory(mod, tempFolder);
            List<string> files = Directory.EnumerateFiles(tempFolder, "*", SearchOption.AllDirectories).ToList();

            foreach (string file in files)
            {
                switch (Path.GetExtension(file))
                {
                    case ".dll":
                        File.Copy
                        (
                            file,
                            isEnabled
                                ? $"{Properties.Settings.Default.modFolder}/{Path.GetFileName(file)}"
                                : $"{Properties.Settings.Default.modFolder}/Disabled/{Path.GetFileName(file)}",
                            true
                        );
                        break;
                    case ".txt":
                    case ".md":
                        File.Copy
                        (
                            file,
                            $"{Properties.Settings.Default.installFolder}/{Path.GetFileNameWithoutExtension(file)}({Path.GetFileNameWithoutExtension(mod)}){Path.GetExtension(file)}",
                            true
                        );
                        break;
                    case ".ini":
                        break;
                    default:
                        string path = Path.GetDirectoryName(file)
                                          ?.Replace
                                          (
                                              Properties.Settings.Default.temp,
                                              Properties.Settings.Default.installFolder
                                          );
                        if (!Directory.Exists(path))
                            if (path != null)
                                Directory.CreateDirectory(path);
                        File.Copy(file, $"{path}/{Path.GetFileName(file)}", true);
                        break;
                }
            }

            Directory.Delete(tempFolder, true);

            _installedMods.Add(mod);
        }

        private static void MoveDirectory(string source, string target)
        {
            string sourcePath = source.TrimEnd('\\', ' ');
            string targetPath = target.TrimEnd('\\', ' ');
            IEnumerable<IGrouping<string, string>> files = Directory.EnumerateFiles
                                                                    (
                                                                        sourcePath,
                                                                        "*",
                                                                        SearchOption.AllDirectories
                                                                    )
                                                                    .GroupBy(Path.GetDirectoryName);

            foreach (IGrouping<string, string> folder in files)
            {
                string targetFolder = folder.Key.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (string file in folder)
                {
                    string targetFile = Path.Combine
                    (
                        targetFolder,
                        Path.GetFileName(file) ?? throw new NoNullAllowedException("File name is null!")
                    );
                    if (File.Exists(targetFile))
                    {
                        if (!File.Exists($"{targetFolder}/{Path.GetFileName(targetFile)}.vanilla"))
                        {
                            File.Move(targetFile, $"{targetFolder}/{Path.GetFileName(targetFile)}.vanilla");
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

        internal static string OSPath(string os)
        {
            return os == "MacOS"
                ? $"{Properties.Settings.Default.installFolder}/Contents/Resources/Data/Managed"
                : $"{Properties.Settings.Default.installFolder}/hollow_knight_Data/Managed";
        }

        internal static bool PathCheck(string os, FolderBrowserDialog fb)
        {
            return os == "MacOS"
                ? File.Exists
                (
                    fb.SelectedPath + "/Contents/Resources/Data/Managed/Assembly-CSharp.dll"
                )
                && new[] {"hollow_knight.app", "Hollow Knight.app"}.Contains(Path.GetFileName(fb.SelectedPath))
                : File.Exists(fb.SelectedPath + "/hollow_knight_Data/Managed/Assembly-CSharp.dll");
        }
    }
}