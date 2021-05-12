using CodeM.FastApi.Config;
using CodeM.FastApi.Context;
using CodeM.FastApi.Services;
using CodeM.FastApi.System.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace CodeM.FastApi.System.Middlewares
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private ApplicationConfig mConfig;

        public PermissionMiddleware(RequestDelegate next, ApplicationConfig config)
        {
            _next = next;
            mConfig = config;
        }

        public async Task Invoke(HttpContext context)
        {
            ControllerContext cc = ControllerContext.FromHttpContext(context, mConfig);

            dynamic permission = PermissionUtils.GetPermission(cc.Request);
            if (permission != null)
            {
                //处理即时令牌
                if (permission.SupportLoginToken)
                {
                }

                //是否需要登录
                if (permission.RequireLogin)
                {
                    string userCode = LoginUtils.GetLoginUserCode(cc);
                    if (string.IsNullOrWhiteSpace(userCode))
                    {
                        await cc.JsonAsync(2001, null, "请先登录。");
                        return;
                    }

                    //用户是否存在
                    dynamic userObj = UserService.GetUserByCode(userCode);
                    if (userObj == null)
                    {
                        await cc.JsonAsync(1002, null, "获取当前登录用户信息失败。");
                        return;
                    }

                    if (userObj.MustChangePassNow && 
                        !("/login/user".Equals(cc.Request.Path, StringComparison.OrdinalIgnoreCase) ||
                        "/changepass".Equals(cc.Request.Path, StringComparison.OrdinalIgnoreCase)))
                    {
                        await cc.JsonAsync(2002, null, "请修改登录密码，再继续操作。");
                        return;
                    }

                    //用户是否合法
                    string error;
                    string platform = cc.Headers.Get("Platform", string.Empty);
                    if (!LoginUtils.CheckUserValidity(userObj, platform, out error))
                    {
                        await cc.JsonAsync(1003, null, error);
                        return;
                    }

                    //用户是否拥有权限
                    if (!PermissionUtils.HasPermission(userCode, platform, permission.Code))
                    {
                        cc.State = 401;
                        return;
                    }
                }

                await _next(context);
            }
            else
            {
                cc.State = 404;
            }
        }
    }
}