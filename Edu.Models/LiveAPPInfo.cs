using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models
{
    public class LiveAPPInfo
    {
        public int ID { get; set; }//直播ID

        
        public string Name { get; set; }//直播名称

        
        public string Creator { get; set; }//直播创建人

        public string CreateDate { get; set; }//创建时间

        public string StartDate { get; set; }//开始时间

        public string EndDate { get; set; }//结束时间
        
        public string Des { get; set; }//直播描述
        
        public string CourseID { get; set; }//研讨ID

        public string Host { get; set; }//直播主持人

        
        public string Steam { get; set; }//直播串流码（手机直播的时候用这个字段，根据时间生成唯一码）     
        
        public string ImagePath { get; set; }//直播图片存放地址
        public int Status { get; set; }//直播状态 0未开始 1正在直播 2已经结束

        
        public string Livepath { get; set; }//直播地址 
        
        public string LiveUserpath { get; set; }//用户直播地址 

        public string UserID { get; set; }//


        public int JType { get; set; }//参与类型0公开 1我参与 2我创建的


        public string UserTotal { get; set; }

        public string DiscussTotal { get; set; }

        public string LengthTime { get; set; }

        public string RecordTotal { get; set; }
        public int? IsPublic { get; set; }

        public string UpdateTime { get; set; }


        public virtual ICollection<string> ListUserID { get; set; }
    }
}
