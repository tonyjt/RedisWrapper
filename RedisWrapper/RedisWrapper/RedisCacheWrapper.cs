using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisWrapper
{
    public class RedisCacheWrapper
    {
        //private static string Header = RedisWrapperConfiguration.GetConfig().Header;

        /// <summary>
        /// Get Typed Value
        /// </summary>
        /// <typeparam name="T">generic class</typeparam>
        /// <param name="key">key </param>
        /// <returns></returns>
        public static T GetTypedValue<T>(string key)
        {
            string fullKey = GetKey<T>(key);

            T result;
            try
            {
                using (var redisClient = GetRedisClient())
                {
                    IRedisTypedClient<T> redis = redisClient.As<T>();

                    result = redis.GetValue(fullKey);
                }
            }
            catch
            {
                result = default(T);
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
            try
            {
                using (var redisClient = GetRedisClient())
                {
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
            }
            catch(Exception ex)
            {
                throw new RedisWrapperException("set entry to redis failed:{0}", ex.Message);
            }
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
        public static R GetTypedValue<P, R>(P para, Func<P,R> funGet, int expireSeconds = 0)
        {
            string key = para.ToString();

            R result = GetTypedValue<R>(key);

            if (EqualsDefaultValue<R>(result))
            {
                result = funGet(para);

                try
                {
                    SetEntry<R>(key, result, expireSeconds);
                }
                catch (RedisWrapperException ex)
                {
                    //..Write log
                }
            }

            return result;
        }
        
        public static bool Delete<T>(string key)
        {
            return Delete<T>(new string[] { key });
           
        }

        public static bool Delete<T>(string[] keys)
        {
            for(int i=0;i<keys.Length;i++)
            {
                string key = keys[i];
                key = GetKey<T>(key);
            }

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
            return string.Format("{0}:{1}:{2}",RedisWrapperConfiguration.GetConfig().Header,
                GetClassName<T>(),
                key);
        }

        private static bool EqualsDefaultValue<T>(T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default(T));
        }

        private static RedisClient GetRedisClient()
        {
            return new RedisClient(RedisWrapperConfiguration.GetConfig().Host,
                RedisWrapperConfiguration.GetConfig().Port);
        }
    }
}
