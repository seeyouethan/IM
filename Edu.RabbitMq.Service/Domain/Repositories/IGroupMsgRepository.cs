using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKCS.PMC.MsgContracts;

namespace Edu.RabbitMq.Service.Domain.Repositories
{
    public interface IGroupMsgRepository
    {
        Task<bool> BetchAdd(IEnumerable<GroupMsg> pointContributionRecordList);

        int Add(object condition);

        int Delete(object condition);

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        IEnumerable<GroupMsg> Get(object condition);
    }
}
