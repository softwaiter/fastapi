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
                .Query();
            return result;
        }

        public static List<dynamic> GetEffectiveListByModule(params string[] moduleCodes)
        {
            List<dynamic> result = OrmUtils.Model("RoleModule")
                .In("Module", moduleCodes)
                .Equals("Role.Actived", true)
                .Equals("Role.Deleted", false)
                .Query();
            return result;
        }
    }
}
