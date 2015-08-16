using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginOVH.Models.Dedicated
{
    class HardwareSpecificationsDisk
    {
        public long numberOfDisks { get; set; }
        public string diskType { get; set; }
        public ComplexeTypeUnitAndValue<long> diskSize { get; set; }
        public string raidController { get; set; }
    }
}
