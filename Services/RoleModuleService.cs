using CodeM.Common.Orm;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class RoleModuleService
    {
        public static List<dynamic> GetEffectiveListByRole(params object[] roleCodes)
        {
            List<dynamic> result = OrmUtils.Model("RoleModule")
                .In("Role", roleCodes)
                .Equals("Module.Actived", true)
                .Equals("Module.Deleted", false)
                .GetValue("Id", "Role", "CreateTime", "UpdateTime", "Module.Id", "Module.Code", "Module.Name")
                .Query();
            return result;
        }

        public static List<dynamic> GetEffectiveListByModule(params string[] moduleCodes)
        {
            List<dynamic> result = OrmUtils.Model("RoleModule")
                .In("Module", moduleCodes)
                .Equals("Role.Actived", true)
                .Equals("Role.Deleted", false)
                .GetValue("Id", "Module", "CreateTime", "UpdateTime", "Role.Id", "Role.Code", "Role.Name")
                .Query();
            return result;
        }
    }
}
