using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginOVH.Models
{
    class MrtgTimestampValue
    {
        public long timestamp { get; set; }
        public ComplexeTypeUnitAndValue<double> value { get; set; }
    }
}
