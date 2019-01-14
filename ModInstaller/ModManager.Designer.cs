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
            openFileDialog  = new OpenFileDialog();
            this.folderBrowserDialog1 = new FolderBrowserDialog();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            _button4 = new Button();
            button3 = new System.Windows.Forms.Button();
            panel = new Panel();
            _browser = new WebBrowser();
            this.SuspendLayout();
            CenterToScreen();
            CenterToParent();
            //
            //
            //
            this.openFileDialog.Filter = "Mod files|*.zip; *.dll|All files|*.*";
            this.openFileDialog.Multiselect = true;
            this.openFileDialog.Title = "Select the mods you wish to install";
            openFileDialog.FileOk += DoManualInstall;
            //
            // panel
            //
            panel.AutoScroll = true;
            panel.Size = new Size(500, 1);
            // 
            // button1
            // 
            //this.button1.Location = new System.Drawing.Point(12, 372);
            this.button1.Name = "button1";
            this.button1.TabIndex = 1;
            this.button1.Text = "Disable all mods (revert to vanilla)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.EnableApiClick);
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
            // button3
            //
            this.button3.Name = "button3";
            this.button3.TabIndex = 3;
            this.button3.Text = "Change Default Install Path";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.ChangePathClick);
            //
            //button4
            //
            _button4.Text = "If you liked this installer, please consider making a donation!";
            _button4.Click += DonateButtonClick;
            // 
            // ModManager
            // 
            this.ClientSize = new System.Drawing.Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            Controls.Add(button3);
            Controls.Add(_button4);
            Controls.Add(panel);
//            Controls.Add(_browser);
            MaximizeBox = false;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ModManager";
            this.Text = "Mod Manager";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.ControlBox = true;
        }

        


        #endregion

        private OpenFileDialog openFileDialog;
        private FolderBrowserDialog folderBrowserDialog1;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button _button4;
        private WebBrowser _browser;
        private Panel panel;
    }
}