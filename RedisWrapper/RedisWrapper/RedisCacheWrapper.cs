using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RedisWrapper
{
    public class RedisCacheWrapper
    {
        //private static string Header = RedisWrapperConfiguration.GetConfig().Header;
        public static string clientKey = "redisclient";
        /// <summary>
        /// Get Typed Value
        /// </summary>
        /// <typeparam name="T">generic class</typeparam>
        /// <param name="key">key </param>
        /// <returns></returns>
        public static T GetTypedValue<T>(string key)
        {
            string fullKey = GetKey<T>(key);

            return Get<T>(fullKey);
        }

        public static T GetTypedValue<T>(string keyHeader, string key)
        {
            return Get<T>(GetKey<T>(keyHeader, key));
        }


        /// <summary>
        /// Get Typed Value And SetEntry if it is null
        /// </summary>
        /// <typeparam name="P">parameter</typeparam>
        /// <typeparam name="R">result</typeparam>
        /// <param name="para">parameter</param>
        /// <param name="funGet">Get real value if it is null</param>
        /// <param name="expireSeconds">-1：not expired, 0:default expire secons</param>
        /// <returns></returns>
        public static R GetTypedValue<P, R>(P para, Func<P, R> funGet, int expireSeconds = 0)
        {
            string key = GetKey<R>(para.ToString());

            return Get<P, R>(key, para, funGet, expireSeconds);

        }

        public static R GetTypedValue<P, R>(string keyHeader, P para, Func<P, R> funGet, int expireSeconds = 0)
        {
            string key = GetKey<R>(keyHeader,para.ToString());

            return Get<P, R>(key, para, funGet, expireSeconds);
        }

        /// <summary>
        /// Get Typed Value And SetEntry if it is null
        /// </summary>
        /// <typeparam name="P">parameter</typeparam>
        /// <typeparam name="R">result</typeparam>
        /// <param name="para">parameter</param>
        /// <param name="funGet">Get real value if it is null</param>
        /// <param name="expireSeconds">-1：not expired, 0:default expire secons</param>
        /// <returns></returns>
        private static R Get<P, R>(string fullKey, P para, Func<P, R> funGet, int expireSeconds = 0)
        {
            R result = Get<R>(fullKey);

            if (EqualsDefaultValue<R>(result))
            {
                result = funGet(para);

                try
                {
                    Set<R>(fullKey, result, expireSeconds);
                }
                catch (RedisWrapperException ex)
                {
                    //..Write log
                }
            }

            return result;
        }


        public static ICollection<T> GetTypedValueCollection<T>(string key)
        {
            string fullKey = GetCollectionKey<T>(key);

            return Get<ICollection<T>>(fullKey);
        }

        public static ICollection<T> GetTypedValueCollection<T>(string keyHeader, string key)
        {
            return Get<ICollection<T>>(GetKey<T>(keyHeader, key));
        }

        public static ICollection<R> GetTypedValueCollection<P, R>(P para, Func<P, ICollection<R>> funGet, int expireSeconds = 0)
        {
            string key = GetKey<P>(para.ToString());

            return GetCollection<P, R>(key, para, funGet, expireSeconds);
        }

        public static ICollection<R> GetTypedValueCollection<P, R>(string keyHeader,P para, Func<P, ICollection<R>> funGet, int expireSeconds = 0)
        {
            string key = GetKey<P>(keyHeader,para.ToString());

            return GetCollection<P, R>(key, para, funGet, expireSeconds);
        }

        public static ICollection<R> GetCollection<P, R>(string fullKey,P para, Func<P, ICollection<R>> funGet, int expireSeconds = 0)
        {

            ICollection<R> result = Get<ICollection<R>>(fullKey);

            if (EqualsDefaultValue<ICollection<R>>(result))
            {
                result = funGet(para);

                try
                {
                    Set(fullKey, result, expireSeconds);
                }
                catch (RedisWrapperException ex)
                {
                    //..Write log
                }
            }

            return result;
        }

        private static T Get<T>(string fullKey)
        {
            T result;

            RedisClient redisClient = null;

            try
            {
                redisClient = GetRedisClient();

                IRedisTypedClient<T> redis = redisClient.As<T>();

                result = redis.GetValue(fullKey);

            }
            catch
            {
                result = default(T);
            }
            finally
            {
                ReleaseRedisClient(redisClient);
            }
            return result;
        }
        /// <summary>
        /// Set Entry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireSeconds">-1：not expired, 0:default expire secons</param>
        public static void SetEntry<T>(string key, T value,int expireSeconds =0)
        {
            string fullKey = GetKey<T>(key);

            Set(fullKey, value, expireSeconds);
        }

        public static void SetEntry<T>(string keyHeader,string key, T value, int expireSeconds = 0)
        {
            string fullKey = GetKey<T>(keyHeader,key);

            Set(fullKey, value, expireSeconds);
        }

        public static void SetCollection<T>(string key, ICollection<T> valueCollection, int expireSeconds = 0)
        {
            string fullKey = GetCollectionKey<T>(key);

            Set(fullKey, valueCollection, expireSeconds);
        }

        public static void SetCollection<T>(string keyHeader, string key, ICollection<T> valueCollection, int expireSeconds = 0)
        {
            string fullKey = GetCollectionKey<T>(keyHeader,key);

            Set(fullKey, valueCollection, expireSeconds);
        }
        private static void Set<T>(string fullKey, T value, int expireSeconds = 0)
        {
            RedisClient redisClient = null;

            try
            {
                redisClient = GetRedisClient();

                IRedisTypedClient<T> redis = redisClient.As<T>();

                if (expireSeconds >= 0)
                {

                    TimeSpan ts = TimeSpan.FromSeconds(expireSeconds > 0 ? expireSeconds : RedisWrapperConfiguration.GetConfig().DefaultExpireSeconds);

                    redis.SetEntry(fullKey, value, ts);
                }
                else
                {
                    redis.SetEntry(fullKey, value);
                }

            }
            catch (Exception ex)
            {
                throw new RedisWrapperException("set entry to redis failed:{0}", ex.Message);
            }
            finally
            {
                ReleaseRedisClient(redisClient);
            }
        }


        

        public static void DisposeRedisClient(RedisClient client)
        {
            client.Dispose();
        }
        
        public static bool Delete<T>(string key)
        {
            return Delete<T>(new string[] { key });
        }

        public static bool Delete<T>(string keyHeader, string key)
        {
            return Delete<T>(keyHeader,new string[] { key });
        }
        /// <summary>
        /// 删除列表
        /// </summary>
        /// <typeparam name="T">T,not Collection</typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DeleteCollection<T>(string key)
        {
            return DeleteCollection<T>(new string[] { key });
        }
        /// <summary>
        /// 删除列表
        /// </summary>
        /// <typeparam name="T">T,not Collection</typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DeleteCollection<T>(string keyHeader, string key)
        {
            return DeleteCollection<T>(keyHeader,new string[] { key });
        }

        public static bool Delete<T>(string[] keys)
        {
            for(int i=0;i<keys.Length;i++)
            {
                keys[i] = GetKey<T>(keys[i]);
            }

            return DeleteKeys<T>(keys);
        }


        public static bool Delete<T>(string keyHeader,string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = GetKey<T>(keyHeader, keys[i]);
            }

            return DeleteKeys<T>(keys);
        }

      
        public static bool DeleteCollection<T>(string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = GetCollectionKey<T>(keys[i]);
            }

            return DeleteKeys<T>(keys);
        }

        public static bool DeleteCollection<T>(string header,string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = GetCollectionKey<T>(header, keys[i]);
            }

            return DeleteKeys<T>(keys);
        }

        private static bool DeleteKeys<T>(string[] keys)
        {
            try
            {
                using (var redisClient = GetRedisClient())
                {
                    return redisClient.RemoveEntry(keys);
                }
            }
            catch (RedisException ex)
            {
                //..Write log
                return false;
            }
        }

        private static string GetClassName<T>()
        {
            return typeof(T).Name;
        }

        private static string GetKey<T>(string key)
        {
            return string.Format("{0}:{1}",
                GetClassName<T>().ToLower(),
                key);
        }

        private static string GetKey<T>(string keyHeader, string key)
        {
            return string.Format("{0}:{1}", 
                keyHeader.ToLower(),
                key);
        }

        public static string GetCollectionKey<T>(string key)
        {
            return string.Format("{0}s:{1}",
                GetClassName<T>().ToLower(),
                key);
        }

        public static string GetCollectionKey<T>(string keyHeader, string key)
        {
            return string.Format("{0}:{1}",
                keyHeader.ToLower(),
                key);
        }

        private static bool EqualsDefaultValue<T>(T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default(T));
        }

        private static RedisClient GetRedisClientFromHttpContext()
        {
            lock (HttpContext.Current.Items)
            {
                if (!HttpContext.Current.Items.Contains(clientKey))
                {
                    HttpContext.Current.Items[clientKey] = CreateRedisClient();
                }
            }
            RedisClient client = HttpContext.Current.Items[clientKey] as RedisClient;

            return client;
        }

        private static RedisClient GetRedisClient()
        {
            RedisClient client;
            switch (RedisWrapperConfiguration.GetConfig().ConnectType)
            {
                case ConnectType.HttpRequest:
                    client = GetRedisClientFromHttpContext();
                    break;
                default:
                    client = CreateRedisClient();
                    break;
            }
            return client;
        }

        private static void ReleaseRedisClient(RedisClient client)
        {
            if (client != null)
            {
                switch (RedisWrapperConfiguration.GetConfig().ConnectType)
                {
                    case ConnectType.HttpRequest: break;
                    default:
                        client.Dispose();
                        break;
                }
            }
        }

       
        //private static void Dispose

        private static RedisClient CreateRedisClient()
        {
            return new RedisClient(RedisWrapperConfiguration.GetConfig().Host,
               RedisWrapperConfiguration.GetConfig().Port);
        }
    }
}
