using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenovaLauncher
{
    class MinecraftVersion
    {
        private string _name;
        private string _uuid;
        private bool _isBeta;

        public MinecraftVersion(string name, string uuid, bool isBeta)
        {
            _name = name;
            _uuid = uuid;
            _isBeta = isBeta;
        }

        public string Name
        {
            get { return _name + (_isBeta ? " (Beta)" : ""); }
        }
    }
}
