// Uncomment these only if you want to export GetString() or ExecuteBang().
//#define DLLEXPORT_GETSTRING
//#define DLLEXPORT_EXECUTEBANG

using System;
using System.IO;
using System.Runtime.InteropServices;
using Rainmeter;

using Ovh.RestLib;
using RestSharp;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using PluginOVH.Models;
using PluginOVH.Models.Dedicated;
using PluginOVH.Models.Vps;
using System.Diagnostics;

// Overview: This is a blank canvas on which to build your plugin.

// Note: Measure.GetString, Plugin.GetString, Measure.ExecuteBang, and
// Plugin.ExecuteBang have been commented out. If you need GetString
// and/or ExecuteBang and you have read what they are used for from the
// SDK docs, uncomment the function(s). Otherwise leave them commented out
// (or get rid of them)!

namespace PluginOVH
{
    internal class Measure
    {
        private enum Service
        {
            Dedicated,
            VPS
        }
        private enum MeasureSource
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
        private enum Provider
        {
            OvhCA,
            OvhEU,
            KimsufiCA,
            KimsufiEU
        }

        // measure attributes
        private OvhApi _ovhApi;
        private Dedicated _dedicatedInformations;
        private Vps _vpsInformations;
        private HardwareSpecifications _dedicatedHardware;
        private List<string> _ips;

        private Service _service;
        private Provider _provider;
        private MeasureSource _measure;
        private bool _consumerKeyActivated;
        // measure settings
        private string _applicationKey;
        private string _applicationSecret;
        private string _serverName;

        #region static data
        private HardwareSpecifications getDedicatedHardwareSpecs()
        {
            try
            {
                return this._ovhApi.Get<HardwareSpecifications>(String.Format("dedicated/server/{0}/specifications/hardware", this._serverName));
            }
            catch (AggregateException e)
            {
                API.Log(API.LogType.Error, e.InnerException.Message);
                return null;
            }
        }

        private Dedicated getDedicatedInformations()
        {
            try
            {
                return this._ovhApi.Get<Dedicated>(String.Format("dedicated/server/{0}", this._serverName));
            }
            catch (AggregateException e)
            {
                API.Log(API.LogType.Error, e.InnerException.Message);
                return null;
            }
        }

        private Vps getVpsInformations()
        {
            try
            {
                return this._ovhApi.Get<Vps>(String.Format("vps/{0}", this._serverName));
            }
            catch (AggregateException e)
            {
                API.Log(API.LogType.Error, e.InnerException.Message);
                return null;
            }
        }

        private List<string> getServerIps()
        {
            try
            {
                switch (this._service)
                {
                    case Service.Dedicated:
                        return this._ovhApi.Get<List<string>>(String.Format("dedicated/server/{0}/ips", this._serverName));
                    case Service.VPS:
                        return this._ovhApi.Get<List<string>>(String.Format("vps/{0}/ips", this._serverName));
                }
            }
            catch(AggregateException e)
            {
                API.Log(API.LogType.Error, e.InnerException.Message);
            }

            return null;
        }
        #endregion

        private DedicatedServerRtmLoad getDedicatedStatsLoad()
        {
            try
            {
                return this._ovhApi.Get<DedicatedServerRtmLoad>(String.Format("dedicated/server/{0}/statistics/load", this._serverName));
            }
            catch (AggregateException e)
            {
                API.Log(API.LogType.Error, e.InnerException.Message);
                return null;
            }
        }

        internal Measure()
        {
            // measure attributes
            this._consumerKeyActivated = false;
            this._measure = MeasureSource.CPULoad;
            this._provider = Provider.OvhEU;
            this._service = Service.Dedicated;
            // measure settings
            this._applicationKey = "";
            this._applicationSecret = "";
            this._serverName = "";
        }

