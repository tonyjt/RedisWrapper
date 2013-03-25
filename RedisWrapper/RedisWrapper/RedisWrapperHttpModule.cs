using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RedisWrapper
{
    public class RedisWrapperHttpModule:IHttpModule
    {
        public void Init(HttpApplication app)
        {
            app.EndRequest += new EventHandler(EndRequestEventHandler);
        }

        private static void EndRequestEventHandler(object sender, EventArgs args)
        {
            string clientKey = RedisCacheWrapper.clientKey;
            HttpContext context = ((HttpApplication)sender).Context;
            if (context.Items.Contains(clientKey))
            {
                RedisClient client = context.Items[clientKey] as RedisClient;
                client.Dispose();
            }
        }
        public void Dispose()
        {
        }

    }
}
