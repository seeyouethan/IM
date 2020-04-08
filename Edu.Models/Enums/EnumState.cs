using Edu.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models
{
    /// <summary>
    /// 数据状态
    /// </summary>
    public enum EnumState
    {
        [EnumDescription("已经删除")]
        Delete = -1,
        [EnumDescription("禁止使用")]
        Ban = 0,
        [EnumDescription("正常使用")]
        Normal = 1
    }

}
