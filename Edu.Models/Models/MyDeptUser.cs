using System.Collections.Generic;

namespace Edu.Models.Models
{
    /// <summary>
    /// 刘静杰创建 临时类 2018年6月26日 
    /// </summary>
    public class MyDeptUser
    {
        public string id { get; set; }
        public string pid { get; set; }
        public string RealName { get; set; }
        public string department { get; set; }
        public int type { get; set; }

        public List<MyDeptUser> list { get; set; } 

    }
}
