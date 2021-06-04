using CodeM.FastApi.Config;
using CodeM.FastApi.Context;
using CodeM.FastApi.Services;
using CodeM.FastApi.System.Runtime;
using CodeM.FastApi.System.Utils;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeM.FastApi.System.Middlewares
{
    public class DataParamMiddleware
    {
        private readonly RequestDelegate _next;
        private ApplicationConfig mConfig;

        public DataParamMiddleware(RequestDelegate next, ApplicationConfig config)
        {
            _next = next;
            mConfig = config;
        }

        public async Task Invoke(HttpContext context)
        {
            ControllerContext cc = ControllerContext.FromHttpContext(context, mConfig);

            dynamic permission = PermissionUtils.GetPermissionSetting(cc.Request);
            if (permission != null)
            {
                string platform = cc.Headers.Get("Platform", string.Empty);
                string module = cc.Headers.Get("Module", string.Empty);
                string role = string.Empty;
                if (!string.IsNullOrWhiteSpace(module))
                {
                    string userCode = LoginUtils.GetLoginUserCode(cc);
                    if (!string.IsNullOrWhiteSpace(userCode))
                    {
                        role = cc.Headers.Get("Role", string.Empty);
                        if (!(!string.IsNullOrWhiteSpace(role) &&
                            UserRoleService.HasActivedRole(userCode, role)))
                        {
                            dynamic result = UserRoleService.GetFirstEffectiveRole(userCode, platform);
                            if (result != null)
                            {
                                role = result.Role;
                            }
                        }
                    }
                }

                List<string> unionIdents = new List<string>();
                unionIdents.Add(string.Concat(permission.Code, platform));
                if (!string.IsNullOrWhiteSpace(module))
                {
                    unionIdents.Add(string.Concat(permission.Code, platform, module));
                    if (!string.IsNullOrWhiteSpace(role))
                    {
                        unionIdents.Add(string.Concat(permission.Code, platform, module, role));
                    }
                }

                dynamic permissionData = PermissionDataService.GetEffectiveFirstByUnionIdent(unionIdents.ToArray());
                if (permissionData != null)
                {
                    if (permissionData.IsOutOfRange)
                    {
                        cc.State = 416; // 超过数据服务范围
                        return;
                    }

                    RuntimeEnvironment env = new RuntimeEnvironment(context, mConfig);

                    if (!string.IsNullOrWhiteSpace(permissionData.CheckRules))
                    {
                        if (!PermissionUtils.CheckPermissionDataRule(permissionData.UnionIdent, env))
                        {
                            cc.State = 416;
                            return;
                        }
                    }

                    List<dynamic> paramSettings = PermissionDataParamService.GetListByPermissionData(permissionData.Code);
                    paramSettings.ForEach(item =>
                    {
                        RequestParamUtils.ProcessParam(cc, item, env);
                    });
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