using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModInstaller
{
    class ModsList
    {
        public string Name { get; set; }

        public List<string> Filename { get; set; }

        public string Link { get; set; }

        public List<string> Dependencies { get; set; }

        public List<string> Optional { get; set; }
    }
}
