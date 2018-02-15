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
        private void PopulateCheckBox(CheckedListBox modlist, string Folder, string FileType)
        {
            DirectoryInfo dinfo = new DirectoryInfo(Folder);
            FileInfo[] Files = dinfo.GetFiles(FileType);
            foreach (FileInfo file in Files)
            {
                modlist.Items.Add((System.IO.Path.GetFileNameWithoutExtension(file.Name)));
                installedMods.Add(file.Name);
            }
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
            PopulateCheckBox(InstalledMods, Properties.Settings.Default.modFolder, "*.dll");
            for (int i = 0; i < InstalledMods.Items.Count; i++)
            {
                InstalledMods.SetItemCheckState(i, CheckState.Checked);
            }
            PopulateCheckBox(InstalledMods, Properties.Settings.Default.modFolder + "\\Disabled", "*.dll");
        }

        private void InstalledMods_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                if (System.IO.File.Exists(Properties.Settings.Default.modFolder + "\\Disabled\\" + installedMods[e.Index]))
                {
                    System.IO.File.Move(Properties.Settings.Default.modFolder + "\\Disabled\\" + installedMods[e.Index], Properties.Settings.Default.modFolder + "\\" + installedMods[e.Index]);
                }
            }
            else
            {
                System.IO.File.Move(Properties.Settings.Default.modFolder + "\\" + installedMods[e.Index], Properties.Settings.Default.modFolder + "\\Disabled\\" + installedMods[e.Index]);
            }
        }

        private List<string> installedMods = new List<string>();

    }
}
