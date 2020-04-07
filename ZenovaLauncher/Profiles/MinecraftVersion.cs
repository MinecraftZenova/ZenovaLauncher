using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenovaLauncher
{
    public class MinecraftVersion
    {
        private string _name;
        private string _uuid;

        public MinecraftVersion(string name, string uuid, bool isBeta = false, bool isHistorical = false)
        {
            _name = name;
            _uuid = uuid;
            Beta = isBeta;
            Historical = isHistorical;
        }

        public string Name
        {
            get { return _name + (Beta ? " (Beta)" : ""); }
        }

        public bool Beta { get; set; }
        public bool Historical { get; set; }
        public bool Release { get { return !Beta && !Historical; } }
    }
}
