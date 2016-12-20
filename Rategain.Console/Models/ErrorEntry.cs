using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateGain.Console.Models
{
    /// <summary>
    ///单条记录的错误信息
    /// </summary>
    public class ErrorEntry
    {
        public string Field { get; set; }

        public string Msg { get; set; }
    }

    public class FileErrorRecord
    {
        public FileErrorRecord(string fullName)
        {
            FullName = fullName;
            ErrorRecord = new List<string>();
        }

        public string FullName { get; set; }

        /// <summary>
        /// 记录本文件所有的错误
        /// </summary>
        public List<string> ErrorRecord { get; set; }
    }
}
