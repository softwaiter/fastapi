using CodeM.FastApi.Context;
using CodeM.FastApi.Services;
using CodeM.FastApi.System.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controllers
{
    public class LoggedInUser
    {
        public async Task Handle(ControllerContext cc)
        {
            string userCode = LoginUtils.GetLoginUserCode(cc);
            if (!string.IsNullOrWhiteSpace(userCode))
            {
                dynamic loggedInUser = UserService.GetUserByCode(userCode);
                if (loggedInUser != null)
                {
                    string error;
                    string platform = cc.Headers.Get("Platform", string.Empty);
                    if (LoginUtils.CheckUserValidity(loggedInUser, platform, out error))
                    {
                        List<dynamic> userRoles = UserRoleService.GetEffectiveListByUserAndProduct(userCode, platform);

                        List<object> roleCodes = new List<object>();
                        foreach (dynamic userRole in userRoles)
                        {
                            roleCodes.Add(userRole.Role);
                        }

                        List<string> moduleCodes = new List<string>();

                        List<dynamic> roleModules = RoleModuleService.GetEffectiveListByRole(roleCodes.ToArray());
                        foreach (dynamic roleModule in roleModules)
                        {
                            if (!moduleCodes.Contains(roleModule.Module))
                            {
                                moduleCodes.Add(roleModule.Module);
                            }
                        }

                        List<dynamic> userModules = UserModuleService.GetEffectiveListByUserAndProduct(userCode, platform);
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
                        await cc.JsonAsync(1003, null, "获取当前登录用户信息失败。");
                    }
                }
                else
                {
                    await cc.JsonAsync(1002, null, "获取当前登录用户信息失败。");
                }
            }
            else
            {
                await cc.JsonAsync(1001, null, "获取当前登录用户信息失败。");
            }
        }
    }
}
