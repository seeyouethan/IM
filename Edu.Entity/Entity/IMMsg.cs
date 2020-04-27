namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("IMMsg")]
    public partial class IMMsg
    {
        public int ID { get; set; }

        [StringLength(255)]
        public string FromuID { get; set; }

        [StringLength(255)]
        public string TouID { get; set; }
        /// <summary>
        /// 消息内容，若是text类型，则直接存放文本，若是img 则存放的为一个img的url, 若为file 则存放的是file的名称，比如abc.docx
        /// </summary>
        [StringLength(2000)]
        public string Msg { get; set; }

        [StringLength(255)]
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 引用/回复的 消息的id
        /// </summary>
        public int? QuoteId { get; set; }

        /// <summary>
        /// 2020年4月8日11:07:23 新增 引用具体内容
        /// </summary>
        public string QuoteContent { get; set; }

        /// <summary>
        /// 发送人姓名
        /// </summary>
        public string fromusername { get; set; }
        /// <summary>
        /// 接收人姓名
        /// </summary>
        public string tousername { get; set; }
        /// <summary>
        /// 消息类型：（0、1、2分别表示文本消息、图片消息、文件消息）新增type=3表示一个整合的消息,同时包含了文本字符串和图片,新增type=4 表示是语音消息
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// 是否是群组消息 0表示否，1表示是
        /// </summary>
        public int isgroup { get; set; }
        
        /// <summary>
        /// 2018年12月20日 新增的字段，用来存放文件的下载地址  
        /// 2019年9月29日 新增 当msgtype=1 即图片消息的时候，该字段存放缩略图的地址
        /// </summary>
        public string FileUrl { get; set; }

        /// <summary>
        /// type=4 的时候，存放的语音消息的长度
        /// </summary>
        public int Duration { get; set; }
        /// <summary>
        /// 如果类型为3，则表示存放的图片的集合，中间用分号隔开
        /// </summary>
        public string ImgList { get; set; }

        /// <summary>
        /// 存放移动端的id
        /// </summary>
        public string id1 { get; set; }
        /// <summary>
        /// 上传到HFS后生成的filecode 
        /// </summary>
        public string NewFileName { get; set; }
        /// <summary>
        /// 是否已经被删除
        /// </summary>
        public int IsDel { get; set; }

        /// <summary>
        /// 该消息是否是通知消息  2019年11月5日新增
        /// 暂时不用了，判断回执类消息，使用Type=="8" 来判断
        /// </summary>
        public bool IsNotice { get; set; }
        public string NoticeCreateData { get; set; }
        /// <summary>
        /// 2020年2月3日新增 消息主题ID 若为空则表示为普通聊天消息
        /// </summary>
        public string SubjectId { get; set; }

        /// <summary>
        /// 点赞数
        /// </summary>
        public int ThumbCount { get; set; }

    }
}