using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.SignalRServer.Models
{
    /// <summary>
    /// IOS离线推送要推送消息的实体
    /// </summary>
    public class DeviceMsg
    {
        /// <summary>
        /// 唯一id
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 要接收消息的用户id
        /// </summary>
        public string uid { get; set; }
        /// <summary>
        /// devicetoken
        /// </summary>
        public string devicetoken { get; set; }
        /// <summary>
        /// 该设备的第几条通知消息（app图标右上角的数字）
        /// </summary>
        public int msgcount { get; set; }
        /// <summary>
        /// 消息类型 
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
        /// </summary>
        public int msgtype { get; set; }
        /// <summary>
        /// 消息内容 json格式数据
        /// </summary>
        public string msgcontent { get; set; }
        /// <summary>
        ///  消息时间
        /// </summary>
        public string msgtime { get; set; }
        /// <summary>
        /// 界面显示的标题
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// alert中body字段内容
        /// </summary>
        public string body { get; set; }
    }
}