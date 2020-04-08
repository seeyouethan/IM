using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models.Models.Msg
{
    public class SingleMsg
    {
        /// <summary>
        /// 这条消息在数据库中存放的ID
        /// </summary>
        public string msgid { get; set; }
        /// <summary>
        /// 发送者的ID
        /// </summary>
        public string uid { get; set; }
        /// <summary>
        /// 发送者的真实姓名
        /// </summary>
        public string truename { get; set; }
        /// <summary>
        /// 发送者的头像
        /// </summary>
        public string photo { get; set; }
        /// <summary>
        /// 消息发送时间
        /// </summary>
        public string createtime { get; set; }
        /// <summary>
        /// 消息内容（如果type=1表示图片消息，则该字段存放内容为图片的地址）
        /// 如果type=2,则该字段存放的内容为完整的文件名 如：Abc.docx
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 消息类型（0、1、2分别表示文本消息、图片消息、文件消息其中目前还不支持文件消息，待后期完善）
        /// 2018年12月12日 新增type=3表示一个整合的消息,同时包含了文本字符串和图片，例如：xxxx[$PICTURE$]xxxxxxxx[$PICTURE$]xxxxx其中[$PICTURE$]为占位符，图片地址从imgs中获取
        /// 新增type=4表示语音类消息
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 群组ID，若为空则表示是单人消息，若不为空则表示是所在群组的消息
        /// </summary>
        public string groupid { get; set; }
        /// <summary>
        /// 若type=3 则这个字段中存放的list字符串分别是msg中[$PICTURE$]占位符中对应的图片地址
        /// </summary>

        public List<string> imgs=new List<string>();

        /// <summary>
        /// 如果是文件类型，则该字段存放的是文件的下载地址URL
        /// 如果是语音类型，则该字段存放的是语音的播放地址URL
        /// 如果是图片类型，则该字段存放的是对应缩略图的地址URL
        /// </summary>
        public string fileurl { get; set; }
        
        /// <summary>
        /// 如果是语音类型，则该字段存放的是语音消息的长度，(整数，以秒为单位)
        /// </summary>
        public int duration { get; set; }

    }
}
