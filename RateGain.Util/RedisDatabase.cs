﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RateGain.Util
{
    public class RedisDatabase
    {
        public ConnectionMultiplexer Connection { get; set; }

        public IDatabase DataBase { get; }
        // Redis数据库索引
        public int Index { get; private set; }

        public RedisDatabase(ConnectionMultiplexer connection, int dbIndex)
        {
            Index = dbIndex;
            Connection = connection;
            DataBase = Connection.GetDatabase(Index);
        }

        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            try
            {
                var db = DataBase;
                var value = db.StringGet(key);
                return value;
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, LogHelper.LogMessageType.Error);
                return null;
            }
        }

        public T[] Get<T>(IEnumerable<string> keys)
        {
            var list = new List<T>();

            var db = DataBase;
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
            Insert(key, str, TimeSpan.MaxValue);
        }

        public void Insert<T>(string key, T item, TimeSpan expire)
        {

            try
            {
                var db = DataBase;
                var json = item is string ? item as string : JsonConvert.SerializeObject(item);

                if (db.StringSet(key, json, expire))
                {
                    LogHelper.Write(key + " expire timespan " + expire, LogHelper.LogMessageType.Debug);
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        /// <summary>
        /// 内部转换为TimeSpan，即将过期，推荐使用重载方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <param name="expire"></param>
        [ObsoleteAttribute]
        public void Insert<T>(string key, T item, DateTime expire)
        {
            try
            {
                var db = DataBase;
                var json = item is string ? item as string : JsonConvert.SerializeObject(item);
                var timespan = TimeSpan.FromSeconds((expire - DateTime.Now).TotalSeconds);
                if (timespan > TimeSpan.Zero)
                {
                    if (db.StringSet(key, json, timespan))
                    {
                        LogHelper.Write(key + " expire timespan " + timespan, LogHelper.LogMessageType.Debug);
                    }
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
                var db = DataBase;
                db.KeyDelete(key);
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, LogHelper.LogMessageType.Error);
            }
        }

        /// <summary>
        /// 根据正则表达式得到指定的keys
        /// </summary>
        /// <param name="pattern">请注意查看 https://redis.readthedocs.org/en/2.6/key/keys.html 具体的正则要求 </param>
        /// <returns></returns>
        public List<RedisKey> GetKeys(string pattern = null)
        {
            var endPoint = Connection.GetEndPoints()[0];
            var server = Connection.GetServer(endPoint);
            return server.Keys(Index, pattern).ToList();
        }

        public void Clear()
        {
            var conn = Connection;
            // 获取redis的服务端连接点，这里默认取得第一个
            var endPoint = conn.GetEndPoints()[0];
            var server = conn.GetServer(endPoint);
            var keys = server.Keys(Index, "*").ToArray();
            conn.GetDatabase(Index).KeyDelete(keys);

        }
    }
}
