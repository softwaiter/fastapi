using CodeM.FastApi.Context;
using CodeM.FastApi.Services;
using CodeM.FastApi.System.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controllers
{
    public class GetUserProductModules
    {
        public async Task Handle(ControllerContext cc)
        {
            string userCode = LoginUtils.GetLoginUserCode(cc);
            string prodCode = cc.RouteParams["product"];

            List<dynamic> modules = UserModuleService.GetEffectiveListByUserAndProduct(userCode, prodCode);

            List<dynamic> userRoles = UserRoleService.GetEffectiveListByUserAndProduct(userCode, prodCode);
            List<string> roleCodes = new List<string>();
            userRoles.ForEach(item =>
            {
                roleCodes.Add(item.Role);
            });

            if (roleCodes.Count > 0)
            {
                List<dynamic> roleModules = RoleModuleService.GetEffectiveListByRole(roleCodes.ToArray());
                roleModules.ForEach(item =>
                {
                    if (modules.Find(item2 => { return item2.Module.Code == item.Module.Code; }) == null)
                    {
                        modules.Add(new
                        {
                            Id = 1,
                            User = userCode,
                            Product = prodCode,
                            Module = new {
                                Id = item.Module.Id,
                                Code = item.Module.Code,
                                Name = item.Module.Name
                            }
                        });
                    }
                });
            }

            modules.Sort((left, right) =>
            {
                if (left.Module.Id < right.Module.Id)
                {
                    return -1;
                }
                else if (left.Module.Id > right.Module.Id)
                {
                    return 1;
                }
                return 0;
            });

            await cc.JsonAsync(new { list = modules });
        }
    }
}
