using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.Models.Models.Msg
{
    /// <summary>
    /// 向SignalR发送消息实体
    /// </summary>
    public class Msg
    {
        /// <summary>
        /// 发送者的id
        /// </summary>
        public string fromuid { get; set; }
        /// <summary>
        /// 接收者的id（如果isgroupd=1，则该id表示群组的id）
        /// </summary>
        public string touid { get; set; }
        /// <summary>
        /// 是否是群组，0表示否，1表示是
        /// </summary>
        public int isgroup { get; set; }
        /// <summary>
        /// 发送者的真实姓名
        /// </summary>
        public string fromrealname { get; set; }
        /// <summary>
        /// 接收者的真实姓名（如果isgroupd=1，则该名称表示群组的名称）
        /// </summary>
        public string torealname { get; set; }
        /// <summary>
        /// 通过signalr转发成功后生成的消息id，该id是消息回执用的，初始赋值为空，发送成功后会赋值为服务端存放的id
        /// </summary>
        public string id0 { get; set; }
        /// <summary>
        /// 客户端发送的时候生成的消息id
        /// </summary>
        public string id1 { get; set; }
        /// <summary>
        /// 发送消息的时间，字符串格式，24小时格式，精确到秒 yyyy/mm/dd hh:mm:ss
        /// </summary>
        public string msgtime { get; set; }
        /// <summary>
        /// 消息类型
        /// 0表示文本消息
        /// 1表示图片消息
        /// 2表示文件消息
        /// 3表示图文混合消息（移动端暂时用不到）
        /// 4表示语音类消息（移动端专用）
        /// 5表示地图签到信息
        /// 6表示群组视频聊天消息 2019年4月19日 新增功能
        /// 7表示视频类消息(video) 2019年11月18日 新增功能
        /// 8表示通知类消息(目前此类消息，消息内容只考虑文本字符串类消息)，需要其他成员去确认回执的 2019年12月13日 新增功能
        /// </summary>
        public int msgtype { get; set; }
        /// <summary>
        /// 消息内容，根据msgtype的不同，存放不同的数据
        /// msgtype=0 存放文本消息内容
        /// msgtype=1 存放图片的url
        /// msgtype=2 存放文件的url
        /// msgtype=3 存放图图文混合消息（移动端暂时用不到）
        /// msgtype=4 存放语音消息的url    
        /// msgtype=5 存放地图签到消息    
        /// msgtype=6 存放 xxx加入视频聊天
        /// msgtype=7 存放 视频播放地址的url 
        /// msgtype=8 存放 文本消息内容(回执类消息)
        /// msgtype=9 存放 协同文档
        /// msgtype=10 存放 协同研讨
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 当且仅当msgtype=1,即为图片消息的时候，该字段存放的是生成的缩略图的地址;修改为图片文件的名字2020年4月27日15:37:30
        /// 当且仅当msgtype=2，即为文件消息的时候，该字段存放完整的文件名，比如abc.docx
        /// 
        /// 
        ///  msgtype=9 存放 协同文档对应的id
        ///  msgtype=10 存放 协同研讨对应的id
        /// 
        /// 其他情况下，该字段赋值为空字符串
        /// </summary>
        public string filename { get; set; }
        /// <summary>
        /// 当msgtype=2,即为文件消息的时候，该字段存放的是文件的大小，单位为Byte;
        /// 当msgtype=4，即为语音消息的时候，则该字段存放的是语音消息的长度，(整数，以秒为单位)  
        /// 当msgtype=8，表示对应的用户是否已经读过了这个回执类消息 0表示未读，1表示已读        
        /// 其他情况下，该字段赋值为0
        /// </summary>
        public int duration { get; set; }
        /// <summary>
        /// 当且仅当msgtype=3的时候，即为图文混合消息的时候，该字段存放图片的集合
        /// 其他情况下，赋值为一个空的集合即可。
        /// </summary>
        public List<string> imglist { get; set; }
        /// <summary>
        /// 终端类型，主要分为okcs\oa\app 三个终端，如果为空，则默认为app端发送的消息 2019年11月13日新增
        /// 这个字段，目前主要用于多个终端同时在线的时候，互相同步自己的消息
        /// </summary>
        public string terminal { get; set; }

        /// <summary>
        /// 2020年2月3日新增，该字段不为空时，表示为主题消息，该字段表示主题的ID
        /// </summary>
        public string subjectId { get; set; }


        /// <summary>
        /// 主题名称(备用字段)
        /// </summary>
        public string subjectTitle { get; set; }


        /// <summary>
        /// 引用消息的Id
        /// </summary>
        public int? quoteId {
            get;set;
        }
        /// <summary>
        /// 引用消息的具体内容 比如：张三：这道题改怎么解？
        /// 具体格式就是 【发送者】：【内容】，注意用中文的冒号
        /// </summary>
        public string quoteContent { get; set; }


        /// <summary>
        /// 这条消息的点赞次数
        /// </summary>
        public int thumbCount { get; set; }

        /// <summary>
        /// 我自己是否点赞 true/false
        /// </summary>
        public bool selfThumb { get; set; }

    }

    public class GDPosition
    {
        /// <summary>
        /// 经纬度
        /// </summary>
        public string latitude { get; set; }
        /// <summary>
        /// 具体地址
        /// </summary>
        public string address { get; set; }
    } 
}