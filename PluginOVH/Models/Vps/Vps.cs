using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginOVH.Models.Vps
{
    class Vps
    {
        public string cluster { get; set; }
        public long memoryLimit { get; set; }
        public string netbootMode { get; set; }
        public string zone { get; set; }
        public string name { get; set; }
        public object model { get; set; }
        public string keymap { get; set; }
        public string state { get; set; }
        public long vcore { get; set; }
        public string offerType { get; set; }
        public bool slaMonitoring { get; set; }
        public string displayName { get; set; }
    }
}