        private void loadSettings(API api)
        {
            string missingParameterMessage = "'{0}' is mandatory and no value has been set";
            string missingParameterDetailledMessage = "'{0}' is mandatory and no value has been set or the value is not valid. The default value '{1}' will be use for the measure '{2}'.";

            #region ApplicationKey consistency check
            this._applicationKey = api.ReadString("ApplicationKey", "");
            if (this._applicationKey.Equals(""))
                API.Log(API.LogType.Warning, String.Format(missingParameterMessage, "ApplicationKey"));
            #endregion

            #region ApplicationSecret consistency check
            this._applicationSecret = api.ReadString("ApplicationSecret", "");
            if (this._applicationSecret.Equals(""))
                API.Log(API.LogType.Warning, String.Format(missingParameterMessage, "ApplicationSecret"));
            #endregion

            #region ServerOvhName consistency check
            this._serverName = api.ReadString("ServerName", "");
            if (this._serverName.Equals(""))
                API.Log(API.LogType.Warning, String.Format(missingParameterMessage, "ServerName"));
            #endregion

            #region MeasureSource consistency check
            string measure = api.ReadString("MeasureSource", "");

            if (Enum.IsDefined(typeof(MeasureSource), measure))
                this._measure = (MeasureSource)Enum.Parse(typeof(MeasureSource), measure, true);
            else
                API.Log(API.LogType.Warning, String.Format(missingParameterDetailledMessage, "MeasureSource", MeasureSource.CPULoad, api.GetMeasureName()));
            #endregion

            #region Provider consistency check
            string provider = api.ReadString("Provider", "");

            if (Enum.IsDefined(typeof(Provider), provider))
                this._provider = (Provider)Enum.Parse(typeof(Provider), provider, true);
            else
                API.Log(API.LogType.Warning, String.Format(missingParameterDetailledMessage, "Provider", Provider.OvhEU, api.GetMeasureName()));
            #endregion

            #region Service check
            string service = api.ReadString("Service", "");

            if (Enum.IsDefined(typeof(Service), service))
                this._service = (Service)Enum.Parse(typeof(Service), service, true);
            else
                API.Log(API.LogType.Warning, String.Format(missingParameterDetailledMessage, "Service", Service.Dedicated, api.GetMeasureName()));
            #endregion
        }

        private void loadConsumerKey(API api)
        {
            // build the plugin config file path
            string pluginConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rainmeter", "Plugins", "ovhPluginConfig.xml");

            if (!this._applicationKey.Equals("") && !this._applicationSecret.Equals(""))
            {
                // Prepare API call
                try
                {
                    switch (this._provider)
                    {
                        case Provider.OvhEU:
                            this._ovhApi = new OvhApi(this._applicationKey, this._applicationSecret, "ovh-eu");
                            break;
                        case Provider.OvhCA:
                            this._ovhApi = new OvhApi(this._applicationKey, this._applicationSecret, "ovh-ca");
                            break;
                        case Provider.KimsufiEU:
                            this._ovhApi = new OvhApi(this._applicationKey, this._applicationSecret, "kimsufi-eu");
                            break;
                        case Provider.KimsufiCA:
                            this._ovhApi = new OvhApi(this._applicationKey, this._applicationSecret, "kimsufi-ca");
                            break;
                    }

                    // check if the config file exists
                    if (File.Exists(pluginConfigFile))
                    {
                        // The config file exists, search for the current measureName config value
                        XDocument configFile = XDocument.Load(pluginConfigFile);
                        XElement measureConsumerKey = configFile.Element("config").Element(this._provider.ToString()).Element(this._applicationKey);

                        // A config value has been found, load it in memory
                        if (measureConsumerKey != null)
                        {
                            this._ovhApi.ConsumerKey = measureConsumerKey.Value;
                            this._consumerKeyActivated = true;
                        }
                        // Unable to found a config value, call the API for a new one
                        else
                        {
                            // generate a new one and start the credential validation page in the default user browser
                            Process.Start(this._ovhApi.RequestConsumerKey());
                            // and store it inside config file and store the keys
                            if (configFile.Element("config").Element(this._provider.ToString()) != null)
                            {
                                configFile.Element("config").Element(this._provider.ToString()).Add(new XElement(this._applicationKey, this._ovhApi.ConsumerKey));
                            } else
                            {
                                configFile.Element("config").Add(new XElement(this._provider.ToString(), new XElement(this._applicationKey, this._ovhApi.ConsumerKey)));
                            }
                            
                            configFile.Save(pluginConfigFile);
                        }

                    }
                    else
                    {
                        // generate a new one and start the credential validation page in the default user browser
                        Process.Start(this._ovhApi.RequestConsumerKey());
                        // generate config file and store the keys
                        new XDocument(
                            new XElement("config",
                                new XElement(this._provider.ToString(), 
                                    new XElement(this._applicationKey, this._ovhApi.ConsumerKey)
                                )
                            )
                        ).Save(pluginConfigFile);
                    }
                }
                catch (Exception e)
                {
                    API.Log(API.LogType.Error, e.Message);
                }
            }
        }
        
        private void loadBasicsInformations()
        {
            switch (this._service)
            {
                case Service.Dedicated:
                    this._dedicatedInformations = this.getDedicatedInformations();
                    this._dedicatedHardware = this.getDedicatedHardwareSpecs();
                    this._ips = this.getServerIps();
                    break;
                case Service.VPS:
                    this._vpsInformations = this.getVpsInformations();
                    this._ips = this.getServerIps();
                    break;
            }
        }

        #region CPULoad
        private double getCpuLoad()
        {
            switch (this._service)
            {
                case Service.Dedicated:
                    return this.getDedicatedCpuLoad();
                case Service.VPS:
                    switch(this._provider)
                    {
                        case Provider.OvhCA:
                        case Provider.OvhEU:
                            return this.getOvhVpsCpuLoad();
                    }
                    break;
            }

            return 0.0;
        }

