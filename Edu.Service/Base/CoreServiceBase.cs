using Edu.Data;
using Edu.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Service
{
    public class CoreServiceBase
    {
        /// <summary>
        /// 获取或设置 工作单元对象，用于处理同步业务的事务操作
        /// </summary>
        protected UnitOfWork unitOfWork { get; set; }
        public CoreServiceBase()
        {
            unitOfWork = new UnitOfWork();
    
        }
    }
}
