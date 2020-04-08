using System;
using System.Configuration;
using MassTransit;

namespace EduIM.WebAPI.Service
{
    public static class MassTransitConfig
    {
        static IBusControl _bus;
        static BusHandle _busHandle;

        public static IBus Bus
        {
            get { return _bus; }
        }

        public static void MassTransit_Start()
        {
            _bus = MassTransit.Bus.Factory.CreateUsingRabbitMq(x =>
            {
                var host = x.Host(new Uri(ConfigurationManager.AppSettings["RabbitMQHost"]), h =>
                {
                    h.Username(ConfigurationManager.AppSettings["RabbitMQUsername"]);
                    h.Password(ConfigurationManager.AppSettings["RabbitMQPassword"]);
                });


            });
            _busHandle = _bus.Start();
        }

        /// <summary>
        /// 向指定队列发送消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queue"></param>
        public static void SendMessage(object message, string queue)
        {
            var sendEndPoint = _bus.GetSendEndpoint(new Uri(ConfigurationManager.AppSettings["RabbitMQHost"] + queue)).Result;

            sendEndPoint.Send(message);
        }

        /// <summary>
        /// 发送广播消息
        /// </summary>
        /// <param name="message"></param>
        public static void PublishMessage(object message)
        {
            _bus.Publish(message);
        }

        public static void MassTransit_Stop()
        {
            if (_bus != null)
                _busHandle.Stop(TimeSpan.FromSeconds(30));
        }

        public static Uri GetAppQueueHostAddress()
        {
            return new Uri(ConfigurationManager.AppSettings["RabbitMQHost"] + ConfigurationManager.AppSettings["AppMsgQueueName"]);
        }

    }
}
