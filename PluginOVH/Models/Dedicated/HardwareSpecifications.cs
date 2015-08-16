using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginOVH.Models.Dedicated
{
    class HardwareSpecifications
    {
        public ComplexeTypeUnitAndValue<long> memorySize { get; set; }
        public string processorArchitecture { get; set; }
        public List<HardwareSpecificationsDisk> diskGroups { get; set; }
        public string defaultHardwareRaidType { get; set; }
        public string processorName { get; set; }
        public string description { get; set; }
        public long numberOfProcessors { get; set; }
        public long coresPerProcessor { get; set; }
        public List<ComplexeTypeUnitAndValue<long>> usbKeys { get; set; }
        public ComplexeTypeUnitAndValue<long> defaultHardwareRaidSize { get; set; }
        public string motherboard { get; set; }
    }
}
