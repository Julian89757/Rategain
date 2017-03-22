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
        private static readonly string DatePartDir = DateTime.Now.ToString("yyyy-MM-dd");

        private static readonly string TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        #endregion

        // SFTP下载客户端
        private static readonly SftpOperation _sftpClient = new SftpOperation(FptHost, "22", FtpUserId, FtpPwd);

        /// <summary>
        /// 级联任务
        /// </summary>
        public static Action<string> AnyFileDownLoadedOperate { private get; set; }

        /// <summary>
        /// 级联任务完成之后进行的操作
        /// </summary>
        public static Action AllFileDownLoadedOperate { private get; set; }


        public static async Task<int> GetDataAsync()
        {
            LogHelper.Write("SFTP start check files", LogHelper.LogMessageType.Info);

            //  正则表达式匹配 \w*_2016-01-18.csv$
            var patternString = @"\w*_" + DatePartDir + ".csv" + "$";
            var dateIEnumerable = _sftpClient.GetPatternFileList(RemotePath, patternString);
            Directory.CreateDirectory(DownloadRootDir + "/" + DatePartDir);
            var downLoadlist = dateIEnumerable.Where(x => !File.Exists(DownloadRootDir + DatePartDir + @"\" + x)).ToList();

            if (!downLoadlist.Any())
            {
                LogHelper.Write($"This time {TimeStamp} we do not need download files", LogHelper.LogMessageType.Info);
                return 0;
            }

            LogHelper.Write($"This time {TimeStamp} we need download {downLoadlist.ToList().Count()} files", LogHelper.LogMessageType.Info);

            await GetFtpDataAsync(downLoadlist);

            if (AnyFileDownLoadedOperate != null)
            {
                AllFileDownLoadedOperate();
            }
            LogHelper.Write("SFTP async download completed", LogHelper.LogMessageType.Info);
            return downLoadlist.Count;
        }

        public static async Task GetFtpDataAsync(List<string> downLoadlist)
        {
            _sftpClient.Connect();
            var tasks = new List<Task<string>>();
            foreach (var fileName in downLoadlist)
            {
                var remotePath = RemotePath + fileName;
                var localPath = DownloadRootDir + DatePartDir + @"\" + fileName;
                tasks.Add(_sftpClient.GetFtpDataAsync(remotePath, localPath, AnyFileDownLoadedOperate));
            }

            //  DoIndependentWork();

            try
            {
                // 等待级联任务结束 Task.WaitAll(tasks.Where(x => x != null).ToArray());
                await Task.WhenAll(tasks.Where(x => x != null));

                _sftpClient.Disconnect();
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
