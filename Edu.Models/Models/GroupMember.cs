using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models.Models
{
    /// <summary>
    /// 弹出选择成员界面的成员Model
    /// </summary>
    public class GroupMember
    {
        public string department { get; set; }
        public string logo { get; set; }
        public string pId { get; set; }
        public string realName { get; set; }
        public string type { get; set; }
        public string userId { get; set; }
        public int userType { get; set; }
    }
}
