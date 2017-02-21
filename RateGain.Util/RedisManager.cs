using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RateGain.Util
{
    public class RedisManager
    {
        private static ConnectionMultiplexer _instance = null;
        private static object _syncObject = new object();
        public static ConnectionMultiplexer Connection
        {
            get
            {
                if( null == _instance)
                {
                    lock(_syncObject)
                    {
                        if( null  == _instance)
                        {
                            _instance = ConnectionMultiplexer.Connect(Opt);
                        }
                    }
                }
                return _instance; 
            }
        }
        private static readonly ConfigurationOptions Opt = ConfigurationOptions.Parse(ConfigurationManager.ConnectionStrings["redis"].ConnectionString);

        /// <summary>
        /// redis used as database
        /// </summary>
        public static RedisDbs Dbs
        {
            get { return new RedisDbs(Connection); }
        }
    }

    public class RedisDbs
    {

        Dictionary<string, RedisDatabase> Dbs = new Dictionary<string, RedisDatabase>();

        public RedisDbs(ConnectionMultiplexer Connection)
        {
            Dbs = new Dictionary<string, RedisDatabase>
            {
                // C# 4.0 命名参数
                ["Db0"] = new RedisDatabase(dbIndex :0,connection : Connection),
                ["Db1"] = new RedisDatabase(dbIndex: 1, connection: Connection),
                ["Db2"] = new RedisDatabase(dbIndex: 2, connection: Connection),
                ["Db3"] = new RedisDatabase(dbIndex: 3, connection: Connection),
                ["Db4"] = new RedisDatabase(dbIndex: 4, connection: Connection),
            };
        }

        public RedisDatabase this[string key]
        {
            get
            {
                if (Dbs.ContainsKey(key))
                    return Dbs[key];
                else
                    throw new Exception( $"{key} is not  exist.");
            }
        }
    }
}


/*
 ConnectionMultiplexer 是核心对象，隐藏了分布式多点服务器的细节，
 由于这个对象做了很多，在调用者中被设计成共享的和可回收的对象。
     不要每次调用的时候创建这个对象，使用这个对象是线程安全的。
     
     
     */