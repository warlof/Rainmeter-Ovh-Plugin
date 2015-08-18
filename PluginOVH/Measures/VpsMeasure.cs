using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rainmeter;
using PluginOVH.Models;

namespace PluginOVH.Measures
{
    class VpsMeasure : AbstractMeasure
    {
        private StaticVpsMeasure _data;
        
        private double getVpsUse(string useType)
        {
            try
            {
                // get used cpu
                ComplexeTypeUnitAndValue<double> ressource = this._data.getOvh().Get<ComplexeTypeUnitAndValue<double>>(String.Format("vps/{0}/use?type={1}", this._data.getServerName(), useType));
                if (ressource != null)
                    return ressource.value;
            }
            catch (AggregateException e)
            {
                API.Log(API.LogType.Error, String.Format("PluginOVH.dll | An error occured trying to fetch VPS {0} for the measure {1}. Detailled message is {2}", useType, this._measureName, e.Message));
            }

            return 0.0;
        }
        
        public override void Reload(API api, ref double maxValue)
        {
            #region get servername
            string serverName = api.ReadString("ServerName", "");
            if (serverName.Equals(""))
                API.Log(API.LogType.Warning, String.Format("'{0}' is mandatory and no value has been set", "ServerName"));
            else
                this._data = StaticVpsMeasure.instance(serverName, api);
            #endregion

            #region MeasureSource consistency check
            string measure = api.ReadString("MeasureSource", "");

            if (Enum.IsDefined(typeof(MeasureSource), measure))
                this._measure = (MeasureSource)Enum.Parse(typeof(MeasureSource), measure, true);
            else
                API.Log(API.LogType.Warning, String.Format("'{0}' is mandatory and no value has been set or the value is not valid. The default value '{1}' will be use for the measure '{2}'.",
                    "MeasureSource", MeasureSource.CPULoad, api.GetMeasureName()));
            #endregion

            this._measureName = api.GetMeasureName();
        }

        public override double Update()
        {
            if (this._data.isEnableKey()) {
                switch (this._measure)
                {
                    case MeasureSource.CPULoad:
                        return this.getCpuLoad();
                    case MeasureSource.RAMLoad:
                        return this.getRamLoad();
                    case MeasureSource.RAMSize:
                        return this.getRamSize();
                    case MeasureSource.Upload:
                        return this.getUpload();
                    case MeasureSource.Download:
                        return this.getDownload();
                }
            }

            return 0.0;
        }

        public override string GetString()
        {
            if (this._data.isEnableKey())
            {
                switch (this._measure)
                {
                    case MeasureSource.ReverseName:
                        return this.getReverseName();
                    case MeasureSource.FirstIp:
                        return this._data.getIp(1);
                    case MeasureSource.SecondIp:
                        return this._data.getIp(2);
                }
            }

            return null;
        }

        protected override double getCpuLoad()
        {
            double cpuMax;
            
            cpuMax = this.getVpsUse("cpu:max");
            if (cpuMax > 0)
                return this.getVpsUse("cpu:used") / cpuMax;

            return 0.0;
        }

        protected override double getRamLoad()
        {
            if (this._data.getInfo() != null && this._data.getInfo().memoryLimit > 0)
                return this.getVpsUse("mem:used") / this._data.getInfo().memoryLimit;

            return 0.0;
        }

        protected override double getRamSize()
        {
            if (this._data.getInfo() != null)
                return this._data.getInfo().memoryLimit;
            return 0.0;
        }

        protected override string getReverseName()
        {
            if (this._data.getInfo() != null)
                return this._data.getInfo().displayName;
            return null;
        }
        
        protected override double getUpload()
        {
            return this.getVpsUse("net:tx");
        }

        protected override double getDownload()
        {
            return this.getVpsUse("net:rx");
        }

        ~VpsMeasure()
        {
            this._data.disposeMeasure(this._data.getServerName());
        }
    }
}
