using System.IO;

namespace Edu.Service.Service
{
    public class FileConvertService : IFileConvertService
    {
        private readonly IFileConvertRepository _iFileConvertRepository = new FileConvertRepository();

        public FileConvertService()
        {
        }
        public bool FileConvert(string fileCode, out string message)
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
                    return _iFileConvertRepository.FileConvert(fileCode, ".pdf", out message);
                case ".pdf":
                case ".caj":
                    message = string.Empty;
                    return true;
                case ".docx":
                case ".doc":
                    return _iFileConvertRepository.FileConvert(fileCode, ".xml", out message);
                default:
                    message = string.Empty;
                    return true;
            }
        }

        public string GetFileContentWithCustomUrl(string fileCode, string url)
        {
            return _iFileConvertRepository.GetFileContentWithCustomUrl(fileCode, url);
        }
    }
}
