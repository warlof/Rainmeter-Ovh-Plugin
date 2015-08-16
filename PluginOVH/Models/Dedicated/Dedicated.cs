using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginOVH.Models.Dedicated
{
    class Dedicated
    {
        public string datacenter { get; set; }
        public bool professionalUse { get; set; }
        public string supportLevel { get; set; }
        public string ip { get; set; }
        public string name { get; set; }
        public string commercialRange { get; set; }
        public string os { get; set; }
        public string state { get; set; }
        public string reverse { get; set; }
        public long serverId { get; set; }
        public bool monitoring { get; set; }
        public string rack { get; set; }
        public string rootDevice { get; set; }
        public long linkSpeed { get; set; }
        public long bootId { get; set; }
    }
}
