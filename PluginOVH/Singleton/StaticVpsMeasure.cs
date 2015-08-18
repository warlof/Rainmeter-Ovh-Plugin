using PluginOVH.Models.Vps;
using Rainmeter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginOVH
{
    class StaticVpsMeasure : AbstractStaticMeasure
    {
        private Vps _info;
        private static Dictionary<string, StaticVpsMeasure> _instances = new Dictionary<string, StaticVpsMeasure>();

        private StaticVpsMeasure(API api, string serverName) : base(api, serverName)
        {
            StaticVpsMeasure._instances.Add(serverName, this);

            if (this.isEnableKey())
            {
                try {
                    this._info = this._ovhApi.Get<Vps>(String.Format("vps/{0}", this._serverName));
                } catch (AggregateException e) {
                    API.Log(API.LogType.Error, e.InnerException.Message);
                }
            }
        }

        public static StaticVpsMeasure instance(string serverName, API api)
        {
            if (StaticVpsMeasure._instances.ContainsKey(serverName))
                return StaticVpsMeasure._instances[serverName];

            return new StaticVpsMeasure(api, serverName);
        }

        public Vps getInfo()
        {
            return this._info;
        }

        protected override void setIps()
        {
            try {
                this._ips = this._ovhApi.Get<List<string>>(String.Format("vps/{0}/ips", this._serverName));
            } catch (AggregateException e) {
                API.Log(API.LogType.Error, e.InnerException.Message);
            }
        }
    }
}
