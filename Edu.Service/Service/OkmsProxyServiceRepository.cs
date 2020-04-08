using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using OKMS.Proxy.Model;

namespace Edu.Service.Service
{

    public class OkmsProxyServiceRepository : IOkmsProxyServiceRepository
    {
        public RetValue<FileNameHfsFileCode> GetHfsFileInfo(string dbCode, string fileName, string dbName, string userIp, string orgId, string hfsfilecode, int taskType = 0)
        {
            RetValue<FileNameHfsFileCode> ret = new RetValue<FileNameHfsFileCode>();
            if (string.IsNullOrWhiteSpace(dbCode)) throw new ArgumentException("dbCode");
            try
            {
                var entity = new OKMS.Proxy.WcfService.HfsFileEntity
                {
                    DbCode = dbCode,
                    DbName = dbName,
                    FileName = fileName,
                    FileExtension = ".pdf",
                    UserIP = userIp,
                    OrgId = orgId,
                    TaskType = taskType,
                    HfsFileCode = hfsfilecode
                };

                //entity.OnlyDownloadPdf = true;
                //通过配置节点实例化通道工厂
                using (ChannelFactory<OKMS.Proxy.WcfService.IService> channel = new ChannelFactory<OKMS.Proxy.WcfService.IService>("okms.proxy"))
                {
                    var service = channel.CreateChannel();
                    ret = service.GetPdfChapterInfo(entity);
                }

            }
            catch (Exception ex)
            {
                ret.Status = -1;
                ret.Msg = ex.Message;
            }
            return ret;

        }

        public OKMS.Proxy.WcfService.DownloadStatus GetDownloadStatusForProxy(string dbCode, string fileName, string userip, string orgId, int taskType = 0)
        {
            OKMS.Proxy.WcfService.DownloadStatus downloadStatus = new OKMS.Proxy.WcfService.DownloadStatus();
            try
            {
                OKMS.Proxy.WcfService.HfsFileEntity entity = new OKMS.Proxy.WcfService.HfsFileEntity
                {
                    DbCode = dbCode,
                    FileName = fileName,
                    FileExtension = ".pdf",
                    UserIP = userip,
                    OrgId = orgId,
                    TaskType = taskType
                };

                //通过配置节点实例化通道工厂
                using (ChannelFactory<OKMS.Proxy.WcfService.IService> channel = new ChannelFactory<OKMS.Proxy.WcfService.IService>("okms.proxy"))
                {
                    var service = channel.CreateChannel();
                    downloadStatus = service.GetDownloadStatus(entity);
                }

            }
            catch (Exception ex)
            {
                downloadStatus.Status = -1;
                downloadStatus.Msg = ex.Message;
            }
            return downloadStatus;
        }

    }
}
