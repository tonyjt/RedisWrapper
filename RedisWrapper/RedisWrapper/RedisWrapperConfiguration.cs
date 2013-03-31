﻿using Configuration;
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

        private int connectType;

        public ConnectType ConnectType { get { return (ConnectType)connectType; } }

        private bool auth;

        public bool Auth{get{return auth;}}

        private string password;

        public string Password { get { return password; } }

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

            connectType = node.GetIntAttribute("connecttype", 1);

            auth = node.GetBoolAttribute("auth", false);

            password = node.GetStringAttribute("password", "");
        }
    }
}
