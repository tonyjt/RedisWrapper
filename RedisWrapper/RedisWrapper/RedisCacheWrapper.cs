﻿using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Generic;

namespace RedisWrapper
{
    public class RedisCacheWrapper
    {
        //private static string Header = RedisWrapperConfiguration.GetConfig().Header;
        public static string clientKey = "redisclient";

        #region Entry
        /// <summary>
        /// Get Typed Value
        /// </summary>
        /// <typeparam name="T">generic class</typeparam>
        /// <param name="key">key </param>
        /// <returns></returns>
        public static T GetTypedValue<T>(string key, RedisClient client = null)
        {
            string fullKey = GetKey<T>(key);

            return Get<T>(fullKey,client);
        }

        public static T GetTypedValue<T>(string keyHeader, string key, RedisClient client = null)
        {
            return Get<T>(GetKey<T>(keyHeader, key), client);
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
        public static R GetTypedValue<P, R>(P para, Func<P, R> funGet, int expireSeconds = 0, RedisClient client = null)
        {
            string key = GetKey<R>(para.ToString());

            return Get<P, R>(key, para, funGet, expireSeconds,client);

        }

        public static R GetTypedValue<P, R>(string keyHeader, P para, Func<P, R> funGet, int expireSeconds = 0,RedisClient client = null )
        {
            string key = GetKey<R>(keyHeader,para.ToString());

            return Get<P, R>(key, para, funGet, expireSeconds, client);
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
        private static R Get<P, R>(string fullKey, P para, Func<P, R> funGet, int expireSeconds, RedisClient client)
        {
            R result = Get<R>(fullKey, client);

            if (EqualsDefaultValue<R>(result))
            {
                result = funGet(para);

                Set<R>(fullKey, result, expireSeconds, client);
            }

            return result;
        }

        private static T Get<T>(string fullKey, RedisClient client)
        {
            T result;

            RedisClient redisClient = null;

            try
            {
                redisClient = client == null? GetRedisClient():client;

                IRedisTypedClient<T> redis = redisClient.As<T>();

                result = redis.GetValue(fullKey);

            }
            catch
            {
                result = default(T);
            }
            finally
            {
                if(client == null)  ReleaseRedisClient(redisClient);
            }
            return result;
        }
        /// <summary>
        /// Set Entry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireSeconds">-1：not expired, 0:default expire seconds</param>
        public static void SetEntry<T>(string key, T value, int expireSeconds = 0, RedisClient client = null)
        {
            string fullKey = GetKey<T>(key);

            Set(fullKey, value, expireSeconds, client);
        }

        public static void SetEntry<T>(string keyHeader, string key, T value, int expireSeconds = 0, RedisClient client = null)
        {
            string fullKey = GetKey<T>(keyHeader, key);

            Set(fullKey, value, expireSeconds,client);
        }

        private static void Set<T>(string fullKey, T value, int expireSeconds,RedisClient client)
        {
            if (object.Equals(value, default(T))) return;

            RedisClient redisClient = null;

            try
            {
                redisClient = client== null?GetRedisClient():client;

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
                if(client == null)   ReleaseRedisClient(redisClient);
            }
        }

        public static bool Delete<T>(string key, RedisClient client = null)
        {
            return Delete<T>(new string[] { key },client);
        }

        public static bool Delete<T>(string keyHeader, string key, RedisClient client = null)
        {
            return Delete<T>(keyHeader, new string[] { key },client);
        }

        public static bool Delete<T>(string[] keys, RedisClient client = null)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = GetKey<T>(keys[i]);
            }

            return DeleteKeys<T>(keys,client);
        }


        public static bool Delete<T>(string keyHeader, string[] keys, RedisClient client = null)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = GetKey<T>(keyHeader, keys[i]);
            }

