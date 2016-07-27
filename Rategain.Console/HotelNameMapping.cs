using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RateGain.Console.Models;
using RateGain.Util;

namespace RateGain.Console
{
    public class HotelNameMapping
    {
        /// <summary>
        /// 匹配列表
        /// </summary>
        private static readonly List<MapEntry> CmsHotelNames = new List<MapEntry>();

        public static ILuceneService luceneService = new LuceneService();

        static HotelNameMapping()
        {
            // 每次都从本地文件加载匹配信息
            var manager = (new RedisCacheCollection())["Db4"];
            var db = manager.GetDataBase();
            if (db != null)
            {
                try
                {
                    var _path = Directory.GetCurrentDirectory() + @"\App_Data\MapResult.json";
                    if (File.Exists(_path))
                    {
                        using (var reader = new StreamReader(_path))
                        {
                            var text = reader.ReadToEnd();
                            CmsHotelNames = JsonConvert.DeserializeObject<List<MapEntry>>(text);
                        }
                    }
                    else
                    {
                        throw new Exception("There is  no hotel Map information. ");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Write("Parse hotel name json file error", LogHelper.LogMessageType.Error, ex);
                    throw ex;
                }
            }

            LogHelper.Write("hotel name map completed", LogHelper.LogMessageType.Info);
        }

        /// <summary>
        ///  通过 Lunene.net 得到最可能的HotelName匹配
        /// </summary>
        /// <returns></returns>
        public static void InitMappinng()
        {
            // 对数据源建立搜索索引
            luceneService.BuildIndex(CmsHotelNames.Select(x => new SampleDataFileRow
            {
                LineText = x.CmsName
            }).ToList());

            // 对新加入的rategain hotelName 做订阅处理
            var subscriber = RedisCacheManager.Connection.GetSubscriber();
            subscriber.Subscribe("rategain_hotels", (sender, arg) =>
            {
                LogHelper.Write(arg, LogHelper.LogMessageType.Debug);
                var mostPossible = luceneService.Search(arg);
                if (mostPossible != null)
                {
                    var temp = CmsHotelNames.FirstOrDefault(x => x.CmsName == mostPossible.LineText);
                    temp.MapName = arg;
                    temp.Passed = false;

                    var _path = Directory.GetCurrentDirectory() + @"\App_Data\MapResult.json";
                    if (File.Exists(_path))
                    {
                        using (var writer = new StreamWriter(_path,false))
                        {
                            writer.Write(JsonConvert.SerializeObject(CmsHotelNames));
                            writer.Flush();
                        }
                    }
                    else
                    {
                        throw new Exception("There is  no hotel Map information. ");
                    }
                }
            });
        }

        /// <summary>
        /// 根据匹配配置文件找到 对应的hotelcode
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string LuceneMap(string propertyName)
        {
            var manager = (new RedisCacheCollection())["Db4"];
            var db = manager.GetDataBase();
            if (db != null && !db.SetContains("rategain_hotels", propertyName))
            {
                db.SetAdd("rategain_hotels", propertyName);
                db.Publish("rategain_hotels", propertyName);
            }

            var temp = CmsHotelNames.FirstOrDefault(x => x.MapName == propertyName);
            if (temp == null)
            {
                return null;
            }
            else
            {
                return temp.Enable ? temp.HotelCode : null;
            }
        }

        public static void DisposeIndexDirectory()
        {
            luceneService.DisposeIndexDirectory();
        }
    }
}
