using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginOVH.Models
{
    public class ComplexeTypeUnitAndValue<T>
    {
        public string unit { get; set; }
        public T value { get; set; }
    }
}
