using Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CustomExtension;

namespace RedisWrapper
{
    public class RedisWrapperConfiguration:ConfigurationBase
    {
        private string header;

        public string Header { get { return header; } }

        private int defaultExpireSeconds;

        public int DefaultExpireSeconds { get { return defaultExpireSeconds; } }

        private string host;

        public string Host { get { return host; } }

        private int port;

        public int Port { get { return port; } }

        public static RedisWrapperConfiguration GetConfig()
        {
            return GetConfig<RedisWrapperConfiguration>("redis", "cache",true);
        }

        protected override void LoadValuesFromConfigurationXml(XmlNode node)
        {
                header = node.GetStringAttribute("header", "redis");

                defaultExpireSeconds = node.GetIntAttribute("expire", 300);

                host = node.GetStringAttribute("host", "127.0.0.1");

                port = node.GetIntAttribute("port", 6379);
        }
    }
}
