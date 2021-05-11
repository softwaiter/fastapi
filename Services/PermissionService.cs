using CodeM.Common.Orm;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class PermissionService
    {
        public static List<dynamic> GetEffectiveList()
        {
            List<dynamic> result = OrmUtils.Model("Permission")
                .Equals("Actived", true)
                .Equals("Deleted", false)
                .Query();
            return result;
        }
    }
}
