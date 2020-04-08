using Edu.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models
{
    public enum ActionClick
    {
        [EnumDescription("增加")]
        Add = 1,
        [EnumDescription("修改")]
        Mody = 2,
        [EnumDescription("删除")]
        Delete = 3,
        [EnumDescription("查询")]
        Search = 4,
        [EnumDescription("浏览")]
        View = 5,
        [EnumDescription("登录")]
        Login = 6,
        [EnumDescription("注销")]
        LoginOut = 7,
        [EnumDescription("其他")]
        Other = 9
    }
}
