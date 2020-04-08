using System;

namespace Edu.SignalRServer.Service
{
    public class LoggerHelper
    {
        private static readonly log4net.ILog Loginfo = log4net.LogManager.GetLogger("loginfo");
        private static readonly log4net.ILog Logerror = log4net.LogManager.GetLogger("logerror");
        private static readonly log4net.ILog Logmonitor = log4net.LogManager.GetLogger("logmonitor");
        private static readonly log4net.ILog LogmonitorOa = log4net.LogManager.GetLogger("logmonitorOA");
        private static readonly log4net.ILog AndroidPushLogger = log4net.LogManager.GetLogger("AndroidPush");
        private static readonly log4net.ILog IOSPushLogger = log4net.LogManager.GetLogger("IOSPush");

        public static void Error(string errorMsg, Exception ex = null)
        {
            if (ex != null)
            {
                Logerror.Error(errorMsg, ex);
            }
            else
            {
                Logerror.Error(errorMsg);
            }
        }

        public static void Info(string msg)
        {
            Loginfo.Info(msg);
        }

        public static void Monitor(string msg)
        {
            Logmonitor.Info(msg);
        }

        public static void MonitorOa(string msg)
        {
            LogmonitorOa.Info(msg);
        }

        public static void AndroidPush(string msg)
        {
            AndroidPushLogger.Info(msg);
        }

        public static void IOSPush(string msg)
        {
            IOSPushLogger.Info(msg);
        }
    }
    
}