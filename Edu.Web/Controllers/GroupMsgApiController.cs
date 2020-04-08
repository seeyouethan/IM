using System.Web.Mvc;
using Edu.Service;

namespace Edu.Web.Controllers
{
    public class GroupMsgApiController : BaseControl
    {

        /// <summary>
        /// 根据群组ID获取到未读的群组消息
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="groupid"></param>
        /// <returns></returns>
        public JsonResult GetUnReadGroupMsg(string uid,string groupid)
        {
            return null;
        }
    }
}


