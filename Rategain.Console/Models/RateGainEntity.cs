using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;

namespace RateGain.Console.Models
{
    /// <summary>
    /// 自定义实现 ICloneable接口的待复制对象,同时自动收集数据错误,必须使用C#2.0属性写法
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
            set{    }
        }

        private string _CrsHotelId;
        [JsonIgnore]
        public string CrsHotelId
        {
            get { return _CrsHotelId;  }
            set
            {
                _CrsHotelId = value;
                if( value == null)
                {
                    Errors.Add(new ErrorEntry {  Field = nameof(CrsHotelId) , Msg= $"CrsHotelId is null."  });
                }
            }
        }

        private string _Date;
        [JsonIgnore]
        public string Date
        {
            get { return _Date; }
            set
            {
                _Date = value;
                DateTime outDate;
                if(!DateTime.TryParse(value,out outDate)  || outDate < DateTime.Now.Date)
                {
                    Errors.Add(new ErrorEntry {  Field= nameof(Date), Msg= $"Date is invalid." });
                }
            }
        }

        [JsonIgnore]
        public int LengthOfStay { get; set; }

        private string _Avaliablity;
        [JsonIgnore]
        public string Availablity
        {
            get { return _Avaliablity; }
            set
            {
                _Avaliablity = value;
                if (value != "O")
                    Errors.Add(new ErrorEntry { Field = nameof(Availablity), Msg = $"Availablity is invalid." });
            }
        }

        private string _Channel;
        public string Channel
        {
            get { return _Channel; }
            set
            {
                _Channel = value;
                if (value == null)
                    Errors.Add(new ErrorEntry { Field = nameof(Channel), Msg = $"Channel is null." });
            }
        }

        private float _Rate;
        public float Rate
        {
            get { return _Rate; }
            set
            {
                _Rate = value;
                if (value == 0)
                    Errors.Add(new ErrorEntry { Field = nameof(Rate), Msg = $"{ nameof(Rate)} is invalid." });
            }
        }

        public string Currency { get; set; }

        private string _Promotion;
        [JsonIgnore]
        public string Promotion
        {
            get { return _Promotion; }
            set
            {
                _Promotion = value;
                if (Promotion == null)
                    Errors.Add(new ErrorEntry { Field = nameof(Promotion), Msg = $"{ nameof(Promotion)} is null." });
            }
        }

        [JsonIgnore]
        public string Restriction { get; set; }

        private string _RoomType;
        public string RoomType
        {
            get { return _RoomType; }
            set
            {
                _RoomType = value;
                if (string.IsNullOrEmpty(value))
                    Errors.Add(new ErrorEntry { Field = nameof(RoomType), Msg = $"RoomType is null or empty." });
            }
        }

        public bool Intax { get; set; }

        // 为记录添加注脚
        public Tuple<string,int> Footing { get; set; }

        // 单条记录中的所有错误字段
        private List<ErrorEntry> Errors = new List<ErrorEntry> { };
        public string StrError()
        {
            if(Errors!= null && Errors.Any() )
            {
                return JsonConvert.SerializeObject(new { record = $"line:{Footing.Item2}", detail = Errors });
            }
            return string.Empty;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}

/*
 MemberwiseClone() 是浅表复制，值类型直接拷贝，引用类型只拷贝引用，实际上引用的是一个对象，
 而string类型作为引用类型，MemberwiseClone拷贝是一个特例，可以认为与值类型一样，这里的自定义拷贝就使用了这个特例。
*/
