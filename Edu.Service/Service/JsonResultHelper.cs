using Edu.Models.Models;

namespace Edu.Service.Service
{
    public static class JsonResultHelper
    {
        public static JsonResultModel CreateJson(object data, bool success = true, string msg = null)
        {
            return new JsonResultModel { code = success ? JsonResultType.Success : JsonResultType.Failed, data = data, msg = msg };
        }

    }
}
