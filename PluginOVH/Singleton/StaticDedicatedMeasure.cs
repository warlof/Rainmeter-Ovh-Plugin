using PluginOVH.Core;
using PluginOVH.Models;
using PluginOVH.Models.Dedicated;
using Rainmeter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginOVH
{
    class StaticDedicatedMeasure : AbstractStaticMeasure
    {
        private Dedicated _info;
        private HardwareSpecifications _hardware;
        private DedicatedServerRtmLoad _rtmInformations;
        private long lastRtmCall;

        private static Dictionary<string, StaticDedicatedMeasure> _instances = new Dictionary<string, StaticDedicatedMeasure>();

        private StaticDedicatedMeasure(API api, string serverName) : base(api, serverName)
        {
            StaticDedicatedMeasure._instances.Add(serverName, this);

            if (this.isEnableKey())
            {
                try {
                    this._hardware = this._ovhApi.Get<HardwareSpecifications>(String.Format("dedicated/server/{0}/specifications/hardware", this._serverName));
                } catch (AggregateException e) {
                    API.Log(API.LogType.Error, e.InnerException.Message);
                }

                try {
                    this._info = this._ovhApi.Get<Dedicated>(String.Format("dedicated/server/{0}", this._serverName));
                } catch (AggregateException e) {
                    API.Log(API.LogType.Error, e.InnerException.Message);
                }
            }
        }

        public static StaticDedicatedMeasure instance(string serverName, API api)
        {
            if (StaticDedicatedMeasure._instances.ContainsKey(serverName))
                return StaticDedicatedMeasure._instances[serverName];
            
            return new StaticDedicatedMeasure(api, serverName);
        }

        public Dedicated getInfo()
        {
            return this._info;
        }

        public HardwareSpecifications getHardwareSpecs()
        {
            return this._hardware;
        }

        public DedicatedServerRtmLoad getRtmInformations()
        {
            // if last call and 20 seconds is before current time, make a new call
            if ((this.lastRtmCall + 20) < Helper.UnixTimeNow())
            {
                try {
                    this._rtmInformations = this._ovhApi.Get<DedicatedServerRtmLoad>(String.Format("dedicated/server/{0}/statistics/load", this._serverName));
                    this.lastRtmCall = Helper.UnixTimeNow();
                } catch (AggregateException e) {

                }
            }

            return this._rtmInformations;
        }

        protected override void setIps()
        {
            try {
                this._ips =  this._ovhApi.Get<List<string>>(String.Format("dedicated/server/{0}/ips", this._serverName));
            } catch (AggregateException e) {
                API.Log(API.LogType.Error, e.InnerException.Message);
            }
        }

    }
}
