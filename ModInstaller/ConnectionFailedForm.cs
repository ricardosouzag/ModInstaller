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
    public partial class ConnectionFailedForm : Form
    {
        private ModManager mainForm;

        public ConnectionFailedForm()
        {
            InitializeComponent();
        }

        public ConnectionFailedForm(ModManager sender)
        {
            mainForm = sender;
            InitializeComponent();
        }

        private void ClickOfflineMode(object sender, System.EventArgs e)
        {
            mainForm.isOffline = true;
            Close();
        }

        private void ClickRetry(object sender, System.EventArgs e)
        {
            Dispose();
        }

        private void ClickAbort(object sender, System.EventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
        }
    }
}
