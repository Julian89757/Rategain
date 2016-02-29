using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RateGain.Util
{
    public class RedisCacheManager
    {
        #region static
        public static RedisCacheManager Website
        {
            get { return Managers[0]; }
        }

        public static RedisCacheManager Viator
        {
            get { return Managers[1]; }
        }

        public static RedisCacheManager Windsurfer
        {
            get { return Managers[2]; }
        }

        public static RedisCacheManager HeartBeat
        {
            get { return Managers[3]; }
        }

        public static RedisCacheManager RateGainData
        {
            get { return Managers[4]; }
        }

        public static RedisCacheManager Session
        {
            get { return Managers[5]; }
        }

        private static readonly ConfigurationOptions Opt;

        public static List<RedisCacheManager> Managers { get; private set; }

        static RedisCacheManager()
        {
            Opt = ConfigurationOptions.Parse(ConfigurationManager.ConnectionStrings["redis"].ConnectionString);
            Managers = new List<RedisCacheManager>
            {
                new RedisCacheManager(0, "Website"),
                new RedisCacheManager(1, "Viator"),
                new RedisCacheManager(2, "Windsurfer"),
                new RedisCacheManager(3, "HeartBeat"),
                new RedisCacheManager(4, "RateGainData"),
                new RedisCacheManager(5, "Session")
            };
        }
        #endregion

        public int Index { get; private set; }

        public string Name { get; private set; }

        private RedisCacheManager(int dbIndex, string name)
        {
            Index = dbIndex;
            Name = name;
        }

        public ConnectionMultiplexer Connection
        {
            get
            {
                return ConnectionMultiplexer.Connect(Opt);
            }
        }

        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            try
            {
                using (var conn = ConnectionMultiplexer.Connect(Opt))
                {
                    var db = conn.GetDatabase(Index);
                    var value = db.StringGet(key);
                    return value;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message,LogHelper.LogMessageType.Fatal);
                return null;
            }
        }

        public T Get<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }
            try
            {
                var json = Get(key);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, LogHelper.LogMessageType.Fatal);
                return default(T);
            }
            return null;
        }

        public T[] Get<T>(string[] keys)
        {
            var list = new List<T>();
            using (var conn = ConnectionMultiplexer.Connect(Opt))
            {
                var db = conn.GetDatabase(Index);
                foreach (var key in keys)
                {
                    try
                    {
                        var value = db.StringGet(key);
                        var obj = JsonConvert.DeserializeObject<T>(value);
                        list.Add(obj);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Write(ex.Message, LogHelper.LogMessageType.Fatal);
                    }
                }
            }
            return list.ToArray();
        }

        public void Insert<T>(string key, T item)
        {
            if (string.IsNullOrEmpty(key) || item == null)
            {
                return;
            }
            var str = item is string
                ? item as string
                : JsonConvert.SerializeObject(item);
            Insert(key, str, DateTime.MaxValue);
        }

        public void Insert<T>(string key, T item, DateTime expire)
        {
            try
            {
                using (var conn = ConnectionMultiplexer.Connect(Opt))
                {
                    var db = conn.GetDatabase(Index);
                    var json = item is string ? item as string : JsonConvert.SerializeObject(item);
                    db.StringSet(key, json);
                    // 必须指定是Utc时间还是本地时间
                    expire = DateTime.SpecifyKind(expire, DateTimeKind.Local);
                    db.KeyExpire(key, expire);
                    conn.Close();
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public void DeleteKey(string key)
        {
            try
            {
                using (var conn = ConnectionMultiplexer.Connect(Opt))
                {
                    var db = conn.GetDatabase(Index);
                    db.KeyDelete(key);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, LogHelper.LogMessageType.Fatal);
            }
        }

        public List<RedisKey> GetKeys(string pattern = null)
        {
            using (var conn = ConnectionMultiplexer.Connect(Opt))
            {
                var endPoint = conn.GetEndPoints()[0];
                var server = conn.GetServer(endPoint);
                return server.Keys(Index, pattern).ToList();
            }
        }

        public void Clear()
        {
            using (var conn = ConnectionMultiplexer.Connect(Opt))
            {
                var endPoint = conn.GetEndPoints()[0];
                var server = conn.GetServer(endPoint);
                var keys = server.Keys(Index, "*").ToArray();
                conn.GetDatabase(Index).KeyDelete(keys);
            }
        }
    }
}