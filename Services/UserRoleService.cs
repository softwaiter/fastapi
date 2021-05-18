using CodeM.Common.Orm;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class UserRoleService
    {
        public static List<dynamic> GetEffectiveListByUserAndProduct(string userCode, string prodCode)
        {
            List<dynamic> result = OrmUtils.Model("UserRole")
                .Equals("User", userCode)
                .Equals("Role.Product", prodCode)
                .Equals("Role.Actived", true)
                .Equals("Role.Deleted", false)
                .Query();
            return result;
        }

        public static dynamic GetFirstEffectiveRole(string userCode, string prodCode)
        {
            dynamic result = OrmUtils.Model("UserRole")
                .Equals("User", userCode)
                .Equals("Role.Product", prodCode)
                .Equals("Role.Actived", true)
                .Equals("Role.Deleted", false)
                .QueryFirst();
            return result;
        }

        public static bool HasActivedRole(string userCode, string roleCode)
        {
            long count = OrmUtils.Model("UserRole")
                .Equals("User", userCode)
                .Equals("Role", roleCode)
                .Equals("Role.Actived", true)
                .Equals("Role.Deleted", false)
                .Count();
            return count > 0;
        }

        public static long GetUserRoleCount(string userCode, params string[] roleCodes)
        {
            long count = OrmUtils.Model("UserRole")
                .Equals("User", userCode)
                .In("Role", roleCodes)
                .Count();
            return count;
        }
    }
}
