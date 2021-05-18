using CodeM.Common.Orm;

namespace CodeM.FastApi.Services
{
    public class PermissionDataService
    {
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