        private double getOvhVpsCpuLoad()
        {
            double cpuUsed = 0.0;
            double cpuMax = 1.0;

            try
            {
                // get used ram
                ComplexeTypeUnitAndValue<double> serversLoad = this._ovhApi.Get<ComplexeTypeUnitAndValue<double>>(String.Format("vps/{0}/use?type=cpu:used", this._serverName));
                if (serversLoad != null)
                {
                    API.Log(API.LogType.Debug, String.Format("Current server CPU usage : {0}", serversLoad.value));
                    cpuUsed = serversLoad.value;
                }
                else
                {
                    API.Log(API.LogType.Error, this._ovhApi.LastError().ToString());
                }

                // get total ram
                serversLoad = this._ovhApi.Get<ComplexeTypeUnitAndValue<double>>(String.Format("vps/{0}/use?type=cpu:max", this._serverName));
                if (serversLoad != null)
                {
                    API.Log(API.LogType.Debug, String.Format("Current server CPU max : {0}", serversLoad.value));
                    cpuMax = serversLoad.value;
                }
                else
                {
                    API.Log(API.LogType.Error, this._ovhApi.LastError().ToString());
                }

                // return ratio
                return cpuUsed / cpuMax;
            }
            catch (AggregateException e)
            {
                API.Log(API.LogType.Error, e.InnerException.Message);
                return 0.0;
            }
        }

        private double getDedicatedCpuLoad()
        {
            double cpuLoad = 0.0;
            DedicatedServerRtmLoad load = this.getDedicatedStatsLoad();
            if (load != null) {
                cpuLoad = load.cpu.value;
                API.Log(API.LogType.Debug, String.Format("PluginOVH.dll | Current server CPU usage : {0}", cpuLoad));
            } else {
                API.Log(API.LogType.Notice, this._ovhApi.LastError().ToString());
            }

            return cpuLoad/100;
        }
        #endregion

        #region RAMLoad
        private double getRamLoad()
        {
            switch (this._service)
            {
                case Service.Dedicated:
                    return this.getDedicatedRamLoad();
                case Service.VPS:
                    switch (this._provider)
                    {
                        case Provider.OvhCA:
                        case Provider.OvhEU:
                            return this.getOvhVpsRamLoad();
                    }
                    break;
                    
            }

            return 0.0;
        }

        private double getOvhVpsRamLoad()
        {
            double memUsed = 0.0;
            double memMax = 1.0;

            try
            {
                // get used ram
                ComplexeTypeUnitAndValue<double> serversLoad = this._ovhApi.Get<ComplexeTypeUnitAndValue<double>>(String.Format("vps/{0}/use?type=mem:used", this._serverName));
                if (serversLoad != null)
                {
                    API.Log(API.LogType.Debug, String.Format("Current server RAM usage : {0}", serversLoad.value));
                    memUsed = serversLoad.value;
                }
                else
                {
                    API.Log(API.LogType.Error, this._ovhApi.LastError().ToString());
                }

                // get total ram
                serversLoad = this._ovhApi.Get<ComplexeTypeUnitAndValue<double>>(String.Format("vps/{0}/use?type=mem:max", this._serverName));
                if (serversLoad != null)
                {
                    API.Log(API.LogType.Debug, String.Format("Current server RAM max : {0}", serversLoad.value));
                    memMax = serversLoad.value;
                }
                else
                {
                    API.Log(API.LogType.Error, this._ovhApi.LastError().ToString());
                }

                // return ratio
                return memUsed / memMax;
            }
            catch (AggregateException e)
            {
                API.Log(API.LogType.Error, e.InnerException.Message);
                return 0.0;
            }
        }

        private double getDedicatedRamLoad()
        {
            double ramLoad = 0.0;
            DedicatedServerRtmLoad load = this.getDedicatedStatsLoad();
            if (load != null)
            {
                ramLoad = load.memory.value;
                API.Log(API.LogType.Debug, String.Format("PluginOVH.dll | Current server RAM usage : {0}", ramLoad));
            }
            else
            {
                API.Log(API.LogType.Notice, this._ovhApi.LastError().ToString());
            }

            return ramLoad / 100;
        }
        #endregion

        #region NetworkUsage
        // upload = tx
        // download = rx
        private double getUpload()
        {
            switch (this._service)
            {
                case Service.Dedicated:
                    return this.getDedicatedTraffic("traffic:upload");
                case Service.VPS:
                    switch (this._provider) {
                        case Provider.OvhCA:
                        case Provider.OvhEU:
                            return this.getVpsTraffic("net:tx");
                    }
                    break;
            }

            return 0.0;
        }

