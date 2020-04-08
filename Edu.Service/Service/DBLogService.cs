using Edu.Entity;
using Edu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Service
{
    public class DBLogService : CoreServiceBase
    {
        /// <summary>
        /// 插入日志
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="opID"></param>
        /// <param name="TableName"></param>
        /// <param name="ac"></param>
        /// <param name="bz"></param>
        /// <returns></returns>
        public bool insert(ActionClick ac, object obj, string Name = "")
        {
            string TableName = "";
            string bz = "";

            Type type = obj.GetType();
            try
            {
                TableName = type.FullName;
                bz = Edu.Tools.JsonHelper.Instance.Serialize(obj);
            }
            catch (Exception ex){
                Edu.Tools.LogHelper.Info(ex.Message);
            };
            LogInfo info = new LogInfo();
            if (string.IsNullOrEmpty(Name))
            {
                info.Name = type.Name;
            }
            else
            {
                info.Name = Name;
            }
         
       
            info.TableName = TableName;
            info.OpType = (int)ac;
            info.Remark = bz;
            info.Url = Edu.Tools.WebHelper.GetUrl();
            info.CreateDate = DateTime.Now;
            info.UserID = LoginUserService.UserID;
            info.UserName = LoginUserService.userName;
            info.IP = Edu.Tools.WebHelper.GetIP();
            unitOfWork.DLogInfo.Insert(info);
            unitOfWork.Save();
            return false;
        }

        public bool insert(string TableName, ActionClick ac, string Name = "", string bz = "")
        {

            LogInfo info = new LogInfo();
            info.Name = Name;
            info.TableName = TableName;
            info.OpType = (int)ac;
            info.Remark = bz;
            info.Url = Edu.Tools.WebHelper.GetUrl();
            info.CreateDate = DateTime.Now;
            info.UserID = LoginUserService.UserID;
            info.UserName = LoginUserService.userName;
            info.IP = Edu.Tools.WebHelper.GetIP();
            unitOfWork.DLogInfo.Insert(info);
            unitOfWork.Save();
            return false;
        }

    }
}
