using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNet.AAMS.Foundation.Authentication;
using KNet.AAMS.Foundation.Model;
using Edu.Tools;
using System.Web;
using System.ServiceModel;
using KNet.AAMS.Web.Security;

namespace Edu.Service
{
    public class ssoUser
    {
        /// <summary>
        /// 获取单个用户信息
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public DeptUser GetUserInfo(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return null;
            var fac = new ChannelFactory<IAuthenticationService>("authentication");
            try
            {
                var ser = fac.CreateChannel();
                IList<string> userNames = new List<string>();
                userNames.Add(userName);
                var users = ser.GetDeptUsersByUserNames(userNames);
                if (users == null || users.Count() == 0)
                    return null;
                return users[0];
            }
            finally
            {
                fac.Abort();
            }
        }

        public DeptUser GetUserByID(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return null;
            var fac = new ChannelFactory<IAuthenticationService>("authentication");
            try
            {
                var ser = fac.CreateChannel();
                IList<string> userIDs = new List<string>();
                userIDs.Add(userID);
                var users = ser.GetDeptUsersByUserId(userID);

                if (users == null || users.Count() == 0)
                    return null;

                return users[0];
                //如果是离职，Enabled为false
            }
            finally
            {
                fac.Abort();
            }
        }

        public IList<DeptUser> SearchUserFromAll(string keyword, int pageIndex = 1, int pageSize = 50, int groupId = 0)
        {
            List<object> listUsers = new List<object>();
            IList<DeptUser> users = new List<DeptUser>();
            using (var factory = new ChannelFactory<IAuthenticationService>("authentication"))
            {
                try
                {
                    var service = factory.CreateChannel();
                    Dictionary<string, string> dicCondition = new Dictionary<string, string>();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicCondition.Add("realname", keyword);
                    }

                    int count = 0;
                    users = service.GetDeptUsers(dicCondition, pageIndex, pageSize, out count);
                }
                finally
                {
                    factory.Abort();
                }
                
            }
            return users;
        }

        public IList<User> GetUsersByRealName(string keyword, out int count, int pageIndex = 1, int pageSize = 50, int groupId = 0)
        {
            count = 0;
            IList<User> users = new List<User>();
            if (keyword == "")
            {
                return null;
            }
            else
            {
                using (var factory = new ChannelFactory<IAuthenticationService>("authentication"))
                {
                    try
                    {
                        var service = factory.CreateChannel();
                        users = service.GetUsersByRealName(keyword, pageIndex, pageSize, out count);
                    }
                    finally
                    {
                        factory.Abort();
                    }
                    
                }
                return users;
            }
        }

        /// <summary>
        /// 根据用户uid的list获得对应的用户的list  liujingjie
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public IList<User> GetUsersByUserIds(IList<string> userIds)
        {
            IList<User> users = new List<User>();
            if (userIds == null || userIds.Count == 0)
            {
                return null;
            }
            else
            {
                using (var factory = new ChannelFactory<IAuthenticationService>("authentication"))
                {
                    try
                    {
                        var service = factory.CreateChannel();
                        users = service.GetUsersByUserIds(userIds);
                    }
                    finally
                    {
                        factory.Abort();
                    }
                }
                return users;
            }
        }

        public IList<DeptUser> GetDeptUsersByRealName(string orgid,string realName,int page,int size,out int total)
        {
            IList<DeptUser> users = new List<DeptUser>();

            using (var factory = new ChannelFactory<IAuthenticationService>("authentication"))
            {
                try
                {
                    var service = factory.CreateChannel();
                    users = service.GetDeptUsersByRealName(orgid, realName, page, size, out total);
                }
                finally
                {
                    factory.Abort();
                }
            }
            return users;
        }




