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
    public class RateGainEntity
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

        public string Rate { get; set; }

        public string Currency { get; set; }

        [JsonIgnore]
        public string Promotion { get; set; }

        [JsonIgnore]
        public string Restriction { get; set; }

        public string RoomType { get; set; }

        public bool Intax { get; set; }
    }
}
