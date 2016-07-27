using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using RateGain.Util;
using Renci.SshNet;

namespace RateGain.Console
{ 
    public class SftpOperation
    {
        #region 字段或属性

        private SftpClient sftp;

        public bool Connected { get { return sftp.IsConnected; } }

        #endregion

 
        public SftpOperation(string ip, string port, string user, string pwd)
        {
            sftp = new SftpClient(ip, Int32.Parse(port), user, pwd);
        }
     
        public bool Connect()
        {
            try
            {
                if (!Connected)
                {
                    sftp.Connect();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("连接SFTP失败，原因：{0}", ex.Message));
            }
        }
  
        public void Disconnect()
        {
            try
            {
                if (sftp != null && Connected)
                {
                    sftp.Disconnect();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("断开SFTP失败，原因：{0}", ex.Message));
            }
        }

        
        /// <summary>  
        /// SFTP上传文件  
        /// </summary>  
        /// <param name="localPath">本地路径</param>  
        /// <param name="remotePath">远程路径</param>  
        public void Put(string localPath, string remotePath)
        {
            try
            {
                using (var file = File.OpenRead(localPath))
                {
                    Connect();
                    sftp.UploadFile(file, remotePath);
                    Disconnect();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SFTP文件上传失败，原因：{0}", ex.Message));
            }
        }
        

        /// <summary>  
        /// SFTP获取文件  
        /// </summary>  
        /// <param name="remotePath">远程路径</param>  
        /// <param name="localPath">本地路径</param>  
        public void Get(string remotePath, string localPath)
        {
            try
            {
                Connect();
                var byt = sftp.ReadAllBytes(remotePath);
                Disconnect();
                File.WriteAllBytes(localPath, byt);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SFTP文件获取失败，原因：{0}", ex.Message));
            }
        }

        public  Task<string> GetAsync(string source, string destination)
        {
            // 将在线程池上运行的指定工作队列， 并返回该工作句柄
            return Task.Run(
                () =>
            {
                using (var saveFile = File.OpenWrite(destination))
                {
                    sftp.DownloadFile(source, saveFile);
                    return destination;
                }
            });
        }


        // 这里是级联任务
        public  Task<string> GetAsync(string remotePath, string localPath, Action<string> cbFunc)
        {
            
            // 这样是不是更合理一点
            var task= Task.Factory.StartNew(() =>
            {
                using (var saveFile = File.OpenWrite(localPath))
                {
                    sftp.DownloadFile(remotePath, saveFile);
                    return localPath;
                }
            });

            Task<string> computeTask = null;
            //  创建延续任务
            if (cbFunc != null)
            {
                computeTask = task.ContinueWith(x =>
                {
                    cbFunc(x.Result);
                    return x.Result;
                },TaskContinuationOptions.NotOnFaulted);

                // 如果指定任务的延续任务选项没有得到满足，延续任务将不会被scheduled，而会被cancled，这将引起 TaskCanceledException异常：用于告知任务取消的异常
            }
            return computeTask;
        }

        /// <summary>  
        /// 删除SFTP文件   
        /// </summary>  
        /// <param name="remoteFile">远程路径</param>  
        public void Delete(string remoteFile)
        {
            try
            {
                Connect();
                sftp.Delete(remoteFile);
                Disconnect();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SFTP文件删除失败，原因：{0}", ex.Message));
            }
        }

        /// <summary>  
        /// 获取SFTP文件列表  
        /// </summary>  
        /// <param name="remotePath">远程目录</param>  
        /// <param name="fileSuffix">文件后缀</param>  
        /// <returns></returns>  
        public ArrayList GetFileList(string remotePath, string fileSuffix)
        {
            try
            {
                Connect();
                var files = sftp.ListDirectory(remotePath);
                Disconnect();
                var objList = new ArrayList();
                foreach (var file in files)
                {
                    string name = file.Name;
                    if (name.Length > (fileSuffix.Length + 1) && fileSuffix == name.Substring(name.Length - fileSuffix.Length))
                    {
                        objList.Add(name);
                    }
                }
                return objList;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SFTP文件列表获取失败，原因：{0}", ex.Message));
            }
        }

        /// <summary>
        /// 根据正则获取制定文件
        /// </summary>
        /// <param name="remotePath">远程路径</param>
        /// <param name="patternString">正则表达式</param>
        /// <returns></returns>
        public ArrayList GetPatternFileList(string remotePath, string patternString)
        {
            try
            {
                Connect();
                var files = sftp.ListDirectory(remotePath);
                Disconnect();
                var objList = new ArrayList();
                foreach (var file in files)
                {
                    string name = file.Name;
                    if (name.IsCustomerRegex(new Regex(patternString)))
                    {
                        objList.Add(name);
                    }
                }
                return objList;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SFTP文件列表获取指定文件失败，原因：{0}", ex.Message));
            }
        }

        /// <summary>  
        /// 移动SFTP文件  
        /// </summary>  
        /// <param name="oldRemotePath">旧远程路径</param>  
        /// <param name="newRemotePath">新远程路径</param>  
        public void Move(string oldRemotePath, string newRemotePath)
        {
            try
            {
                Connect();
                sftp.RenameFile(oldRemotePath, newRemotePath);
                Disconnect();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SFTP文件移动失败，原因：{0}", ex.Message));
            }
        }

    }

}