using System;
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


        private void button1_Click_1(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            Properties.Settings.Default.installFolder = folderBrowserDialog1.SelectedPath;
            Properties.Settings.Default.APIFolder = Properties.Settings.Default.installFolder + "\\hollow_knight_data\\managed";
            Properties.Settings.Default.modFolder = Properties.Settings.Default.APIFolder + "\\Mods";
            Properties.Settings.Default.Save();
            button4.Enabled = ((Properties.Settings.Default.installFolder != "") && (openFileDialog2.FileName != "openFileDialog2"));
            System.IO.Directory.CreateDirectory(Properties.Settings.Default.modFolder);
            System.IO.Directory.CreateDirectory(Properties.Settings.Default.APIFolder);
            System.IO.Directory.CreateDirectory(Properties.Settings.Default.modFolder + "\\Disabled");
            label1.Text = "Install path: \n" + Properties.Settings.Default.installFolder;
            button5.Enabled = Properties.Settings.Default.installFolder != "";
        }



        private void button2_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "openFileDialog1")
            label2.Text = "Selected file: \n" + openFileDialog1.FileName;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
            if (openFileDialog2.FileName != "openFileDialog2")
            {
                List<string> newMods = new List<string>();
                foreach (string mod in openFileDialog2.FileNames)
                {
                    newMods.Add(System.IO.Path.GetFileNameWithoutExtension(mod));
                }
                label3.Text = "Selected file(s): \n" + String.Join("\n", newMods.ToArray());
                button4.Enabled = ((Properties.Settings.Default.installFolder != "") && (openFileDialog2.FileName != "openFileDialog2"));
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (string mod in openFileDialog2.FileNames)
            {
                System.IO.File.Copy(mod, System.IO.Path.Combine(folderBrowserDialog1.SelectedPath + "/hollow_knight_data/managed/Mods", System.IO.Path.GetFileName(mod)), true);
            }
            if (openFileDialog1.FileName == "openFileDialog1")
            MessageBox.Show("Succesfully installed mods!");
            else
            {
                System.IO.File.Copy(openFileDialog1.FileName, System.IO.Path.Combine(folderBrowserDialog1.SelectedPath + "/hollow_knight_data/managed", System.IO.Path.GetFileName(openFileDialog1.FileName)), true);
                MessageBox.Show("Succesfully installed API and mods!");
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        public Settings settings;

        private void button5_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.SelectedPath == "")
            {
                MessageBox.Show("Please select an install path.");
            }
            else
            {
                Form2 form2 = new Form2(this);
                form2.Show();
            }
        }
    }
}
