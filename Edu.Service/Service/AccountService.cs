using Edu.Entity;
using Edu.Models;
using Edu.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Web;
using System.Text.RegularExpressions;
using Edu.Models.Models;

namespace Edu.Service
{
    public class AccountService : CoreServiceBase
    {
        public Result Login(LoginModel model)
        {
            Result result = new Result();
            if (string.IsNullOrEmpty(model.Account))
            {
                result.R = false;
                result.M = "用户名不能为空";
                return result;
            }
            if (string.IsNullOrEmpty(model.Password))
            {
                result.R = false;
                result.M = "密码不能为空";
                return result;
            }
            var loginUser = unitOfWork.DUserInfo.Get(p => p.UserName == model.Account).FirstOrDefault();
            if (loginUser == null)
            {
                result.R = false;
                result.M = "指定账号的用户不存在";
                return result;
            }
            if (loginUser.States != (int)UserInfo_UserState.Normal)
            {
                result.R = false;
                result.M = "用户处于不可用状态";
                return result;
            }
            if (loginUser.Pwd != Edu.Tools.SecureHelper.MD5(model.Password) && loginUser.Pwd != model.Password)
            {
                result.R = false;
                result.M = "登录密码不正确";
                return result;
            }
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(loginUser.ID.ToString(), true, 300);
            string encryptticket = FormsAuthentication.Encrypt(ticket);
            new DBLogService().insert(ActionClick.Login, loginUser);
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encryptticket));
            result.R = true;

            return result; ;
        }
        public Result Login(int userid)
        {
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(userid.ToString(), true, 300);
            string encryptticket = FormsAuthentication.Encrypt(ticket);
            new DBLogService().insert(ActionClick.Login, new { id = userid });
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encryptticket));
            Result result = new Result();
            result.R = true;
            return result;
        }
        /// <summary>
        /// 校验短信验证码
        /// </summary>
        /// <returns></returns>
        public Result verfiCode(string Phone , string verfiCode)
        {
            Result result = new Result();
            //var code = unitOfWork.DPhoneUserCode.Get(p => p.Code == verfiCode.Trim() && p.Phone == Phone.Trim()).FirstOrDefault();
            //if (code == null)
            //{
            //    result.R = false;
            //    result.M = "短信验证码校验失败，请重新输入验证码！";
            //    return result;
            //}
            //TimeSpan res = DateTime.Now - (DateTime)code.CreateDate.Value;
            //if (res.Hours >= 2)
            //{
            //    result.R = false;
            //    result.M = "短信验证码已经过期，请重新获取短信验证码！";
            //    return result;
            //}
            result.R = true;
            return result;
        }
        public Result Register(UserInfo user1)
        {
       
            Result result = new Result();

            var resultCode = verfiCode(user1.Phone, HttpContext.Current.Request.Form["txtValidateCode"].ToString());
            if (!resultCode.R)
            {
                result.R = false;
                result.M = resultCode.M;
                return result;
            }

            if (string.IsNullOrEmpty(user1.Pwd))
            {
                result.R = false;
                result.M = "密码不能为空";
                return result;
            }
            if (user1.Pwd != HttpContext.Current.Request.Form["rePwd"].ToString())
            {
                result.R = false;
                result.M = "密码不一致";
                return result;
            }
            string par = "^[a-zA-Z0-9_]{5,16}$";
            if (!Regex.IsMatch(user1.UserName, par))
            {
                result.R = false;
                result.M = "用户名输入错误";
                return result;
            }
            //string emailStr = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            //if (!Regex.IsMatch(user1.Email, emailStr))
            //{
            //    result.R = false;
            //    result.M = "邮箱格式错误";
            //    return result;
            //}

            var user = unitOfWork.DUserInfo.Get(p => p.UserName == user1.UserName).FirstOrDefault();
            if (user != null)
            {
                result.R = false;
                result.M = "该用户名已经存在";
                return result;
            }
            //var userEmail = unitOfWork.DUserInfo.Get(p => p.Email == user1.Email).FirstOrDefault();
            //if (userEmail != null)
            //{
            //    result.R = false;
            //    result.M = "该邮箱已经注册过账户";
            //    return result;
            //}
            var userPhoto = unitOfWork.DUserInfo.Get(p => p.Phone == user1.Phone).FirstOrDefault();
            if (userPhoto != null)
            {
                result.R = false;
                result.M = "该手机已经注册过账户";
                return result;
            }
            string scode = Guid.NewGuid().ToString().Replace("-", "");
            UserInfo ui = new UserInfo();
            ui.uID = Guid.NewGuid().ToString();
            ui.UserName = user1.UserName;
            ui.TrueName = user1.TrueName;
            ui.Pwd = Edu.Tools.SecureHelper.MD5(user1.Pwd);
            ui.RoleID = 2;
            ui.Email = user1.Email;
            ui.Phone = user1.Phone;
 
            ui.States = 1;
            ui.CreateDate = DateTime.Now;
            ui.CreateUser = "0";
            ui.IPAdress = StringHelper.GetIP();
            unitOfWork.DUserInfo.Insert(ui);
            unitOfWork.Save();
            new DBLogService().insert(ActionClick.Add, ui);
            result.R = true;
            result.D = ui;
            return result; ;
        }
        public byte[] GetValidateCode()
        {
            ValidateCode vCode = new ValidateCode();
            string code = vCode.CreateValidateCode(5);
            if (HttpContext.Current.Request.Cookies["ValidateCode"] != null && HttpContext.Current.Request.Cookies["ValidateCode"].Value != "")
            {
                HttpContext.Current.Request.Cookies["ValidateCode"].Expires = DateTime.Now.AddMinutes(-5);
            }
            HttpCookie hcookie = new HttpCookie("ValidateCode", code);
            hcookie.Expires = DateTime.Now.AddMinutes(1);
            HttpContext.Current.Response.Cookies.Add(hcookie);
            byte[] bytes = vCode.CreateValidateGraphic(code);
            return bytes;
        }
    }
}
