using Edu.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Edu.Service
{
    /// <summary>
    /// 权限认证 如果多个角色用逗号分隔
    /// </summary>
    public class MyAuthAttribute : System.Web.Mvc.AuthorizeAttribute
    {

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (LoginUserService.UserID == 0)
            {
                return false;
            }
            UserInfo user = LoginUserService.User;
            string[] StrRoles = Roles.Split(',');//通过逗号来分割允许进入的用户角色
            if (string.IsNullOrWhiteSpace(Roles))
            {
                if (isAdmin == "0")
                {
                    return true;
                }
                else
                {
                    return LoginUserService.Role.IsAdmin == "1";
                }
            }
            else
            {
                if (StrRoles.Contains(user.RoleID.ToString()))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }
        /// <summary>
        /// 是否是管理员 默认否
        /// </summary>
        public string isAdmin = "0";

    }
}
