using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginOVH.Measures
{
    abstract class AbstractMeasure
    {
        protected enum MeasureSource
        {
            RAMLoad,
            RAMSize,
            CPULoad,
            ReverseName,
            FirstIp,
            SecondIp,
            Download,
            Upload
        }

        protected MeasureSource _measure;
        protected string _measureName;

        public abstract void Reload(Rainmeter.API api, ref double maxValue);

        public abstract double Update();

        public abstract string GetString();

        protected abstract double getCpuLoad();
        protected abstract double getRamLoad();
        protected abstract double getRamSize();
        protected abstract double getDownload();
        protected abstract double getUpload();
        protected abstract string getReverseName();
    }
}
