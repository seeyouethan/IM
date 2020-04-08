using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using OKMS.FragmentService.WcfService.Fragment;

namespace Edu.Service.Service
{
    public class FileConvertRepository : IFileConvertRepository
    {
        public virtual bool FileConvert(string fileCode, string convertfileType, out string message)
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

        public string GetFileContentWithCustomUrl(string fileCode, string url)
        {
            var client = new FileConvertServiceClient();
            try
            {
                var entity = client.GetHfsFragmentationFileWithCustomImageUrl(fileCode, url);
                return entity.Content;
            }
            finally
            {
                client.Abort();
            }
        }

    }
}
