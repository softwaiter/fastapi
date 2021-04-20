using CodeM.Common.Orm;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class RoleModuleService
    {
        public static List<dynamic> GetListByRole(params object[] roleCodes)
        {
            List<dynamic> result = OrmUtils.Model("RoleModule").In("Role", roleCodes).Query();
            return result;
        }
    }
}
