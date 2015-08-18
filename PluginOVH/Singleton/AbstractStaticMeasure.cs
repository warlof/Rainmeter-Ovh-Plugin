using Ovh.RestLib;
using Rainmeter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PluginOVH
{
    abstract class AbstractStaticMeasure
    {
        protected enum Provider
        {
            OvhCA,
            OvhEU,
            KimsufiCA,
            KimsufiEU
        }

        private string _consumerKey;
        private string _appKey;
        private string _appSecret;
        private Provider _provider;
        private bool _enableKey;

        protected OvhApi _ovhApi;
        protected string _serverName;
        protected List<string> _ips;
        
        protected AbstractStaticMeasure(API api, string serverName)
        {
            string missingParameterMessage = "'{0}' is mandatory and no value has been set";
            string missingParameterDetailledMessage = "'{0}' is mandatory and no value has been set or the value is not valid. The default value '{1}' will be use for the measure '{2}'.";

            #region ApplicationKey consistency check
            this._appKey = api.ReadString("ApplicationKey", "");
            if (string.IsNullOrEmpty(this._appKey))
                API.Log(API.LogType.Warning, String.Format(missingParameterMessage, "ApplicationKey"));
            #endregion

            #region ApplicationSecret consistency check
            this._appSecret= api.ReadString("ApplicationSecret", "");
            if (string.IsNullOrEmpty(this._appSecret))
                API.Log(API.LogType.Warning, String.Format(missingParameterMessage, "ApplicationSecret"));
            #endregion
            
            #region Provider consistency check
            string provider = api.ReadString("Provider", "");

            if (Enum.IsDefined(typeof(Provider), provider))
                this._provider = (Provider)Enum.Parse(typeof(Provider), provider, true);
            else
                API.Log(API.LogType.Warning, String.Format(missingParameterDetailledMessage, "Provider", Provider.OvhEU, api.GetMeasureName()));
            #endregion

            this._serverName = serverName;

            this.loadConsumerKey();

            if (this._enableKey)
            {
                this.setIps();
            }
        }

        private void loadConsumerKey()
        {
            // build the plugin config file path
            string pluginConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rainmeter", "Plugins", "ovhPluginConfig.xml");
            XDocument config;
            XElement configNode;
            XElement providerNode;
            XElement applicationNode;

            if (!string.IsNullOrEmpty(this._appKey) && !string.IsNullOrEmpty(this._appSecret))
            {
                // Prepare API call
                try
                {
                    switch (this._provider)
                    {
                        case Provider.OvhEU:
                            this._ovhApi = new OvhApi(this._appKey, this._appSecret, "ovh-eu");
                            break;
                        case Provider.OvhCA:
                            this._ovhApi = new OvhApi(this._appKey, this._appSecret, "ovh-ca");
                            break;
                        case Provider.KimsufiEU:
                            this._ovhApi = new OvhApi(this._appKey, this._appSecret, "kimsufi-eu");
                            break;
                        case Provider.KimsufiCA:
                            this._ovhApi = new OvhApi(this._appKey, this._appSecret, "kimsufi-ca");
                            break;
                    }

                    // check if the config file exists
                    if (File.Exists(pluginConfigFile))
                    {
                        // The config file exists, search for the current measureName config value
                        config = XDocument.Load(pluginConfigFile);
                        configNode = config.Element("config");

                        if (configNode != null)
                        {
                            providerNode = configNode.Element(this._provider.ToString());
                            // A provider node has been found, continue the exploration
                            if (providerNode != null)
                            {
                                applicationNode = config.Element("config").Element(this._provider.ToString()).Element(this._appKey);

                                // A config value has been found, load it in memory
                                if (applicationNode != null)
                                {
                                    this._ovhApi.ConsumerKey = applicationNode.Value;
                                    this._enableKey = true;
                                }

                                // No provider node found, creating it and generate further nodes
                            }
                            else
                            {
                                // launch credential validation page in the user browser
                                Process.Start(this._ovhApi.RequestConsumerKey());
                                // create a new application node with the generated consumer key
                                applicationNode = new XElement(this._appKey, this._ovhApi.ConsumerKey);
                                // append it to the provider node
                                providerNode = new XElement(this._provider.ToString(), applicationNode);
                                // append it to the root config node
                                config.Element("config").Add(providerNode);
                                config.Save(pluginConfigFile);
                            }
                            // No config node found, creating it and generate further nodes
                        }
                        else
                        {
                            // launch credential validation page in the user browser
                            Process.Start(this._ovhApi.RequestConsumerKey());
                            // create a new application node with the generated consumer key
                            applicationNode = new XElement(this._appKey, this._ovhApi.ConsumerKey);
                            // append it to the provider node
                            providerNode = new XElement(this._provider.ToString(), applicationNode);
                            // append it to the root config node
                            configNode = new XElement("config", providerNode);
                            config.Add(configNode);
                            config.Save(pluginConfigFile);
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
                                    new XElement(this._appKey, this._ovhApi.ConsumerKey)
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

        public bool isEnableKey()
        {
            return this._enableKey;
        }

        protected abstract void setIps();

        public string getIp(int number)
        {
            if (this._ips != null && this._ips.Count >= number)
                return this._ips[number-1];
            return null;
        }

        public OvhApi getOvh()
        {
            return this._ovhApi;
        }

        public string getServerName()
        {
            return this._serverName;
        }
    }
}
