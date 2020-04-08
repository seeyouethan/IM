using System.Web.Http;

namespace Edu.Service.Auth
{
    /// <summary>
    /// 用于Api的BaseController
    /// </summary>
    public class BaseApiController : ApiController
    {
        protected UnitOfWork unitOfWork = new UnitOfWork();
        protected ssoUser ssoUserOfWork = new ssoUser();
    }
}
