using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisWrapper
{
    public enum ConnectType:byte
    {
        /// <summary>
        /// 一个Http Request一个连接
        /// </summary>
        HttpRequest = 1,

        /// <summary>
        /// 每次访问reids一个连接
        /// </summary>
        EachAccess = 2
    }
}
