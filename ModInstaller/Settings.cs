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
    public class Settings
    {
        public string modFolder;
        public string APIFolder;
        public List<string> installedMods;
        public string installPath;
    }
}
