using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;

namespace RateGainData.Console
{
    /// <summary>
    /// 自定义实现 ICloneable接口的待复制对象
    /// </summary>
    public class RateGainEntity:ICloneable
    {
        [JsonIgnore]
        public string Id
        {
            get
            {
                string[] strArray = { CrsHotelId, Date, LengthOfStay.ToString() };

                return strArray.Aggregate((x, y) => x + ":" + y);
            }
        }

        [JsonIgnore]
        public string CrsHotelId { get; set; }

        [JsonIgnore]
        public string Date { get; set; }

        [JsonIgnore]
        public int LengthOfStay { get; set; }

        [JsonIgnore]
        public string Availablity { get; set; }

        public string Channel { get; set; }

        public float Rate { get; set; }

        public string Currency { get; set; }

        [JsonIgnore]
        public string Promotion { get; set; }

        [JsonIgnore]
        public string Restriction { get; set; }

        public string RoomType { get; set; }

        public bool Intax { get; set; }

        // 为记录添加注脚
        public Tuple<string,int> Footing { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}

/*
 MemberwiseClone() 是浅表复制，值类型直接拷贝，引用类型只拷贝引用，实际上引用的是一个对象，
 而string类型作为引用类型，MemberwiseClone拷贝是一个特例，可以认为与值类型一样，这里的自定义拷贝就使用了这个特例。
*/
