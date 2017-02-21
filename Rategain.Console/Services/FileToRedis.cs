using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Newtonsoft.Json;
using RateGain.Util;
using System.Diagnostics;
using RateGain.Console.Models;
using StackExchange.Redis;

namespace RateGain.Console
{
    public class FileToRedis
    {
        public static string DirPath = ConfigurationManager.AppSettings["DownloadDir"];

        static FileToRedis()
        {
            var datePartDir = DateTime.Now.Date.ToString("yyyy-MM-dd");
            DirPath += datePartDir;
        }

        public static void ToRedis()
        {
            var _stopWatch = Stopwatch.StartNew();

            if (Directory.Exists(DirPath))
            {
                var dir = new DirectoryInfo(DirPath);
                var filenames = dir.GetFiles().Select(x => x.FullName).ToList();

                //  串行化
                 filenames.ForEach(x => GenerateRedisData(x));

                // 并行运算
                //ParallelLoopResult res = Parallel.ForEach(filenames, item =>
                //{
                //    GenerateRedisData(item);
                //});
                _stopWatch.Stop();
                //if(res.IsCompleted)
                //{
                //    var msg = $"Import redis take {_stopWatch.ElapsedMilliseconds} ms to execute";
                //    LogHelper.Write(msg, LogHelper.LogMessageType.Info);
                //}
            }
        }

        public static void  GenerateRedisData(string fullName)
        {
            LogHelper.Write(string.Format("{0} download completed, Now start to import redis server... ", fullName), LogHelper.LogMessageType.Info);

            //  单csv文件数据对象
            var tempList = new List<RateGainEntity>();
            using (var tr = new StreamReader(fullName, Encoding.Default))
                {
                    var tempFileName = fullName.Substring(fullName.LastIndexOf('\\') + 1);
                    var csv = new CsvReader(tr);
                    {
                        var mapProfile = new CsvMapProfile(tempFileName);
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
                                
                                Debug.Assert(csv.Row != 3288,"达到需要调试的行数");
                                
                                if (outDate < DateTime.Now.Date || record.Availablity != "O" || record.Rate == 0 ||
                                    record.Promotion == null || record.Restriction == "Y" ||
                                    record.CrsHotelId == null || record.Channel == null || string.IsNullOrEmpty(record.RoomType))
                                {
                                    continue;
                                }

                                // 这里将体现我对RateGainEntity 类实现 ICloneable接口的意义
                                foreach (var roomtype in record.RoomType.Split(','))
                                {
                                    var cloneObj = (RateGainEntity)(record.Clone());
                                    cloneObj.RoomType = roomtype;
                                    tempList.Add(cloneObj);
                                };
                            }
                            catch (KeyNotFoundException)
                            {
                                //ingore
                            }
                            catch(Exception ex)
                            {
                                LogHelper.Write(string.Format("{0} line {1} cause {2}",fullName,csv.Row,ex.Message), LogHelper.LogMessageType.Error);
                            }
                        }
                        if (!tempList.Any())
                        {
                            LogHelper.Write(string.Format("{0} has no effective record.", fullName), LogHelper.LogMessageType.Info);
                            return;
                        }
                    }
                }

            try
            {
                RemovePreData(tempList, fullName);
                ImportRedisData(tempList);
                LogHelper.Write(string.Format("{0} {1} effective record.", fullName, tempList.Count), LogHelper.LogMessageType.Info);
            }
            catch (IOException ex)
            {
                LogHelper.Write(string.Format("{0} {1} effective record.{2}", fullName, tempList.Count, ex.Message), LogHelper.LogMessageType.Error);
            }
            catch (RedisConnectionException ex)
            {
                LogHelper.Write(string.Format("{0} {1} effective record.{2}", fullName, tempList.Count, ex.Message), LogHelper.LogMessageType.Fatal);
            }
            catch (Exception ex)
            {
                LogHelper.Write(string.Format("{0} {1} effective record.{2}", fullName, tempList.Count, ex.Message), LogHelper.LogMessageType.Fatal);
            }
        }

        // 清除这一批次下 该酒店之前的所有数据
        private static void RemovePreData(List<RateGainEntity> temp, string fullname)
        {
            var timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            var fileName = fullname.Substring(fullname.LastIndexOf('\\') + 1);

            var manager = RedisManager.Dbs["Db4"];
            var db = manager.DataBase;
            // 一般情况下，一个文件内只有一个酒店
            var hotels = temp.Select(x => x.CrsHotelId).Distinct();
            foreach (var h in hotels)
            {
                /***********************第一个文件之前删除*********/
                string ke = h + ":filelog";

                if (string.IsNullOrEmpty(db.HashGet(ke, DirPath)))
                {
                    var keys = manager.GetKeys(h + ":" + @"20??-??-??:*");
                    foreach (var k in keys)
                    {
                        manager.DeleteKey(k);
                    }
                }
                // 设置最新的文件名
                db.HashSet(ke, timeStamp, fileName + "," + db.HashGet(ke, timeStamp));

                /***********************************************/
            }
        }

        private static void ImportRedisData(List<RateGainEntity> tempList)
        {
            var db = RedisManager.Dbs["Db4"].DataBase;
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
                        old.Rate = old.Rate > c.Rate ? c.Rate : old.Rate;
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
        }

    }
}
