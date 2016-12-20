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
using StackExchange.Redis;

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
            try
            {
                var _path = Directory.GetCurrentDirectory() + @"\App_Data\MapResult.json";
                if (File.Exists(_path))
                {
                    using (var reader = new StreamReader(_path))
                    {
                        var text = reader.ReadToEnd();
                        CmsHotelNames = JsonConvert.DeserializeObject<List<MapEntry>>(text);
                        CmsHotelNames.RemoveAll(x => string.IsNullOrEmpty(x.HotelCode));
#if DEBUG
                        var temp = CmsHotelNames.Select(x => new KeyValuePair<string, string>(x.HotelCode, x.CmsName)).ToList();
                        var hoteljson = JsonConvert.SerializeObject(temp);
#endif         
                    }
                    if (CmsHotelNames == null)
                        throw new Exception("There is  no hotel Map information.");
                    var db = RedisManager.Dbs["Db4"].DataBase;
                    var values = CmsHotelNames.Where(x => !string.IsNullOrEmpty(x.MapName)).Select(x => x.MapName).ToArray();
                    db.SetAdd("rategain_hotels", Array.ConvertAll(values, item => (RedisValue)item));
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
            LogHelper.Write("Hotel name map completed", LogHelper.LogMessageType.Info);
        }

        /// <summary>
        ///  通过 Lunene.net 得到最可能的HotelName匹配
        /// </summary>
        /// <returns></returns>
        public static void InitMappinng()
        {
            var dataToIndex = CmsHotelNames.Select((x, index) => new SampleDataFileRow
            {
                LineNumber = index,
                LineText = x.CmsName
            });
            luceneService.BuildIndex(dataToIndex);

            // 先对新加入的rategain hotelName 做订阅处理
            var sub = RedisManager.Connection.GetSubscriber();
            sub.SubscribeAsync("rategain_hotels", (c, v) =>
            {
                //  新加入的rategain hotel name 持久化到另外的文件
                LogHelper.Write(v, LogHelper.LogMessageType.Debug);
                var mostPossible = luceneService.Search(v);
                if (mostPossible != null)
                {
                    var temp = CmsHotelNames.FirstOrDefault(x => x.CmsName == mostPossible.LineText);
                    var rategain_Hotel = new RategainHotel
                    {
                        CmsName = temp.CmsName,
                        FromFile = "",
                        HotelCode = temp.HotelCode,
                        MapName = v
                    };
                    var _path = Directory.GetCurrentDirectory() + @"\App_Data\rategain_hotels.txt";
                    // use a Serializer to serialise the object to the writer [append]
                    using (var sw = new StreamWriter(_path, true))
                    {
                        using (JsonTextWriter jw = new JsonTextWriter(sw))
                        {
                            jw.Formatting = Formatting.Indented;
                            jw.IndentChar = ' ';
                            jw.Indentation = 2;
                            JsonSerializer.Create().Serialize(jw, rategain_Hotel);
                            LogHelper.Write("new rategain hotel added.", LogHelper.LogMessageType.Debug);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 根据匹配配置文件找到 对应的hotelcode
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string LuceneMap(string propertyName)
        {
            var db = RedisManager.Dbs["Db4"].DataBase;
            try
            {
                if (db != null && !db.SetContains("rategain_hotels", propertyName))
                {
                    // 监控到有新的 hotelName加入，发布这个通知
                    db.SetAdd("rategain_hotels", propertyName);
                    db.Publish("rategain_hotels", propertyName);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, LogHelper.LogMessageType.Debug);
            }

            var temp = CmsHotelNames.FirstOrDefault(x => x.MapName == propertyName);

            return temp != null ? (temp.Enable ? temp.HotelCode : null) : null;
        }

        public static void DisposeIndexDirectory()
        {
            luceneService.DisposeIndexDirectory();
        }
    }
}
