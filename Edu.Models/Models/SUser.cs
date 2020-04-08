using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models.Models
{
    public class SUser
    {
        public string id { get; set; }
        public string pid { get; set; }
        public string RealName { get; set; }
        public string userName { get; set; }
        public string department { get; set; }
        public int type { get; set; }
        public string icon { get; set; }
        /// <summary>
        /// 全拼
        /// </summary>
        public string PinYinFull { get; set; }
        /// <summary>
        /// 拼音首字母
        /// </summary>
        public string PinYinSimple { get; set; }
        /// <summary>
        /// 使用可用，20190227新增
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// 2019年3月18日 新增 是否是好友关系
        /// 2019年3月27日 去掉这个功能
        /// </summary>
        //public bool IsFriend { get; set; }
        /// <summary>
        /// 2019年3月18日 新增 电话号码
        /// </summary>
        public string Mobile { get; set; }
        
    }
}
