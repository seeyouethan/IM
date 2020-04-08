using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Edu.Models.Models;
using Newtonsoft.Json;
using Edu.Tools;
using log4net;

namespace EduIM.WebAPI.Filters
{
    public class GlobalActionFilterAttribute : ActionFilterAttribute
    {
        readonly ILog _log = LogManager.GetLogger(typeof(GlobalActionFilterAttribute));
        private int _runTime;

        private string _actionName;

        private string _controllerName;

        private string _serverIp;

        private string _clientIp;

        private readonly Stopwatch _stopwatch = new Stopwatch();
        public bool IsCheck { get; set; }//此属性为一个bool值，true表示验证Cookies,false表示不验证
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (!SkipAuthorize(filterContext))
            {
                _stopwatch.Start();

                var message = "";
                var ignore_identity = false;
                bool isEffective = true;
                if (HttpContext.Current.Request.Headers.Get("ignore-identity") == "true") {
                    isEffective = true;
                }
                else
                {
                    //存在令牌时,向授权服务器验证令牌有效性
                    var accessToken = HttpContext.Current.Request.Headers.Get("accesstoken");
                    isEffective = Authorize(accessToken, out message);
                }
                
                if (isEffective)
                {
                    _actionName = filterContext.ActionDescriptor.ActionName;
                    _controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                    _clientIp = HttpContext.Current.Request.ServerVariables.Get("REMOTE_ADDR");
                    _serverIp = HttpContext.Current.Request.ServerVariables.Get("Local_Addr");
                }
                else
                {
                    //没有访问牌,拒绝访问
                    if (string.IsNullOrEmpty(message))
                    {
                        message = "-1009";//验证失败,无效的accessToken!
                    }
                    filterContext.Response = filterContext.Request.CreateResponse<AppResult>(new AppResult(false, string.Empty, message, 0, 0));
                }
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext filterContext)
        {
            _stopwatch.Stop();
            _runTime = (int)_stopwatch.ElapsedMilliseconds;
            _stopwatch.Reset();
            //记录调用日志 accessToken
            _log.Info("-----------------------------------------------");
            _log.Info("【" + _clientIp + "】访问了" + "【" + _serverIp + "】");
            _log.Info("控制器为【" + _controllerName + "】访问了" + "方法为【" + _actionName + "】");
            _log.Info("耗时【" + _runTime + "】毫秒");
        }

        /// <summary>
        /// 验证access_token是否合法
        /// </summary>
        private bool Authorize(string accessToken, out string message)
        {
            message = "";
            if (string.IsNullOrEmpty(accessToken))
            {
                message = "-1009";//accesstoken is Empty！
                return false;
            }
            string authorizationServer = ConfigurationManager.AppSettings.Get("authorize_server");
            IList<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("access_token",accessToken)
            };
            try
            {
                string resultJson = new HttpClient().PostAsync(authorizationServer + "/oauth/GetTokenInfo", new FormUrlEncodedContent(param)).Result.Content.ReadAsStringAsync().Result;
                Knetoauthresult<AccessToken> tokenInfo = JsonConvert.DeserializeObject<Knetoauthresult<AccessToken>>(resultJson);

                if (tokenInfo.Success)
                {
                    return tokenInfo.Content.IsEffective;
                }

                message = tokenInfo.Message;
                return tokenInfo.Success;
            }
            catch (Exception ex)
            {
                message = "-1009";
                return false;
            }
        }



        /// <summary>
        /// 判断控制器和Action是否要进行拦截（通过判断是否有NoLogAttribute过滤器来验证）
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        private static bool SkipAuthorize(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor.GetCustomAttributes<NoAccessTokenAttribute>().Any() ||
                   actionContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<NoAccessTokenAttribute>().Any();
        }
    }

    /// <summary>
    /// 不需要accesstoken的
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
    public class NoAccessTokenAttribute : Attribute
    {
    }
}