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

        public static RedisWrapperConfiguration GetConfig()
        {
            return GetConfig<RedisWrapperConfiguration>("redis", "cache",true);
        }

        protected override void LoadValuesFromConfigurationXml(XmlNode node)
        {
            header = node.GetStringAttribute("header","redis");
        }
    }
}
