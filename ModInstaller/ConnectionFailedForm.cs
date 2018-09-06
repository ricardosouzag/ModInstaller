using System;
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

        private void ClickOfflineMode(object sender, EventArgs e)
        {
            mainForm.IsOffline = true;
            Close();
        }

        private void ClickRetry(object sender, EventArgs e)
        {
            Dispose();
        }

        private void ClickAbort(object sender, EventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
        }
    }
}
