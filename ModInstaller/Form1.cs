using System;
using System.IO;
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

        private void button2_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "")
            label2.Text = "Selected file:\n" + openFileDialog1.FileName;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
            if (openFileDialog2.FileName != "")
            {
                List<string> newMods = new List<string>();
                foreach (string mod in openFileDialog2.FileNames)
                {
                    newMods.Add(System.IO.Path.GetFileNameWithoutExtension(mod));
                }
                label3.Text = "Selected file(s):\n " + String.Join("\n", newMods.ToArray());
                button4.Enabled = (openFileDialog2.FileName != "");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (string mod in openFileDialog2.FileNames)
            {
                System.IO.File.Copy(mod, System.IO.Path.Combine(Properties.Settings.Default.modFolder, System.IO.Path.GetFileName(mod)), true);
            }
            if (openFileDialog1.FileName == "")
            MessageBox.Show("Succesfully installed mods!");
            else
            {
                System.IO.File.Copy(openFileDialog1.FileName, System.IO.Path.Combine(Properties.Settings.Default.APIFolder, System.IO.Path.GetFileName(openFileDialog1.FileName)), true);
                MessageBox.Show("Succesfully installed API and mods!");
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Finding the local installation path for Hollow Knight
            if (Properties.Settings.Default.installFolder == "")
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();

                foreach (DriveInfo d in allDrives)
                {
                    if (d.DriveFormat == "NTFS")
                    {
                        if (System.IO.Directory.Exists(d.Name + @"Program Files (x86)\Steam\steamapps\common\Hollow Knight"))
                        {
                            DialogResult dialogResult = MessageBox.Show("Is this your Hollow Knight installation path?\n" + d.Name + @"Program Files (x86)\Steam\steamapps\common\Hollow Knight", "Path confirmation", MessageBoxButtons.YesNo);
                            if (dialogResult == DialogResult.Yes)
                            {
                                Properties.Settings.Default.installFolder = d.Name + @"Program Files (x86)\Steam\steamapps\common\Hollow Knight";
                            }
                            else if (dialogResult == DialogResult.No)
                            {
                                Form3 form3 = new Form3();
                                this.Hide();
                                form3.FormClosed += new FormClosedEventHandler(form3_FormClosed);
                                form3.Show();
                            }
                        }
                        else if (System.IO.Directory.Exists(d.Name + @"Program Files\Steam\steamapps\common\Hollow Knight"))
                        {
                            DialogResult dialogResult = MessageBox.Show("Is this your Hollow Knight installation path?\n" + d.Name + @"Program Files\Steam\steamapps\common\Hollow Knight", "Path confirmation", MessageBoxButtons.YesNo);
                            if (dialogResult == DialogResult.Yes)
                            {
                                Properties.Settings.Default.installFolder = d.Name + @"Program Files\Steam\steamapps\common\Hollow Knight";
                            }
                            else if (dialogResult == DialogResult.No)
                            {
                                Form3 form3 = new Form3();
                                this.Hide();
                                form3.FormClosed += new FormClosedEventHandler(form3_FormClosed);
                                form3.Show();
                            }
                        }
                        else if (System.IO.Directory.Exists(d.Name + @"Steam\steamapps\common\Hollow Knight"))
                        {
                            DialogResult dialogResult = MessageBox.Show("Is this your Hollow Knight installation path?\n" + d.Name + @"Steam\steamapps\common\Hollow Knight", "Path confirmation", MessageBoxButtons.YesNo);
                            if (dialogResult == DialogResult.Yes)
                            {
                                Properties.Settings.Default.installFolder = d.Name + @"Steam\steamapps\common\Hollow Knight";
                            }
                            else if (dialogResult == DialogResult.No)
                            {
                                Form3 form3 = new Form3();
                                this.Hide();
                                form3.FormClosed += new FormClosedEventHandler(form3_FormClosed);
                                form3.Show();
                            }
                        }
                        Properties.Settings.Default.APIFolder = Properties.Settings.Default.installFolder + @"\hollow_knight_data\managed";
                        Properties.Settings.Default.modFolder = Properties.Settings.Default.APIFolder + @"\Mods";
                        Properties.Settings.Default.Save();
                        break;
                    }
                    break;
                }
            }
                
        }

        void label_Paint(object sender, PaintEventArgs e)
        {
            //To show long paths for API
            Label label = (Label)sender;
            using (SolidBrush b = new SolidBrush(label.BackColor))
                e.Graphics.FillRectangle(b, label.ClientRectangle);
            TextRenderer.DrawText(
                e.Graphics,
                label.Text,
                label.Font,
                label.ClientRectangle,
                label.ForeColor,
                TextFormatFlags.PathEllipsis);
        }

        void form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Show();
        }
        public Settings settings;

        private void button5_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2(this);
            form2.Show();
        }
    }
}
