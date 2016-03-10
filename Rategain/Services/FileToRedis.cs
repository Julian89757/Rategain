using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using log4net;
using Newtonsoft.Json;
using RateGain.Util;
using RateGainData.Console.Aspects;
using StackExchange.Redis;

namespace RateGainData.Console
{
    public class FileToRedis
    {
        public static string DirPath = ConfigurationManager.AppSettings["DownloadDir"];

        static FileToRedis()
        {
            var datePartDir = DateTime.Now.AddDays(-1).Date.ToString("yyyy-MM-dd");
            DirPath += datePartDir;
        }

        public void ToRedis()
        {
            if (Directory.Exists(DirPath))
            {
                var dir = new DirectoryInfo(DirPath);
                var filenames =
                    dir.GetFiles().Select(x => new KeyValuePair<string, string>(x.FullName, x.Name));

                var i = 0;
                filenames.ToList().ForEach(x =>
                {
                    i++;
                    GenerateRedisData(x.Key);
                });
            }
            System.Console.WriteLine(string.Format("{0}-end import", DateTime.Now));
        }

        [ImportLog]
        public static HandleResp GenerateRedisData(string fullName)
        {
            //  单csv文件数据对象
            var tempList = new List<RateGainEntity>();

            try
            {
                using (var tr = new StreamReader(fullName, Encoding.Default))
                {
                    var csv = new CsvReader(tr);
                    {
                        var mapProfile = new CsvMapProfile();
                        csv.Configuration.RegisterClassMap(mapProfile);

                        while (csv.Read())
                        {
                            try
                            {
                                var record = csv.GetRecord<RateGainEntity>();
                                DateTime outDate;
                                if (!DateTime.TryParse(record.Date, out outDate))
                                {
                                    continue;
                                }
                                if (outDate < DateTime.Now.Date || record.Availablity != "O" || record.Rate == "0" ||
                                    record.Promotion == null || record.Restriction == "Y" ||
                                    record.CrsHotelId == null || record.Channel == null || record.RoomType == "")
                                {
                                    continue;
                                }
                                tempList.Add(record);
                            }
                            catch (KeyNotFoundException)
                            {
                                System.Console.Write("RateGainEntity Id invalid ");
                            }
                        }
                        if (!tempList.Any())
                        {
                            return new HandleResp { Status = 1, EffectiveRecord = 0 };
                        }
                    }
                }

                var manager = (new RedisCacheCollection())["Db4"];
                var db = manager.GetDataBase();
                foreach (var c in tempList)
                {
                    var json = db.StringGet(c.Id);
                    List<RateGainEntity> oldList = null;
                    try
                    {
                        if (!string.IsNullOrEmpty(json))
                        {
                            oldList = JsonConvert.DeserializeObject<List<RateGainEntity>>(json);
                        }
                    }
                    catch (Exception)
                    {
                        oldList = null;
                    }
                    if (oldList != null && oldList.Count > 0)
                    {
                        var old =
                            oldList.FirstOrDefault(x => x.Channel == c.Channel && x.RoomType == c.RoomType);
                        //  更新价格
                        if (old != null)
                        {
                            old.Currency = c.Currency;
                            old.Rate = c.Rate;
                        }
                        else
                        {
                            oldList.Add(c);
                        }
                        db.StringSet(c.Id, JsonConvert.SerializeObject(oldList));
                    }
                    else
                    {
                        db.StringSet(c.Id, JsonConvert.SerializeObject(new[] { c }));
                    }
                    // 为该key设置过期时间 [ 该函数必须指定 DateTimeKind 枚举类型，不能使用默认枚举值DateTimeKind.unspecified
                    var date = c.Id.Split(':')[1];
                    var expiry = DateTime.SpecifyKind(DateTime.Parse(date).AddDays(1), DateTimeKind.Local);
                    db.KeyExpire(c.Id, expiry);
                }
                return new HandleResp() { Status = 1, EffectiveRecord = tempList.Count };
            }
            catch (IOException ex)
            {
                return new HandleResp() { Status = 0, EffectiveRecord = tempList.Count, Desc = "IO exception，Current file is already in used" };
            }
            catch (RedisConnectionException ex)
            {
                return new HandleResp() { Status = 0, EffectiveRecord = tempList.Count, Desc = "Redis server can not connect" };
            }
            catch (Exception ex)
            {
                return new HandleResp() { Status = 0, EffectiveRecord = tempList.Count, Desc = ex.Message };
            }

        }

    }
}
