using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Edu.Service
{
    /// <summary>
    /// 操作待转换的xml文件
    /// </summary>
    public class ResourseConvert
    {
        private string _xmlPath;
        public ResourseConvert(string xmlPath)
        {
            _xmlPath = xmlPath;
        }
        /// <summary>
        /// 插入xml文件
        /// </summary>
        public void InsertXml(string path)
        {
            XElement root = XElement.Load(_xmlPath);
            if (root.Elements("File").FirstOrDefault(p => p.Value == path) == null)
            {
                XElement column = new XElement("File", System.Web.HttpContext.Current.Server.MapPath(path));
                root.Add(column);
                root.Save(_xmlPath);
            }
          
        }

        public static Boolean FileIsUsed(String fileFullName)
        {
            Boolean result = false;

            //判断文件是否存在，如果不存在，直接返回 false
            if (!System.IO.File.Exists(fileFullName))
            {
                result = false;

            }//end: 如果文件不存在的处理逻辑
            else
            {//如果文件存在，则继续判断文件是否已被其它程序使用

                //逻辑：尝试执行打开文件的操作，如果文件已经被其它程序使用，则打开失败，抛出异常，根据此类异常可以判断文件是否已被其它程序使用。
                System.IO.FileStream fileStream = null;
                try
                {
                    fileStream = System.IO.File.Open(fileFullName, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);

                    result = false;
                }
                catch (System.IO.IOException ioEx)
                {
                    result = true;
                }
                catch (System.Exception ex)
                {
                    result = true;
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Close();
                    }
                }

            }//end: 如果文件存在的处理逻辑

            //返回指示文件是否已被其它程序使用的值
            return result;

        }//end method FileIsUsed

    }
}
