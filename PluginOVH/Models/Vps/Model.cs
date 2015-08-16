using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginOVH.Models.Vps
{
    class Model
    {
        public long maximumAdditionnalIp { get; set; }
        public List<string> datacenter { get; set; }
        public long disk { get; set; }
        public string offer { get; set; }
        public string version { get; set; }
        public string name { get; set; }
        public List<string> availableOptions { get; set; }
        public long memory { get; set; }
        public long vcore { get; set; }
    }
}
