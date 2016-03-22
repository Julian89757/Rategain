using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace RateGainData.Console
{
    public sealed class CsvMapProfile : CsvClassMap<RateGainEntity>
    {
        public CsvMapProfile()
        {
            // Map(m => m.RategainHotelId).Name("PROPERTY_ID");
            // Map(m => m.PropertyName).Name("PROPERTY_NAME");

            Map(m => m.CrsHotelId).ConvertUsing(x => HotelMap(x.GetField("PROPERTY_NAME")));

            Map(m => m.Channel).Name("CHANNEL");


            Map(m => m.Date).Name("CHECK_IN_DATE");

            // 有一张表格的某个记录 LengthOfStay 非法
            Map(m => m.LengthOfStay).Name("LENGTH_OF_STAY").Default(0);

            Map(m => m.Availablity).Name("AVAILABILITY_STATUS");

            Map(m => m.Rate).Name("AVERAGE_DAILY_RATE");

            Map(m => m.Currency).Name("CURRENCY");

            //Map(m => m.PromotionFlag).Name("PROMOTION").Default(' ');
            //Map(m => m.RestrictionFlag).Name("RESTRICTION").Default(' ');

            // 匹配 roomtype
            Map(m => m.RoomType).ConvertUsing(x => RoomtypeMap(x.GetField("PRODUCT")));

            Map(m => m.Promotion).Name("PROMOTION");

            Map(m => m.Restriction).Name("RESTRICTION");

            Map(m => m.Intax).ConvertUsing(x => x.GetField("TAX_TYPE") == "inclusive");         // Name("TAX_TYPE")

            //Map(m => m.RoomDecription).Name("DESCRIPTION").Ignore();
            //Map(m => m.DateAdded).Ignore();
            //Map(m => m.DateUpdated).Ignore();
        }

        private static string HotelMap(string PROPERTY_NAME)
        {
            //  优先人工匹配的hotel name
            if (HotelNameMapping.MappDict.ContainsKey(PROPERTY_NAME))
            {
                return HotelNameMapping.MappDict[PROPERTY_NAME];
            }
            //   使用Lunene 匹配
            return new HotelNameMapping().LuceneMap(PROPERTY_NAME);
        }

        private static string RoomtypeMap(string PRODUCT)
        {
            var temp = PRODUCT.Split(',').ToList().FindAll(x => x.IndexOf("RT", 0, StringComparison.InvariantCultureIgnoreCase) < 0);
            var roomtypes = temp.Select(x => x.Substring(x.IndexOf('-') + 1));
            return roomtypes.Aggregate((x, y) => x + "," + y);
        }
    }
}
