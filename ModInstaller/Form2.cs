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
            PopulateCheckBox(InstalledMods, mainForm.modFolder, "*.dll");
            for (int i = 0; i < InstalledMods.Items.Count; i++)
            {
                InstalledMods.SetItemCheckState(i, CheckState.Checked);
            }
            PopulateCheckBox(InstalledMods, mainForm.modFolder + "\\Disabled", "*.dll");
        }

        private void InstalledMods_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                System.IO.File.Move(mainForm.modFolder + "\\Disabled\\" + installedMods[e.Index],mainForm.modFolder + "\\" + installedMods[e.Index]);
            }
            else
            {
                System.IO.File.Move(mainForm.modFolder + "\\" + installedMods[e.Index], mainForm.modFolder + "\\Disabled\\" + installedMods[e.Index]);
            }
        }

        private List<string> installedMods = new List<string>();

    }
}
