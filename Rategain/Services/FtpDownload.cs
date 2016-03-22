using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RateGain.Util;
using RateGainData.Console.Aspects;

// PostSharp use for AOP， Log4net is just for log feature
namespace RateGainData.Console
{
    public class FtpDownload
    {
        #region 配置变量
        public static readonly string FptHost = ConfigurationManager.AppSettings["FtpUrl"];

        public static readonly string FtpUserId = ConfigurationManager.AppSettings["FtpUserId"];

        public static readonly string FtpPwd = ConfigurationManager.AppSettings["FtpPwd"];

        public static readonly string DownloadDir = ConfigurationManager.AppSettings["DownloadDir"];

        #endregion

        // 下载日期文件夹
        private string DatePartDir { get; set; }

        // SFTP下载客户端
        private SFTPOperation SftpClient { get; set; }

        public Func<string, HandleResp> ExecFunc { private get; set; }

        public FtpDownload()
        {
            DatePartDir = DateTime.Now.Date.ToString("yyyy-MM-dd");
            SftpClient = new SFTPOperation(FptHost, "22", FtpUserId, FtpPwd);
        }

        [Log(UserCaseName = "SFTP")]
        public void DownLoadList()
        {
            //  正则表达式匹配 \w*_2016-01-18.csv$
            var patternString = @"\w*_" + DatePartDir + ".csv" + "$";
            var dateList = SftpClient.GetPatternFileList("/", patternString);
            Directory.CreateDirectory(DownloadDir + "/" + DatePartDir);

            var  remainDLlist = dateList.ToArray().Where(x => !File.Exists(DownloadDir + DatePartDir + @"\" + x)).ToList();

            if (!remainDLlist.Any())
            {
                return;
            }

            LogHelper.Write(string.Format("{0} need download {1} files", DatePartDir, remainDLlist.Count()), LogHelper.LogMessageType.Info);

            SftpClient.Connect();
            var tasks = new List<Task>();
            foreach (var ii in remainDLlist)
            {
                var remotePath = "/" + ii;
                var localPath = DownloadDir + DatePartDir + @"\" + ii;
                tasks.Add(SftpClient.GetAsync(remotePath, localPath, ExecFunc));
            }

            //  等待级联子任务结束
            Task.WaitAll(tasks.ToArray());
            SftpClient.Disconnect();
        }

        //[DownLoadLog]
        //private void CbFunc(IAsyncResult result)
        //{
        //    var destination = (string)result.AsyncState;
        //    {
        //        if (ExecFunc != null)
        //        {
        //            ExecFunc(destination);
        //        }
        //    }
        //}

    }
}
