using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginOVH.Models
{
    class DedicatedServerRtmLoad
    {
        public ComplexeTypeUnitAndValue<double> cpu { get; set; }
        public double loadavg1 { get; set; }
        public double loadavg5 { get; set; }
        public ComplexeTypeUnitAndValue<double> memory { get; set; }
        public long uptime { get; set; }
        public double loadavg15 { get; set; }
        public ComplexeTypeUnitAndValue<double> swap { get; set; }
        public long processRunning { get; set; }
        public long processCount { get; set; }
    }
}
