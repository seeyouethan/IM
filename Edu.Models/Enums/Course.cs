using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edu.Tools;

namespace Edu.Models
{
    public enum Course_Type
    {

        [EnumDescription("微课")]
        Micro = 1,
        [EnumDescription("慕课")]
        MOOC = 2
    }
    public enum Course_State
    {
        [EnumDescription("未发布")]
        No = 0,
        [EnumDescription("已经发布")]
        Yes = 1
    }
}
