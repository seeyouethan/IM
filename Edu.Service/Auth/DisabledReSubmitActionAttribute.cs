using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Edu.Service
{
    /// <summary>
    /// 防止重复提交过滤器
    /// </summary>
    /// <remarks>
    /// 跟踪的顺序
    /// OnActionExecuting
    /// OnActionExecuted
    /// OnResultExecuting
    /// OnResultExecuted
    /// </remarks>
    public class DisabledReSubmitActionAttribute : ActionFilterAttribute
    {
        // 不允许重复提交时间间隔
        private int m_ReSubmitSeconds = 1;
        /// <summary>
        /// 构建方法
        /// </summary>
        /// <param name="reSubmitSeconds">不允许重复提交的时间间隔：秒</param>
        public DisabledReSubmitActionAttribute(int reSubmitSeconds)
        {
            m_ReSubmitSeconds = reSubmitSeconds;
        }
        public DisabledReSubmitActionAttribute()
        {
        }

        /// <summary>
        /// 在controller action执行之前调用 
        /// </summary>
        /// <param name="filterContext">controller action内容</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            //第一次加载
            if (session["lastSubmitTime"] == null)
            {
                session["lastSubmitTime"] = DateTime.Now.AddSeconds(-100);
            }
            //计算当前时间和上次提交的时间差
            TimeSpan ts = DateTime.Now - (DateTime)session["lastSubmitTime"];
            //n 秒内不允许重复提交
            if (ts.TotalSeconds < m_ReSubmitSeconds)
            {
                throw new Exception("请勿重复提交");
                // 抛出重复提交异常
            }
            else
            {
                //更新保存的时间值
                session["lastSubmitTime"] = DateTime.Now;
                // 继续执行Action
                base.OnActionExecuting(filterContext);
            }
        }

    }
}