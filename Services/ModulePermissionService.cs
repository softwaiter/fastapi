using CodeM.Common.Orm;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class ModulePermissionService
    {
        public static List<dynamic> GetEffectiveListByPermission(string permissionCode)
        {
            List<dynamic> result = OrmUtils.Model("ModulePermission")
                .Equals("Permission", permissionCode)
                .Equals("Module.Actived", true)
                .Equals("Module.Deleted", false)
                .Query();
            return result;
        }
    }
}
