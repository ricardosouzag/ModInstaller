using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;

namespace ModInstaller
{
    public partial class DownloadHelper : Form
    {
        public DownloadHelper()
        {
            InitializeComponent();
        }

        public DownloadHelper(Uri uri, string path, string modname)
        {
            _modname = modname;
            InitializeComponent();
            StartDownload(uri, path);
        }

        private void StartDownload(Uri uri, string path)
        {
            using (_webClient = new WebClient())
            {
                _webClient.DownloadFileCompleted += Completed;
                _webClient.DownloadProgressChanged += ProgressChanged;

                // Start the stopwatch which we will be using to calculate the download speed
                _sw.Start();

                try
                {
                    // Start downloading the file
                    _webClient.DownloadFileAsync(uri, path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        // The event that will fire whenever the progress of the WebClient is changed
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            labelModname.Text = $"Downloading {_modname}";

            // Calculate download speed and output it to labelSpeed.
            labelSpeed.Text = $"{(e.BytesReceived / 1024d / _sw.Elapsed.TotalSeconds).ToString("0.00")} kb/s";

            // Update the progressbar percentage only when the value is not the same.
            progressBar.Value = e.ProgressPercentage;

            // Update the label with how much data have been downloaded so far and the total size of the file we are currently downloading
            labelDownloaded.Text =
                $"{(e.BytesReceived / 1024d / 1024d).ToString("0.00")} MB / {(e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00")} MB";
        }

        // The event that will trigger when the WebClient is completed
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            // Reset the stopwatch.
            _sw.Reset();

            MessageBox.Show(e.Cancelled ? "Download has been canceled." : "Download completed!");
            Close();
        }
        private WebClient _webClient;
        private readonly Stopwatch _sw = new Stopwatch();
        private readonly string _modname;
    }
}
