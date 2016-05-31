using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RateGain.Util;

// PostSharp use for AOP， Log4net is just for log feature.  PNI.EA.Logging combined with two.

namespace RateGainData.Console
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
        private readonly SFTPOperation _sftpClient = new SFTPOperation(FptHost, "22", FtpUserId, FtpPwd);

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
            //  本地文件已经保存，可以每天执行多次任务，检查添加的新文件。
            var remainDLlist = dateList.ToArray().Where(x => !File.Exists(DownloadRootDir + DatePartDir + @"\" + x)).ToList();

            if (!remainDLlist.Any())
            {
                LogHelper.Write(string.Format("This time {0} we do  not need download files", TimeStamp), LogHelper.LogMessageType.Info);
                return;
            }

            LogHelper.Write(string.Format("This time {0} we need download {1} files", TimeStamp, remainDLlist.Count), LogHelper.LogMessageType.Info);

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
                if (AnyFileDownLoadedOperate == null)
                {
                    if (AllFileDownLoadedOperate != null)
                    {
                        AllFileDownLoadedOperate();
                    }
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
