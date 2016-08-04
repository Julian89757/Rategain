using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration;
using RateGain.Console.Models;

namespace RateGain.Console
{
    public sealed class CsvMapProfile : CsvClassMap<RateGainEntity>
    {
        public CsvMapProfile(string fileName)
        {
            Map(m => m.CrsHotelId).ConvertUsing(x => HotelMap(x.GetField("PROPERTY_NAME")));

            Map(m => m.Channel).Name("CHANNEL");

            Map(m => m.Date).Name("CHECK_IN_DATE");

            // 有一张表格的某个记录 LengthOfStay 非法
            Map(m => m.LengthOfStay).Name("LENGTH_OF_STAY").Default(0);

            Map(m => m.Availablity).Name("AVAILABILITY_STATUS");

            Map(m => m.Rate).Name("AVERAGE_DAILY_RATE");

            Map(m => m.Currency).Name("CURRENCY");

            // 匹配 roomtype
            Map(m => m.RoomType).ConvertUsing(x => RoomtypeMap(x.GetField("PRODUCT")));

            Map(m => m.Promotion).Name("PROMOTION");

            Map(m => m.Restriction).Name("RESTRICTION");

            Map(m => m.Intax).ConvertUsing(x => x.GetField("TAX_TYPE") == "inclusive");         // Name("TAX_TYPE")

            Map(m => m.Footing).ConvertUsing(x => new Tuple<string,int>(fileName, x.Row));
        }

        private static string HotelMap(string PROPERTY_NAME)
        {
            //   使用Lunene 匹配
            return HotelNameMapping.LuceneMap(PROPERTY_NAME);
        }

        private static string RoomtypeMap(string product)
        {
            var temp = product.Split(',').ToList().FindAll(x => x.IndexOf("RM", 0, StringComparison.InvariantCultureIgnoreCase) >= 0);
            var roomtypes = temp.Select(x => x.Substring(x.IndexOf('-') + 1)).ToList();
            if (roomtypes.Any())
            {
                return roomtypes.Aggregate((x, y) => x + "," + y);
            }
            else
                return null;
        }
    }
}
