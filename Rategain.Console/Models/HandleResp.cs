using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateGain.Console.Models
{
    public class HandleResp
    {
        public int Status { get; set; }
        public string Desc { get; set; }
        public int EffectiveRecord { get; set; }
    }
}
