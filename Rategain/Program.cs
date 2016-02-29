using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using CsvHelper;
using log4net;
using log4net.Config;
using log4net.Core;
using RateGain.Util;
using StackExchange.Redis;

namespace RateGainData.Console
{
    // 定时执行ftp下载和导入任务
    public class Program
    {
        static void Main()
        {
            LogHelper.Init();

             // 清除之前非法的keyValue
            try
            {
                var manage = new RedisCacheCollection()["DB4"];
                var keys = manage.GetKeys("*");
                foreach (var key in keys)
                {
                    var date = key.ToString().Split(':')[1];
                    DateTime outDate;
                    if (DateTime.TryParse(date, out outDate) && DateTime.SpecifyKind(outDate, DateTimeKind.Local) >= DateTime.Now.Date)
                        continue;
                    manage.DeleteKey(key);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write("Redis server do not start.", LogHelper.LogMessageType.Error, ex);
            }

            

            HotelNameMapping.InitMappinng();
            var ftpDl = new FtpDownload()
            {
                ExecFunc = FileToRedis.GenerateRedisData
            };
            ftpDl.DownLoadList();

        }
    }
}

