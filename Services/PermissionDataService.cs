using CodeM.Common.Orm;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class PermissionDataService
    {
        public static List<dynamic> GetEffectiveList()
        {
            List<dynamic> result = OrmUtils.Model("PermissionData")
                .Equals("Actived", true)
                .Equals("Deleted", false)
                .Query();
            return result;
        }

        public static dynamic GetEffectiveFirstByUnionIdent(params string[] idents)
        {
            dynamic result = OrmUtils.Model("PermissionData")
                .In("UnionIdent", idents)
                .Equals("Actived", true)
                .Equals("Deleted", false)
                .DescendingSort("Role")
                .DescendingSort("Module")
                .QueryFirst();
            return result;
        }
    }
}
