using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using CsvHelper;
using RateGain.Util;

namespace RateGain.Console
{
    // 定时执行ftp下载和导入任务
    public class Program
    {
        static void Main()
        {
            LogHelper.Init();
            try
            {
                HotelNameMapping.InitMappinng();
            }
            catch
            {
                LogHelper.Write("There is no map file information", LogHelper.LogMessageType.Error);
                return;
            }

            var ftpDownLoad = new FtpDownload()
            {
                AnyFileDownLoadedOperate = FileToRedis.GenerateRedisData,
                AllFileDownLoadedOperate =  FileToRedis.ToRedis
            };
            ftpDownLoad.DownLoadList();

            HotelNameMapping.DisposeIndexDirectory();
        }
    }
}

