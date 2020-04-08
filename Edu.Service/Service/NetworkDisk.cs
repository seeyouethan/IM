using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Cnki.NetworkDisk.Client;
using Edu.Tools;

namespace Edu.Service.Service
{
    public class NetworkDisk
    {
        public virtual dynamic UploadFile(string fileName, string fileType, long fileSize, Stream fileData)
        {
            IFileUpload client = ServiceProxyFactory<IFileUpload>.CreateProxy(ConfigHelper.GetConfigString("UploadIP"));
            UploadInMsg uploadMessage = new UploadInMsg()
            {
                BID = fileType,
                ticket = "5e8b1cm8#%",
                FileName = fileName,
                FileData = fileData,
                FileSize = fileSize
            };
            UploadRetMsg resultMessage = client.UploadFile(uploadMessage);
            return resultMessage;
        }

        public virtual MemoryStream DownLoadFile(string fileCode, string fileType, string fileExtension)
        {
            //创建下载客户端对象
            var client = ServiceProxyFactory<IFileDownload>.CreateProxy(ConfigHelper.GetConfigString("DownloadIP"));
            using (MemoryStream ms = client.Download("5e8b1cm8#%", fileType, fileCode, fileExtension))
            {
                return new MemoryStream(ms.ToArray());
            }

        }

    }
}
