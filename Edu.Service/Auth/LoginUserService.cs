using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Edu.Tools;
using Edu.Data;
using Edu.Entity;

namespace Edu.Service
{
    public class LoginUserService
    {

        public static string userName
        {
            get
            {
                if (User == null)
                {
                    return "";
                }
                return User.UserName;
            }
        }
        public static string realName
        {
            get
            {
                if (User == null)
                {
                    return "";
                }
                return User.TrueName;
            }
        }
        public static string ssoUserID
        {
            get
            {
                if (User == null)
                {
                    return "";
                }
                return User.uID;
            }
        }
        public static string ssoUserUnitID
        {
            get
            {
                if (User == null)
                {
                    return "";
                }
                return User.UnitID;
            }
        }
        public static int UserID
        {
            get
            {
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    if (Edu.Tools.ConfigHelper.GetConfigString("IsSso") == "1")
                    {//启用统一认证
                        string uid = "";
                        var LoginUser = HttpContext.Current.User.Identity as KNet.AAMS.Web.Security.ApplicationIdentity;// 获取当前登录用户
                        if (LoginUser == null)
                        {
                            uid = HttpContext.Current.User.Identity.Name;
                        }
                        else
                        {
                            uid = LoginUser.Id;
                        }
                        var user = new UnitOfWork().DUserInfo.Get(p => p.uID == uid).FirstOrDefault();//通过当前登录guid本系统用户
                        if (user == null)
                        {//没有用户插入用户
                            UnitOfWork unitOfWork = new UnitOfWork();
                            var us = new ssoUser().GetUserByID(uid);
                            if (us != null)
                            {//sso存在用户
                                string scode = Guid.NewGuid().ToString().Replace("-", "");
                                UserInfo ui = new UserInfo();
                                ui.UserName = us.UserName;
                                ui.TrueName = us.RealName;
                                ui.Pwd = us.Password;
                                ui.RoleID = 2;
                                ui.Email = us.Email;
                                ui.States = 1;
                                ui.CreateDate = DateTime.Now;
                                ui.CreateUser = "admin";
                                ui.Photo = us.Logo;
                                ui.uID = us.UserId;
                                ui.UnitID=us.OrgId;
                                unitOfWork.DUserInfo.Insert(ui);
                                unitOfWork.Save();
                                return ui.ID;
                            }
                            else
                            {//sso中不存改用户
                                Edu.Tools.LogHelper.Error("该登录违法登录，请联系管理员,该登录用户" + HttpContext.Current.User.Identity.Name);
                                HttpContext.Current.Response.Write("该登录违法登录，请联系管理员");

                                return user.ID;
                            }
                        }
                        return user.ID;

                    }
                    else
                    {
                        string cookieName = FormsAuthentication.FormsCookieName;
                        HttpCookie authCookie = HttpContext.Current.Request.Cookies[cookieName];
                        if (authCookie != null && authCookie.Value != "")
                        {
                            FormsAuthenticationTicket authTicket = null;
                            authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                            return int.Parse(authTicket.Name);
                        }
                        else return 0;
                    }
                }
                return 0;
            }

        }
        /// <summary>
        /// 读取当前登录用户 如果缓存中存在直接读取缓存 不存在读数据库
        /// </summary>
        public static UserInfo User
        {
            get
            {
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {   //如果有用户登录 不能返回null
                    var old = CacheHelper.GetCache("UserInfo" + HttpContext.Current.User.Identity.Name);
                    if (old == null)
                    {
                        UserInfo user;
                        user = new UnitOfWork().DUserInfo.GetByID(UserID);
                        if (user == null)
                        {
                            Edu.Tools.LogHelper.Error("该登录违法登录，请联系管理员,该登录用户" + HttpContext.Current.User.Identity.Name);
                            HttpContext.Current.Response.Write("该登录违法登录，请联系管理员");
                        }
                        CacheHelper.SetCache("UserInfo" + HttpContext.Current.User.Identity.Name, user);
                        return user;
                    }
                    else
                    {
                        return old as UserInfo;
                    }
                }
                else
                {
                    return null;
                }


            }
        }
        public static UserRole Role
        {
            get
            {
                if (UserID == 0)
                {
                    return null;
                }
                var old = Edu.Tools.CacheHelper.GetCache("UserRole" + UserID);
                if (old == null)
                {
                    UserRole userrole;
                    userrole = new UnitOfWork().DUserRole.GetByID(User.RoleID);
                    CacheHelper.SetCache("UserRole" + UserID, userrole);
                    return userrole;
                }
                else
                {
                    return old as UserRole;
                }
            }
        }
    }
}
