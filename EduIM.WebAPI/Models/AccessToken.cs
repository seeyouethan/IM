using System;

namespace EduIM.WebAPI.Models
{
    /// <summary>
    /// access_token信息
    /// 资源服务器查询access_token状态时的返回结果
    /// </summary>
    public class AccessToken
    {
        /// <summary>
        /// access_token内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 客户端id
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 用户名(Sign模式使用)
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateAt { set; get; }

        /// <summary>
        /// 有效期(秒)
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// refresh_token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// refresh_token有效期
        /// </summary>
        public int RefreshTokenExpire { get; set; }

        /// <summary>
        /// 权限范围
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsEffective { set; get; }
    }
}