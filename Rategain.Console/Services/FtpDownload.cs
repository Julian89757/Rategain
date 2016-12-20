using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RateGain.Util;

namespace RateGain.Console
{
    public class FtpDownload
    {
        #region 配置变量

        private static readonly string FptHost = ConfigurationManager.AppSettings["FtpUrl"];
        private static readonly string FtpUserId = ConfigurationManager.AppSettings["FtpUserId"];
        private static readonly string FtpPwd = ConfigurationManager.AppSettings["FtpPwd"];
        private static readonly string RemotePath = ConfigurationManager.AppSettings["RemotePath"];
        private static readonly string DownloadRootDir = ConfigurationManager.AppSettings["DownloadDir"];

        /// <summary>
        /// 待下载的日期文件夹
        /// </summary>
        private readonly string DatePartDir = DateTime.Now.ToString("yyyy-MM-dd");

        private readonly string TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        #endregion

        // SFTP下载客户端
        private readonly SftpOperation _sftpClient = new SftpOperation(FptHost, "22", FtpUserId, FtpPwd);

        public Action<string> AnyFileDownLoadedOperate { private get; set; }

        public Action AllFileDownLoadedOperate { private get; set; }

        public FtpDownload()
        {
        }

        public void DownLoadList()
        {
            LogHelper.Write("SFTP start check files", LogHelper.LogMessageType.Info);

            //  正则表达式匹配 \w*_2016-01-18.csv$
            var patternString = @"\w*_" + DatePartDir + ".csv" + "$";
            var dateList = _sftpClient.GetPatternFileList(RemotePath, patternString);
            Directory.CreateDirectory(DownloadRootDir + "/" + DatePartDir);
            var remainDLlist = dateList.ToArray().Where(x => !File.Exists(DownloadRootDir + DatePartDir + @"\" + x)).ToList();

            if (!remainDLlist.Any())
            {
                LogHelper.Write($"This time {TimeStamp} we do  not need download files", LogHelper.LogMessageType.Info);
                return;
            }

            LogHelper.Write($"This time {TimeStamp} we need download {remainDLlist.Count} files", LogHelper.LogMessageType.Info);

            _sftpClient.Connect();
            var tasks = new List<Task>();
            foreach (var fileName in remainDLlist)
            {
                var remotePath = RemotePath + fileName;
                var localPath = DownloadRootDir + DatePartDir + @"\" + fileName;
                tasks.Add(_sftpClient.GetAsync(remotePath, localPath, AnyFileDownLoadedOperate));
            }

            //  等待级联子任务结束
            try
            {
                Task.WaitAll(tasks.Where(x => x != null).ToArray());

                _sftpClient.Disconnect();
                if (AnyFileDownLoadedOperate != null)
                {
                    AllFileDownLoadedOperate();
                }

                LogHelper.Write("SFTP async download completed", LogHelper.LogMessageType.Info);
            }
            catch (AggregateException ex)
            {
                ex.Handle(e =>
                {
                    if (!(e is TaskCanceledException))
                        LogHelper.Write(e.Message, LogHelper.LogMessageType.Fatal, e);
                    return true;
                });
            }
            catch (Exception ex)
            {
                LogHelper.Write(ex.Message, LogHelper.LogMessageType.Error, ex);
            }
        }
    }
}
