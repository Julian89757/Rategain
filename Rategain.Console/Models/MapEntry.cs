using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateGain.Console.Models
{
    public class MapEntry
    {
        public string HotelCode { get; set; }

        public string CmsName { get; set; }

        public string MapName { get; set; }

        [JsonIgnore]
        public float Score { get; set; }

        public bool Enable { get; set; } = true;

        public bool Passed { get; set; } = true;


    }
}