            return DeleteKeys<T>(keys, client);
        }


        #endregion

        #region list
        /// <summary>
        /// 批量获取
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="key">通配符（*代表不限，?代表某1位不限）</param>
        /// <returns></returns>
        public static IList<T> GetTypedValueList<T>(string key, RedisClient client = null)
        {
            return GetTypedValueList<T>(new List<string> { key },client);
        }

        public static IList<T> GetTypedValueList<T>(List<string> keys,RedisClient client = null)
        {
            keys = GetListKey<T>(keys).ToList();

            return GetList<T>(keys,client);
        }

        public static IList<T> GetTypedValueList<T>(string keyHeader, string key, RedisClient client = null)
        {
            return GetTypedValueList<T>(keyHeader, new List<string> { key },client);
        }

        public static IList<T> GetTypedValueList<T>(string keyHeader, List<string> keys, RedisClient client = null)
        {
            keys = GetListKey<T>(keyHeader, keys).ToList();

            return GetList<T>(keys, client);
        }

        //public static IList<R> GetTypedValueList<P, R>(P para, Func<P, IList<R>> funGet, int expireSeconds = 0)
        //{
        //    string key = GetKey<P>(para.ToString());

        //    return GetList<P, R>(key, para, funGet, expireSeconds);
        //}

        //public static IList<R> GetTypedValueList<P, R>(string keyHeader,P para, Func<P, IList<R>> funGet, int expireSeconds = 0)
        //{
        //    string key = GetKey<P>(keyHeader,para.ToString());

        //    return GetList<P, R>(key, para, funGet, expireSeconds);
        //}

        //public static IList<R> GetList<P, R>(string fullKey, P para, Func<P, IList<R>> funGet, int expireSeconds = 0)
        //{

        //    IList<R> result = Get<IList<R>>(fullKey);

        //    if (EqualsDefaultValue<IList<R>>(result))
        //    {
        //        result = funGet(para);

        //        try
        //        {
        //            Set(fullKey, result, expireSeconds);
        //        }
        //        catch (RedisWrapperException ex)
        //        {
        //            //..Write log
        //        }
        //    }

        //    return result;
        //}


        public static void SetList<T>(IList<string> keys, IList<T> valueList, int expireSeconds = 0, RedisClient client = null)
        {
            keys = GetListKey<T>(keys);

            Set(keys, valueList.ToList(), expireSeconds, client);
        }

        public static void SetList<T>(string keyHeader, IList<string> keys, IList<T> valueList, int expireSeconds = 0, RedisClient client = null)
        {
            keys = GetListKey<T>(keyHeader, keys);

            Set(keys, valueList.ToList(), expireSeconds,client);
        }



        


        

      

        /// <summary>
        /// 删除列表
        /// </summary>
        /// <typeparam name="T">T,not List</typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DeleteList<T>(string key, RedisClient client = null)
        {
            return DeleteList<T>(new string[] { key },client);
        }
        /// <summary>
        /// 删除列表
        /// </summary>
        /// <typeparam name="T">T,not List</typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DeleteList<T>(string keyHeader, string key, RedisClient client = null)
        {
            return DeleteList<T>(keyHeader, new string[] { key }, client);
        }



        public static bool DeleteList<T>(string[] keys, RedisClient client = null)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = GetListKey<T>(keys[i]);
            }

            return DeleteKeys<T>(keys, client);
        }

        public static bool DeleteList<T>(string header, string[] keys, RedisClient client = null)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = GetListKey<T>(header, keys[i]);
            }

            return DeleteKeys<T>(keys,client);
        }

        private static IList<T> GetList<T>(List<string> fullKeys, RedisClient client)
        {
            IList<T> result;

            RedisClient redisClient = null;

            try
            {
                redisClient = client == null?GetRedisClient():client;

                IRedisTypedClient<T> redis = redisClient.As<T>();

                result = redis.GetValues(fullKeys);

            }
            catch
            {
                result = default(IList<T>);
            }
            finally
            {
                if(client == null)ReleaseRedisClient(redisClient);
            }
            return result;
        }

        private static void Set<T>(IList<string> fullKeys, List<T> values, int expireSeconds,RedisClient client)
        {
            if (values == null) return;

            RedisClient redisClient = null;

            try
            {
                redisClient = client == null?GetRedisClient():client;

                IRedisTypedClient<T> redis = redisClient.As<T>();

                if (expireSeconds >= 0)
                {

                    TimeSpan ts = TimeSpan.FromSeconds(expireSeconds > 0 ? expireSeconds : RedisWrapperConfiguration.GetConfig().DefaultExpireSeconds);
                    for(int i=0;i<values.Count&&i<fullKeys.Count;i++)
                    {
                        redis.SetEntry(fullKeys[i], values[i], ts);
                    }

                }
                else
                {
                    for (int i = 0; i < values.Count && i < fullKeys.Count; i++)
                    {
                        redis.SetEntry(fullKeys[i], values[i]);
                    }
                }

            }
            catch (Exception ex)
            {
                throw new RedisWrapperException("set entry to redis failed:{0}", ex.Message);
            }
            finally
            {
                if(client == null)ReleaseRedisClient(redisClient);
            }
        }


        private static IList<string> GetListKey<T>(IList<string> keys)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                keys[i] = GetKey<T>(keys[i]);
            }

            return keys;
        }

        private static IList<string> GetListKey<T>(string keyHeader, IList<string> keys)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                keys[i] = GetKey<T>(keyHeader,keys[i]);
            }

            return keys;
        }
        #endregion
        private static bool DeleteKeys<T>(string[] keys,RedisClient client)
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

        public static string GetListKey<T>(string key)
        {
            return string.Format("{0}s:{1}",
                GetClassName<T>().ToLower(),
                key);
        }

        public static string GetListKey<T>(string keyHeader, string key)
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
                    if (HttpContext.Current != null)
                    {
                        client = GetRedisClientFromHttpContext();
                        break;
                    }
                    else
                    {
                        goto default;
                    }
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

        public static RedisClient CreateRedisClient()
        {
            string password = RedisWrapperConfiguration.GetConfig().Password;


            if (!string.IsNullOrWhiteSpace(password))
            {

                return new RedisClient(RedisWrapperConfiguration.GetConfig().Host,
                   RedisWrapperConfiguration.GetConfig().Port, password);
            }
            else
            {

                return new RedisClient(RedisWrapperConfiguration.GetConfig().Host,
                   RedisWrapperConfiguration.GetConfig().Port);
            }
        }

        public static void DisposeRedisClient(RedisClient client)
        {
            client.Dispose();
        }
        
      
    }
}
