using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edu.Tools;
namespace Edu.Models
{
    public enum UserInfo_isAdmin
    {

        [EnumDescription("管理用户")]
        admin = 1,
        [EnumDescription("普通用户")]
        commonUser = 2
    }
    public enum UserInfo_UserState
    {
        [EnumDescription("已删除")]
        Delete = -1,
        [EnumDescription("被锁定")]
        Lock = 0,
        [EnumDescription("正常登陆")]
        Normal = 1
    }
    public enum UserInfo_UserStateColor
    {
        [EnumDescription("<span style='color:red'>已删除</span>")]
        Delete = -1,
        [EnumDescription("<span style='color:blue'>被锁定</span>")]
        Lock = 0,
        [EnumDescription("正常登陆")]
        Normal = 1
    }
    public enum UserInfo_Role
    {
        [EnumDescription("超级管理员")]
        admin = 1,
        [EnumDescription("教师")]
        teacher = 2,
        [EnumDescription("普通用户")]
        User = 3

    }

}
