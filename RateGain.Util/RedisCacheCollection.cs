using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RateGain.Util
{
    public class RedisCacheCollection
    {
        private static List<RedisCacheManager> Managers { get;  set; }

        static RedisCacheCollection()
        {
            Managers = new List<RedisCacheManager>
            {
                new RedisCacheManager(0, "DB0"),
                new RedisCacheManager(1, "DB1"),
                new RedisCacheManager(2, "DB2"),
                new RedisCacheManager(3, "Db3"),
                new RedisCacheManager(4, "Db4"),
                new RedisCacheManager(5, "Db5")
            };
        }

        public RedisCacheManager this[int index]
        {
            get { return Managers[0]; }
        }

        public RedisCacheManager this[string name]
        {
            get { return Managers.FirstOrDefault(x=>  string.Equals(x.Name,name,StringComparison.CurrentCultureIgnoreCase) ); }
        }
    }
}