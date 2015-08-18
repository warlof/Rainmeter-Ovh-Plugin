using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rainmeter;
using PluginOVH.Models;

namespace PluginOVH.Measures
{
    class DedicatedMeasure : AbstractMeasure
    {
        private StaticDedicatedMeasure _data;

        private double getTraffic(string type)
        {
            try
            {
                List<MrtgTimestampValue> network = this._data.getOvh().Get<List<MrtgTimestampValue>>(String.Format("dedicated/server/{0}/mrtg?period=hourly&type={1}", this._data.getServerName(), type));
                if (network != null && network.Count > 0)
                    return network[network.Count - 1].value.value;
            }
            catch (AggregateException e)
            {
                API.Log(API.LogType.Error, e.InnerException.Message);
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
                this._data = StaticDedicatedMeasure.instance(serverName, api);
            #endregion

            #region MeasureSource consistency check
            string measure = api.ReadString("MeasureSource", "");

            if (Enum.IsDefined(typeof(MeasureSource), measure))
                this._measure = (MeasureSource)Enum.Parse(typeof(MeasureSource), measure, true);
            else
                API.Log(API.LogType.Warning, String.Format("'{0}' is mandatory and no value has been set or the value is not valid. The default value '{1}' will be use for the measure '{2}'.",
                    "MeasureSource", MeasureSource.RAMLoad, api.GetMeasureName()));
            #endregion

            this._measureName = api.GetMeasureName();
        }

        public override double Update()
        {
            if (this._data.isEnableKey())
            {
                switch (this._measure)
                {
                    case MeasureSource.CPULoad:
                        return this.getCpuLoad();
                    case MeasureSource.Download:
                        return this.getDownload();
                    case MeasureSource.RAMLoad:
                        return this.getRamLoad();
                    case MeasureSource.RAMSize:
                        return this.getRamSize();
                    case MeasureSource.Upload:
                        return this.getUpload();
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
                    case MeasureSource.FirstIp:
                        return this._data.getIp(1);
                    case MeasureSource.SecondIp:
                        return this._data.getIp(2);
                    case MeasureSource.ReverseName:
                        return this.getReverseName();
                }
            }

            return null;
        }

        protected override double getCpuLoad()
        {
            if (this._data.getRtmInformations() != null)
            {
                API.Log(API.LogType.Debug, String.Format("PluginOVH.dll | Current server CPU usage : {0}", this._data.getRtmInformations().cpu.value));
                return this._data.getRtmInformations().cpu.value / 100;
            }
            else
            {
                API.Log(API.LogType.Notice, this._data.getOvh().LastError().ToString());
            }

            return 0.0;
        }

        protected override double getDownload()
        {
            return this.getTraffic("traffic:download");
        }

        protected override double getRamLoad()
        {
            if (this._data.getRtmInformations() != null)
            {
                API.Log(API.LogType.Debug, String.Format("PluginOVH.dll | Current server RAM usage : {0} for server {1} in measure {2}",
                    this._data.getRtmInformations().memory.value, this._data.getServerName(), this._measureName));
                return this._data.getRtmInformations().memory.value / 100;
            } else
            {
                API.Log(API.LogType.Notice, this._data.getOvh().LastError().ToString());
            }

            return 0.0;
        }

        protected override double getRamSize()
        {
            return this._data.getHardwareSpecs().memorySize.value;
        }

        protected override string getReverseName()
        {
            return this._data.getInfo().reverse;
        }
        
        protected override double getUpload()
        {
            return this.getTraffic("traffic:upload");
        }
    }
}
