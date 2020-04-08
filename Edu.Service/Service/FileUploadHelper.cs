using System;
using System.IO;
using System.Web;
using Edu.Models.Models;

namespace Edu.Service.Service
{
    public static class FileUploadHelper
    {
        public static JsonResultModel Upload(HttpPostedFileBase file, string fullPath, string uid, string touid,
            bool isImg = true)
        {
            fullPath = fullPath + "\\" + uid + "\\" + touid + "\\";
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            string strPostfix = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            try
            {
                if (isImg == false && file.ContentLength > 10485760*50)
                {
                    return JsonResultHelper.CreateJson(null, false, "文件大小过大，请上传500M以内的文件.");
                }
                else if (file.ContentLength < 1)
                {
                    return JsonResultHelper.CreateJson(null, false, "文件大小为0，上传失败.");
                }
                if (isImg && file.ContentLength > 1048576*50)
                {
                    return JsonResultHelper.CreateJson(null, false, "图片大小不得大于50M，上传失败.");
                }

                if (file != null)
                {
                    string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    string fileName = file.FileName; // Guid.NewGuid().ToString();
                    string DtFromat = DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
                    string fullFileName = fileName;
                    if (fileName.LastIndexOf('.') > 0)
                    {
                        fullFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + "_" + DtFromat +
                                       fileName.Substring(fileName.LastIndexOf('.'));
                    }
                    if (fileName != null && (fileName.LastIndexOf('\\') > 0))
                        fullFileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);

                    string oldFileName = fullFileName;

                    string fileSavePath = string.Format("{0}{1}", fullPath, fullFileName);
                    var returnurl = isImg
                        ? "/upload/img/" + uid + "/" + touid + "/" + fullFileName
                        : "/upload/file/" + uid + "/" + touid + "/" + fullFileName;
                    file.SaveAs(fileSavePath);
                    object imgObj = new {src = returnurl};
                    object fileObj = new {src = returnurl, name = oldFileName};
                    return JsonResultHelper.CreateJson(isImg ? imgObj : fileObj);
                }
                return JsonResultHelper.CreateJson(null, false, "请添加一个文件.");
            }
            catch (Exception ex)
            {
                //记录日志
                return JsonResultHelper.CreateJson(null, false, "添加文件出错，请联系平台管理员!!" + ex.Message);
            }
        }


        public static JsonResultModel UploadFile(HttpPostedFileBase file, string fullPath)
        {
            fullPath = fullPath + "\\";
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            try
            {
                if (file.ContentLength > 104857600)
                {
                    return JsonResultHelper.CreateJson(null, false, "文件大小过大，请上传100M以内的文件.");
                }
                if (file.ContentLength < 1)
                {
                    return JsonResultHelper.CreateJson(null, false, "文件大小为0，上传失败.");
                }

                if (file != null)
                {
                    string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    string fileName = file.FileName; // Guid.NewGuid().ToString();
                    string DtFromat = DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
                    string fullFileName = fileName;
                    if (fileName.LastIndexOf('.') > 0)
                    {
                        fullFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + "_" + DtFromat +
                                       fileName.Substring(fileName.LastIndexOf('.'));
                    }
                    if (fileName != null && (fileName.LastIndexOf('\\') > 0))
                        fullFileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);

                    string oldFileName = fullFileName;

                    string fileSavePath = string.Format("{0}{1}", fullPath, fullFileName);
                    var returnurl = "/im/upload/worklogfile/" + LoginUserService.ssoUserID + "/" + fullFileName;
                    file.SaveAs(fileSavePath);
                    //object imgObj = new { src = returnurl };
                    object fileObj = new {src = returnurl, name = oldFileName};
                    return JsonResultHelper.CreateJson(fileObj);
                }
                return JsonResultHelper.CreateJson(null, false, "请添加一个文件.");
            }
            catch (Exception ex)
            {
                //记录日志
                return JsonResultHelper.CreateJson(null, false, "添加文件出错，请联系平台管理员!!" + ex.Message);
            }
        }



        public static JsonResultModel UploadApi(HttpPostedFileBase file, string fullPath, string uid, string touid,
            bool isImg = true)
        {
            fullPath = fullPath + "\\" + uid + "\\" + touid + "\\";
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            string strPostfix = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            try
            {
                if (isImg == false && file.ContentLength > 10485760*50)
                {
                    return JsonResultHelper.CreateJson(null, false, "文件大小过大，请上传500M以内的文件.");
                }
                else if (file.ContentLength < 1)
                {
                    return JsonResultHelper.CreateJson(null, false, "文件大小为0，上传失败.");
                }
                if (isImg && file.ContentLength > 1048576*50)
                {
                    return JsonResultHelper.CreateJson(null, false, "图片大小不得大于50M，上传失败.");
                }

                if (file != null)
                {
                    string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    string fileName = file.FileName; // Guid.NewGuid().ToString();
                    string DtFromat = DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
                    string fullFileName = fileName;
                    if (fileName.LastIndexOf('.') > 0)
                    {
                        fullFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + "_" + DtFromat +
                                       fileName.Substring(fileName.LastIndexOf('.'));
                    }
                    if (fileName != null && (fileName.LastIndexOf('\\') > 0))
                        fullFileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);

                    string oldFileName = fullFileName;

                    string fileSavePath = string.Format("{0}{1}", fullPath, fullFileName);
                    var returnurl = isImg
                        ? "/upload/img/" + uid + "/" + touid + "/" + fullFileName
                        : "/upload/file/" + uid + "/" + touid + "/" + fullFileName;
                    file.SaveAs(fileSavePath);
                    object imgObj = returnurl;
                    object fileObj = new {src = returnurl, name = oldFileName};
                    return JsonResultHelper.CreateJson(isImg ? imgObj : fileObj);
                }
                return JsonResultHelper.CreateJson(null, false, "请添加一个文件.");
            }
            catch (Exception ex)
            {
                //记录日志
                return JsonResultHelper.CreateJson(null, false, "添加文件出错，请联系平台管理员!!" + ex.Message);
            }
        }
    }
}
