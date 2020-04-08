namespace Edu.Service.Service
{
    public interface IFileConvertService
    {
        /// <summary>
        /// 文件转换
        /// </summary>
        /// <param name="fileCode"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool FileConvert(string fileCode, out string message);

        /// <summary>
        /// 获取文件内容
        /// </summary>
        /// <param name="fileCode"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        string GetFileContentWithCustomUrl(string fileCode, string url);
    }
}