        private double getDownload()
        {
            switch (this._service)
            {
                case Service.Dedicated:
                    return this.getDedicatedTraffic("traffic:download");
                case Service.VPS:
                    switch (this._provider)
                    {
                        case Provider.OvhCA:
                        case Provider.OvhEU:
                            return this.getVpsTraffic("net:rx");
                    }
                    break;
            }

            return 0.0;
        }

        private double getDedicatedTraffic(string type)
        {
            try
            {
                List<MrtgTimestampValue> upload = this._ovhApi.Get<List<MrtgTimestampValue>>(String.Format("dedicated/server/{0}/mrtg?period=hourly&type={1}", this._serverName, type));
                if (upload.Count > 0)
                    return upload[upload.Count-1].value.value;
                else
                    return 0.0;
            }
            catch (AggregateException e)
            {
                API.Log(API.LogType.Error, e.InnerException.Message);
            }

            return 0.0;
        }

        private double getVpsTraffic(string type)
        {
            try
            {
                ComplexeTypeUnitAndValue<double> upload = this._ovhApi.Get<ComplexeTypeUnitAndValue<double>>(String.Format("vps/{0}/use?type={1}", this._serverName, type));
                return upload.value;
            }
            catch (AggregateException e)
            {
                API.Log(API.LogType.Error, e.InnerException.Message);
            }

            return 0.0;
        }
        #endregion

        #region RAMSize
        private double getRamSize()
        {
            switch (this._service)
            {
                case Service.Dedicated:
                    return this._dedicatedHardware.memorySize.value;
                case Service.VPS:
                    switch (this._provider)
                    {
                        case Provider.OvhCA:
                        case Provider.OvhEU:
                            return this._vpsInformations.memoryLimit;
                    }
                    break;
            }

            return 0.0;
        }
        #endregion

        #region ServerName
        private string getServerReverseName()
        {
            switch (this._service)
            {
                case Service.Dedicated:
                    return this._dedicatedInformations.reverse;
                case Service.VPS:
                    switch (this._provider)
                    {
                        case Provider.OvhCA:
                        case Provider.OvhEU:
                            return this._vpsInformations.displayName;
                    }
                    break;
            }

            return null;
        }
        #endregion

        #region IP
        private string getFirstIp()
        {
            if (this._ips.Count > 0)
                return this._ips[0];
            return null;
        }

        private string getSecondIp()
        {
            if (this._ips.Count > 1)
                return this._ips[1];
            return null;
        }
        #endregion

        internal void Reload(API api, ref double maxValue)
        {
            API.Log(API.LogType.Debug, "Measure has been reloaded");
            API.Log(API.LogType.Debug, String.Format("Measure name is {0}", api.GetMeasureName()));
            API.Log(API.LogType.Debug, String.Format("Config file is : {0}", api.GetSettingsFile()));

            this.loadSettings(api);
            this.loadConsumerKey(api);

            if (this._consumerKeyActivated)
                this.loadBasicsInformations();
        }

        internal double Update()
        {
            if (this._consumerKeyActivated)
            {
                switch (this._measure)
                {
                    case MeasureSource.RAMLoad:
                        return this.getRamLoad();
                    case MeasureSource.CPULoad:
                        return this.getCpuLoad();
                    case MeasureSource.RAMSize:
                        return this.getRamSize();
                    case MeasureSource.Download:
                        return this.getDownload();
                    case MeasureSource.Upload:
                        return this.getUpload();
                    default:
                        return 0.0;
                }
            } else {
                return 0.0;
            }
        }
        
        internal string GetString()
        {
            if (this._consumerKeyActivated)
            {
                switch (this._measure)
                {
                    case MeasureSource.ReverseName:
                        return this.getServerReverseName();
                    case MeasureSource.FirstIp:
                        return this.getFirstIp();
                    case MeasureSource.SecondIp:
                        return this.getSecondIp();
                    default:
                        return null;
                }
            }

            return null;
        }
        
#if DLLEXPORT_EXECUTEBANG
        internal void ExecuteBang(string args)
        {
        }
#endif
    }

    public static class Plugin
    {
        static IntPtr StringBuffer = IntPtr.Zero;
        
        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            GCHandle.FromIntPtr(data).Free();

            if (StringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Reload(new Rainmeter.API(rm), ref maxValue);
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            return measure.Update();
        }
        
        [DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            if (StringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }

            string stringValue = measure.GetString();
            if (stringValue != null)
            {
                StringBuffer = Marshal.StringToHGlobalUni(stringValue);
            }

            return StringBuffer;
        }

#if DLLEXPORT_EXECUTEBANG
        [DllExport]
        public static void ExecuteBang(IntPtr data, IntPtr args)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.ExecuteBang(Marshal.PtrToStringUni(args));
        }
#endif
    }
}
