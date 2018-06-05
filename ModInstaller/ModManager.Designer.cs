using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ModInstaller
{
    partial class ModManager
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModManager));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.InstallList = new System.Windows.Forms.CheckedListBox();
            this.InstalledMods = new System.Windows.Forms.CheckedListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.InstallList);
            this.groupBox1.Controls.Add(this.InstalledMods);
            this.groupBox1.Name = "groupBox1";
            //this.groupBox1.Size = new System.Drawing.Size(269, 354);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "API-Compatible Mods";
             this.groupBox1.MinimumSize = new Size( this.groupBox1.Width,  this.InstalledMods.Height);
             this.groupBox1.MaximumSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
             this.groupBox1.AutoSize = true;
             this.groupBox1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            // 
            // InstallList
            // 
            this.InstallList.FormattingEnabled = true;
            this.InstallList.Location = new System.Drawing.Point(167, 19);
            this.InstallList.Name = "InstallList";
            //this.InstallList.Size = new System.Drawing.Size(96, 319);
            this.InstallList.TabIndex = 2;
            InstallList.ItemCheck += InstallList_ItemCheck;
            // 
            // InstalledMods
            // 
            this.InstalledMods.FormattingEnabled = true;
            this.InstalledMods.Location = new System.Drawing.Point(6, 19);
            this.InstalledMods.Name = "InstalledMods";
            this.InstalledMods.Size = new System.Drawing.Size(179, 319);
            this.InstalledMods.TabIndex = 1;
            InstalledMods.ItemCheck += InstalledMods_ItemCheck;
            // 
            // button1
            // 
            //this.button1.Location = new System.Drawing.Point(12, 372);
            this.button1.Name = "button1";
            this.button1.TabIndex = 1;
            this.button1.Text = "Install Modding API";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.InstallApiClick);
            // 
            // button2
            // 
            //this.button2.Location = new System.Drawing.Point(12, 401);
            this.button2.Name = "button2";
            this.button2.TabIndex = 2;
            this.button2.Text = "Manually Install Mods";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ManualInstallClick);
            // 
            // ModManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            //this.ClientSize = new System.Drawing.Size(294, 436);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ModManager";
            this.Text = "Mod Manager";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion

        private GroupBox groupBox1;
        private CheckedListBox InstalledMods;
        private CheckedListBox InstallList;
        private Button button1;
        private Button button2;
    }
}