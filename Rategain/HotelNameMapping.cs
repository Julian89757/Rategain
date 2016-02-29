using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RateGain.Util;

namespace RateGainData.Console
{
    public class HotelNameMapping
    {
        private static readonly List<SampleDataFileRow> CmsHotelNames = new List<SampleDataFileRow>();
        private static List<string> RategainHotelNames = new List<string>();

        private static List<Tuple<string, string, string>> MapResults = new List<Tuple<string, string, string>>();

        static HotelNameMapping()
        {
            try
            {
                var cmsHotelNameBytes = Res.hotel;
                using (var stream = new MemoryStream(cmsHotelNameBytes))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var  i = 1;
                        while (true)
                        {
                            var line = reader.ReadLine();
                            if (line == null)
                                break;
                            CmsHotelNames.Add(new SampleDataFileRow
                            {
                                LineNumber = i,
                                LineText = line.Trim(',')
                            });
                            i += 1;
                        }
                    }
                }
                RategainHotelNames = Res.Rategain_hotel_ID.Trim().Split(new char[] { '\n' }).ToList();
            }
            catch (Exception ex)
            {
                LogHelper.Write("Parse hotel name json file error", LogHelper.LogMessageType.Error, ex);
            }
        }

        /// <summary>
        ///  通过 Lunene.net 得到最可能的HotelName匹配
        /// </summary>
        /// <returns></returns>
        public static void InitMappinng()
        {
            var luceneService = new LuceneService();
            luceneService.BuildIndex(CmsHotelNames);

            foreach (var c in RategainHotelNames)
            {
                var temp = luceneService.Search(c).FirstOrDefault();
                if (temp != null)
                {
                    var hotelName = temp.LineText.Split(':')[0].Trim();
                    var hotelCode = temp.LineText.Split(':')[1].Trim();
                    var mapp = Tuple.Create<string, string, string>(c, hotelName, hotelCode);
                    MapResults.Add(mapp);
                }
            }
            luceneService.CloseDirectory();
            LogHelper.Write("hotel name map completed",LogHelper.LogMessageType.Info);
        }


        // 目前人工收集的rateGain  hotelname 
        public static Dictionary<string, string> MappDict = new Dictionary<string, string>
        {
            {"Millennium Boulder","BOUMIL01"},
            {"Millennium Bostonian Hotel Bost","BOSMIL01"},
            {"Copthorne Tara Hotel London Kensington Feed","TARCOP01"},   
            {"Millennium Hotel London Mayfair","MAYMIL01"},       
            {"Grand Millennium Al Wahda","ABDGML01"},
            {"Grand Copthorne Waterfront Singapore","SINGCP01"},
            
            {"Millennium Hotel Queenstown Feed","MQTMIL01"},
            {"Copthorne Hotel Cameron Highlands Feed","CAMCOP01"},
            {"The Heritage Hotel Manila Feed","MNLMIL01"},
            {"Copthorne King's Hotel Singapore Feed","SINCOP01"},

            {"Grand Copthorne Waterfront Hotel Singapore Feed","SINGCP01"},
            {"Grand Millennium Kuala Lumpur Feed","KULGML01"},
            {"Orchard Hotel Singapore Feed","SINMIL01"},

            {"Studio M Hotel Singapore Feed","SINMHT01"},
            {"Grand Millennium Shanghai HongQiao Feed","SHAMIL01"},
            {"Grand Millennium Beijing Feed","BEIGML01"},
            {"Millennium Plaza Hotel Dubai Feed","DXBMIL03"}
            
        };

        public string LuceneMap(string propertyName)
        {
            var temp = MapResults.FirstOrDefault(x => x.Item1 == propertyName);
            if (temp == null)
                return null;
            return temp.Item3;
        }

    }
}
