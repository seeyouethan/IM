using OKMS.Proxy.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Service.Service
{

    public interface IOkmsProxyServiceService
    {
        /// <summary>
        /// 获取hfs文件信息
        /// </summary>
        /// <param name="dbCode"></param>
        /// <param name="fileName"></param>
        /// <param name="dbName"></param>
        /// <param name="userIp"></param>
        /// <param name="orgId"></param>
        /// <param name="hfsfilecode"></param>
        /// <param name="taskType"></param>
        /// <returns></returns>
        RetValue<FileNameHfsFileCode> GetHfsFileInfo(string dbCode, string fileName, string dbName, string userIp, string orgId, string hfsfilecode, int taskType = 0);

        /// <summary>
        /// 从下载代理中取得下载进度
        /// </summary>
        /// <param name="dbCode"></param>
        /// <param name="fileName"></param>
        /// <param name="userip"></param>
        /// <param name="orgId"></param>
        /// <param name="taskType"></param>
        /// <returns></returns>
        OKMS.Proxy.WcfService.DownloadStatus GetDownloadStatusForProxy(string dbCode, string fileName, string userip, string orgId, int taskType = 0);
    }
}
