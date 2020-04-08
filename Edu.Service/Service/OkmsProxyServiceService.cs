using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKMS.Proxy.Model;

namespace Edu.Service.Service
{

    public class OkmsProxyServiceService : IOkmsProxyServiceService
    {
        private readonly IOkmsProxyServiceRepository _okmsProxyServiceRepository =new OkmsProxyServiceRepository();

        public OkmsProxyServiceService()
        {

        }

        public RetValue<FileNameHfsFileCode> GetHfsFileInfo(string dbCode, string fileName, string dbName, string userIp, string orgId, string hfsfilecode, int taskType = 0)
        {
            return _okmsProxyServiceRepository.GetHfsFileInfo(dbCode, fileName, dbName, userIp, orgId, hfsfilecode, taskType);
        }

        public OKMS.Proxy.WcfService.DownloadStatus GetDownloadStatusForProxy(string dbCode, string fileName, string userip, string orgId, int taskType = 0)
        {
            return _okmsProxyServiceRepository.GetDownloadStatusForProxy(dbCode, fileName, userip, orgId, taskType);
        }
    }
}
