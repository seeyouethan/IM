using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Service.Service
{
    public static class EnumHelper
    {

        /// <summary>
        /// 获取枚举的描述
        /// </summary>
        /// <param name="en">枚举</param>
        /// <returns>返回枚举的描述</returns>
        public static string GetDescription(Enum en)
        {
            Type type = en.GetType(); //获取类型
            MemberInfo[] memberInfos = type.GetMember(en.ToString()); //获取成员
            if (memberInfos != null && memberInfos.Length > 0)
            {
                DescriptionAttribute[] attrs =
                    memberInfos[0].GetCustomAttributes(typeof (DescriptionAttribute), false) as DescriptionAttribute[];
                    //获取描述特性

                if (attrs != null && attrs.Length > 0)
                {
                    return attrs[0].Description; //返回当前描述
                }
            }
            return en.ToString();
        }
    }
}
