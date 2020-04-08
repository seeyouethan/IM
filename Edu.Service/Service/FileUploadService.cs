using Edu.Models;
using Edu.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Edu.Models.Models;

namespace Edu.Service
{
    public class FileUploadService
    {
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public MFileInfo FileUpload(string fPath)
        {
            var list = System.Web.HttpContext.Current.Request.Files;
            if (list.Count == 0)
            {
                return null;
            }
            var httpfile = list[0];
            if (httpfile == null || httpfile.ContentLength == 0)
            {
                return null;
            }
            if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath(fPath)))
            {
                FileHelper.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath(fPath));
            }
            var uploadsPath = Path.Combine(fPath, CombHelper.GenerateNumber());
          
            string extraName = httpfile.FileName.Substring(httpfile.FileName.LastIndexOf(".") + 1);
            string fName = httpfile.FileName;
            string filepath = uploadsPath + "." + extraName;
            httpfile.SaveAs(System.Web.HttpContext.Current.Server.MapPath(filepath));
            string path = filepath;
            string type = extraName;
            MFileInfo mf = new MFileInfo
            {
                Name = fName,
                ExtraName = extraName,
                Path = filepath,
                Size = httpfile.ContentLength
            };
            return mf;

        }
        /// <summary>
        /// file文件上传
        /// </summary>
        /// <param name="Filedata">上传名称</param>
        /// <param name="fPath">路径需以/结尾</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public MFileInfo FileUpload(string Filedata, string fPath)
        {
            var list = System.Web.HttpContext.Current.Request.Files;
            if (list.Count == 0)
            {
                return null;
            }
            var httpfile = list[0];
            if (httpfile == null || httpfile.ContentLength == 0)
            {
                return null;
            }
            if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath(fPath)))
            {
                FileHelper.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath(fPath));
            }
            var uploadsPath = Path.Combine(fPath, CombHelper.GenerateNumber());
            string extraName = httpfile.FileName.Substring(httpfile.FileName.LastIndexOf(".") + 1);
            string fName = httpfile.FileName;
            string filepath = uploadsPath + "." + extraName;
            httpfile.SaveAs(System.Web.HttpContext.Current.Server.MapPath(filepath));
            string path = filepath;
            string type = extraName;
            MFileInfo mf = new MFileInfo
            {
                Name = fName,
                ExtraName = extraName,
                Path = filepath,
                Size = httpfile.ContentLength
            };
            return mf;

        }
      
    }
}
