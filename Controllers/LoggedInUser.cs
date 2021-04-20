﻿using CodeM.FastApi.Context;
using CodeM.FastApi.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controllers
{
    public class LoggedInUser
    {
        public async Task Handle(ControllerContext cc)
        {
            string token = cc.Headers.Get("token", string.Empty);
            if (!string.IsNullOrWhiteSpace(token))
            {
                string platform = cc.Headers.Get("platform", string.Empty);
                if (!string.IsNullOrWhiteSpace(platform))
                {
                    string userCode = cc.Session.GetString("code");
                    if (!string.IsNullOrWhiteSpace(userCode))
                    {
                        dynamic loggedInUser = UserService.GetUserByCode(userCode);

                        List<dynamic> userRoles = UserRoleService.GetListByUser(userCode);

                        List<object> roleCodes = new List<object>();
                        foreach (dynamic userRole in userRoles)
                        {
                            roleCodes.Add(userRole.Role);
                        }

                        List<string> moduleCodes = new List<string>();

                        List<dynamic> roleModules = RoleModuleService.GetListByRole(roleCodes.ToArray());
                        foreach (dynamic roleModule in roleModules)
                        {
                            if (!moduleCodes.Contains(roleModule.Module))
                            {
                                moduleCodes.Add(roleModule.Module);
                            }
                        }

                        List<dynamic> userModules = UserModuleService.GetListByUserProduct(userCode, platform);
                        foreach (dynamic userModule in userModules)
                        {
                            if (!moduleCodes.Contains(userModule.Module))
                            {
                                moduleCodes.Add(userModule.Module);
                            }
                        }

                        object result = new
                        {
                            user = new
                            {
                                Code = loggedInUser.Code,
                                Name = loggedInUser.Name,
                                MustChangePassNow = loggedInUser.MustChangePassNow
                            },
                            modules = moduleCodes.ToArray()
                        };
                        await cc.JsonAsync(result);
                    }
                    else
                    {
                        await cc.JsonAsync(-1003, null, "获取当前登录用户信息失败。");
                    }
                }
                else
                {
                    await cc.JsonAsync(-1002, null, "获取当前登录用户信息失败。");
                }
                return;
            }

            await cc.JsonAsync(-1001, null, "获取当前登录用户信息失败。");
        }
    }
}