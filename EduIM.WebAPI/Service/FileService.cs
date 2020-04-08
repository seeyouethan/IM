using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Helpers;
using Cnki.NetworkDisk.Client;
using Edu.Models.Models;
using Edu.Tools;
using MassTransit;
using OKMS.FragmentService.WcfService.Fragment;
using System.ServiceModel;

namespace EduIM.WebAPI.Service
{
    public static class FileService
    {

        private static string DownloadType = "ZK-700";
        public static JsonResultCustomer Upload(HttpPostedFileBase file, string uploadFileType = "ZK-700")
        {
            var fileCode="";
            var jsonresult=new JsonResultCustomer();
            var fileName = Path.GetFileName(file.FileName);

            var result = UploadFile(fileName, uploadFileType, file.InputStream.Length, file.InputStream);
            fileCode = ((UploadRetMsg)result).FileCode;
            if (string.IsNullOrEmpty(fileCode))
            {
                jsonresult.Success = false;
                jsonresult.Content = "";
                jsonresult.Error = "上传文件失败";
                return jsonresult;
            }
            jsonresult.Success = true;
            jsonresult.Content = fileCode;
            jsonresult.Error = "";
            jsonresult.Message = "上传文件成功";
            return jsonresult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">带后缀名的filename</param>
        /// <param name="uploadFileType"></param>
        /// <param name="fileSize"></param>
        /// <param name="fileData"></param>
        /// <returns></returns>
        public static JsonResultCustomer UploadImageThumbnail(string fileName, string uploadFileType,long  fileSize  ,Stream fileData)
        {
            uploadFileType = "ZK-700";
            var fileCode = "";
            var jsonresult = new JsonResultCustomer();

            var result = UploadFile(fileName, uploadFileType, fileSize, fileData);
            fileCode = ((UploadRetMsg)result).FileCode;
            if (string.IsNullOrEmpty(fileCode))
            {
                jsonresult.Success = false;
                jsonresult.Content = "";
                jsonresult.Error = "上传文件失败";
                return jsonresult;
            }
            jsonresult.Success = true;
            jsonresult.Content = fileCode;
            jsonresult.Error = "";
            jsonresult.Message = "上传文件成功";
            return jsonresult;
        }

        public static JsonResultCustomer Download(string fileCode)
        {
            var jsonresult = new JsonResultCustomer();
            var buffer = DownloadFileFromNetDisk(DownloadType, fileCode);
            if (buffer == null || buffer.Length == 0)
            {
                jsonresult.Success = false;
                jsonresult.Error = "文件不存在";
                return jsonresult; //"文件不存在!"
            }
            jsonresult.Success = true;
            jsonresult.Content = buffer;
            return jsonresult;
        }

        private static byte[] DownloadFileFromNetDisk(string downFileType, string fileCode)
        {
            MemoryStream ms = null;
            try
            {
                var filetype = Path.GetExtension(fileCode);
                var filecode = Path.GetFileName(fileCode);
                if (filecode == string.Empty || filetype == string.Empty)
                {
                    return null;
                }
                //下载
                var address = ConfigHelper.GetConfigString("DownloadIP");
                var client = ServiceProxyFactory<IFileDownload>.CreateProxy(address);
                ms = client.DownloadFile(fileCode);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (ms != null)
                    ms.Close();
            }

            return ms.ToArray();
        }

        private static UploadRetMsg UploadFile(string fileName, string fileType, long fileSize, Stream fileData)
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

        /// <summary>
        /// 根据md5值从hfs上获取对应的filecode
        /// </summary>
        /// <param name="md5"></param>
        /// <returns></returns>
        public static string GetFileCodeByMD5(string md5)
        {
            var address = ConfigHelper.GetConfigString("UploadIP");
            var client = ServiceProxyFactory<IFileUpload>.CreateProxy(address);
            return client.GetFileCodeByMD5(md5);
        }


        public static bool FileConvert(string fileCode, out string message)
        {
            string fileExtension = Path.GetExtension(fileCode) == null ? string.Empty : Path.GetExtension(fileCode).ToLower();
            switch (fileExtension)
            {
                case ".txt":
                case ".jpg":
                case ".png":
                case ".bmp":
                case ".ppt":
                case ".pptx":
                    return FileConvert(fileCode, ".pdf", out message);
                case ".pdf":
                case ".caj":
                    message = string.Empty;
                    return true;
                case ".docx":
                case ".doc":
                    return FileConvert(fileCode, ".xml", out message);
                default:
                    message = string.Empty;
                    return true;
            }
        }


        public static bool FileConvert(string fileCode, string convertfileType, out string message)
        {
            using (var factory = new ChannelFactory<IFragmentService>("OKMS.FragmentService.WcfService.Fragment.FragmentService"))
            {
                var service = factory.CreateChannel();

                //尝试转换
                int taskId = service.HfsFileConvert(fileCode, convertfileType);
                if (taskId < 0)
                {
                    message = string.Empty;
                    return false;
                }
                //转换成功则返回上传后的文件名称和任务Id
                message = taskId.ToString();
                return true;
            }
        }

    }
}