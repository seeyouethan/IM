using Edu.Tools;

namespace Edu.Models.Enums
{
    public enum CallDtae
    {
        [EnumDescription("不提醒")]
        Add = 0,
        [EnumDescription("提前5分钟")]
        Mody = 1,
        [EnumDescription("提前10分钟")]
        Delete = 2,
        [EnumDescription("提前15分钟")]
        Search = 3,
        [EnumDescription("提前30分钟")]
        View = 4,
        [EnumDescription("提前1小时")]
        Login = 5,
    }
}
