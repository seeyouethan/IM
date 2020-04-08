using System.Threading.Tasks;
using Edu.RabbitMq.Service.Application;
using Edu.RabbitMq.Service.Application.Services;
using MassTransit;
using OKCS.PMC.MsgContracts;

namespace Edu.RabbitMq.Service.Consumer
{
    public class GroupMsgConsumer : IConsumer<GroupMsg>
    {
        private readonly IGroupMsgService _iGroupMsgService;

        public GroupMsgConsumer()
        {
            this._iGroupMsgService = new GroupMsgService();
        }
        public Task Consume(ConsumeContext<GroupMsg> context)
        {
            return Task.Run(() =>
            {
                if (context.Message != null)
                {
                    var groupMsg = new GroupMsg()
                    {
                        ID = context.Message.ID,
                        Content=context.Message.Content,
                        Creator=context.Message.Creator,
                        JumpId = context.Message.JumpId,
                        GroupId = context.Message.GroupId,
                        PostTime = context.Message.PostTime,
                        Type = context.Message.Type
                    };
                    //这里执行操作、执行获取到的groupMsg

                }
            });
        }
    }
}
