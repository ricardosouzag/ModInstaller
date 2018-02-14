using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp3
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
            label1.Text = "Install path: \n" + folderBrowserDialog1.SelectedPath;
            button4.Enabled = ((folderBrowserDialog1.SelectedPath != "folderBrowserDialog1") && (openFileDialog2.FileName != "openFileDialog2"));
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
                List<string> mods = new List<string>();
                foreach (string mod in openFileDialog2.FileNames)
                {
                    mods.Add(System.IO.Path.GetFileName(mod));
                }
                label3.Text = "Selected file(s): \n" + String.Join("\n", mods.ToArray());
                button4.Enabled = ((folderBrowserDialog1.SelectedPath != "") && (openFileDialog2.FileName != "openFileDialog2"));
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
            MessageBox.Show("Use this application to install API mods.");
        }
    }
}
