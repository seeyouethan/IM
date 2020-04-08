using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models.Models.Msg
{
    /// <summary>
    /// 个推消息message
    /// </summary>
    public class GeTuiMessage
    {
        /// <summary>
        /// 消息类型：
        ///     1   -->>   聊天消息
        ///     2   -->>   群组动态
        ///     3   -->>   群组公告
        ///     4   -->>   PMC首页公告
        ///     5   -->>   待办任务（吴雪飞负责发送）
        ///     6   -->>   新增研讨（郭晴晴）AddDiscuss
        ///     7   -->>   删除研讨（郭晴晴）DeleteDiscuss
        ///     8   -->>   修改研讨（郭晴晴）ModifyDiscuss
        ///     9   -->>   完成研讨（郭晴晴）FinishDiscuss 
        ///     10  -->>   激活研讨（郭晴晴）ActivateDiscuss
        ///     11  -->>   提示研讨（郭晴晴）RemindDiscuss
        ///     12  -->>   研讨笔记（郭晴晴）Note
        ///     13  -->>   自建群创建通知（刘静杰）
        ///     14  -->>   新建直播的通知（申志云）
        ///     15  -->>   自建群编辑通知（刘静杰）
        ///     16  -->>   公司OA平台发出的消息（杨立旭）
        ///     18  -->>   工作交办（群组日程、待办）消息通知（刘静杰）
        ///     ...待扩展
        ///  </summary>
        public int mtype { get; set; }
        /// <summary>
        /// 具体消息内容：
        ///     根据type字段类型，来存放对应消息内容的Json字符串
        ///     这里用object是因为需要存一个object类型的Model  这样可以防止字符串出现三个反斜杠，对于后面的json不好解析
        /// </summary>
        public object mcontent { get; set; }
    }
}