        public List<Object> GetMembersOfUnit(string unitId, string realname = "")
        {
            List<Object> ret = new List<Object>();
            var fac = new ChannelFactory<IAuthenticationService>("authentication");
            try
            {
                var ser = fac.CreateChannel();
                //获取机构下的部门信息
                var depList = ser.GetDeptsByOrgId(unitId);
                if (depList != null && depList.Any())
                {
                    ret.AddRange(depList.Select(d => new { id = d.DeptId, pid = d.ParentId, RealName = d.FullName, department = "", type = 1 }));//type = 1,参考okms写的，表示部门/机构信息
                }

                //获取机构下的成员信息
                var userList = new List<KNet.AAMS.Foundation.Model.DeptUser>();
                if (string.IsNullOrWhiteSpace(realname))
                {
                    userList = ser.GetDeptUsersByOrg(unitId).ToList();
                    userList = userList.Where(p => p.Enabled == true).ToList();
                }
                else
                {
                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                    dictionary.Add("orgid", unitId.ToLowerInvariant());
                    dictionary.Add("realname", realname.ToLowerInvariant());
                    int total;
                    userList = ser.GetDeptUsers(dictionary, 1, int.MaxValue, out total).ToList();
                }
                if (userList != null && userList.Any())
                {
                    ret.AddRange(userList.Select(u => new { id = u.UserId, pid = u.DeptId, RealName = u.RealName, userName = u.UserName, department = depList.Where(d => d.DeptId == u.DeptId).FirstOrDefault() == null ? "" : depList.Where(d => d.DeptId == u.DeptId).FirstOrDefault().FullName, type = 0, /*icon = AuthenticationConstant.GetUserLogoUrl(u.UserId),*/Enabled=u.Enabled,Mobile=u.Mobile}));
                    // type = 0,参考okms写的，表示用户成员
                }
                return ret;
            }
            finally
            {
                fac.Abort();
            }
        }


        //public string GetHeadByUserID(string uid = "")
        //{
        //    if (uid == "")
        //    {
        //        uid = Edu.Service.LoginUserService.ssoUserID;

        //    }
        //    string imghead = "";
        //    try
        //    {
        //        if (ConfigHelper.GetConfigString("IsSso") == "1")
        //        {
        //            imghead = KNet.AAMS.Web.Security.AuthenticationConstant.GetUserLogoUrl(uid);
        //        }
        //        else
        //        {
        //            var user = new UnitOfWork().DUserInfo.Get(p => p.uID == uid).FirstOrDefault();
        //            imghead = user.Photo == null ? imghead : user.Photo;
        //        }
        //        if (imghead == null || imghead == "")
        //        {
        //            imghead = HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/Content/images/author.jpg";
        //        }

        //        return imghead;
        //    }
        //    catch
        //    {
        //        return imghead;
        //    }

        //}
        //public string GetHead(string uname = "")
        //{
        //    if (uname == "")
        //    {
        //        uname = Edu.Service.LoginUserService.userName;

        //    }
        //    string imghead = "";
        //    try
        //    {
        //        if (ConfigHelper.GetConfigString("IsSso") == "1")
        //        {
        //            var query = this.GetUserInfo(uname);
        //            imghead = KNet.AAMS.Web.Security.AuthenticationConstant.GetUserLogoUrl(query.UserId);
        //        }
        //        else
        //        {
        //            var user = new UnitOfWork().DUserInfo.GetByID(uname);
        //            imghead = user.Photo==null? imghead : user.Photo;
        //        }
        //        if (imghead == null || imghead == "")
        //        {
        //            imghead = HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/Content/images/author.jpg";
        //        }

        //        return imghead;
        //    }
        //    catch
        //    {
        //        return imghead;
        //    }

        //}

        //public string ClientGetHead(string uname = "")
        //{
        //    if (uname == "")
        //    {
        //        uname = Edu.Service.LoginUserService.userName;

        //    }
        //    string imghead = "";
        //    try
        //    {
        //        var query = this.GetUserInfo(uname);

        //        if (ConfigHelper.GetConfigString("IsSso") == "1")
        //        {
        //            imghead = KNet.AAMS.Web.Security.AuthenticationConstant.GetUserLogoUrl(query.UserId);
        //        }
        //        else
        //        {
        //            var user = new UnitOfWork().DUserInfo.GetByID(uname);
        //            imghead = user.Photo == null ? imghead : user.Photo;
        //        }
        //        if (imghead == null || imghead == "")
        //        {
        //            imghead = ConfigHelper.GetConfigString("website").TrimEnd('/') + "/Content/images/author.jpg";
        //        }

        //        return imghead;
        //    }
        //    catch
        //    {
        //        return imghead;
        //    }

        //}
    }
}