using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModInstaller
{
    public partial class Form2 : Form
    {
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
            foreach (KeyValuePair<string,string> kvp in downloadList)
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
            downloadList.Add("https://drive.google.com/open?id=0BzihlMHqh5UpTGEwVHJXc05NeGM", "RandomizerMod");
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
            PopulateCheckBox(InstalledMods, InstallList, Properties.Settings.Default.modFolder, "*.dll");
            for (int i = 0; i < InstalledMods.Items.Count; i++)
            {
                InstalledMods.SetItemCheckState(i, CheckState.Checked);
            }
            if (!Directory.Exists(Properties.Settings.Default.modFolder + @"\Disabled"))
                Directory.CreateDirectory(Properties.Settings.Default.modFolder + @"\Disabled");
            PopulateCheckBox(InstalledMods, InstallList, Properties.Settings.Default.modFolder + @"\Disabled", "*.dll");
            PopulateCheckBoxLink(InstalledMods, InstallList, downloadList);
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
            else
            {
                File.Move(Properties.Settings.Default.modFolder + @"\" + installedMods[e.Index], Properties.Settings.Default.modFolder + @"\Disabled\" + installedMods[e.Index]);
            }
        }

        private List<string> installedMods = new List<string>();
        private Dictionary<string,string> downloadList = new Dictionary<string, string>();
    }
}
