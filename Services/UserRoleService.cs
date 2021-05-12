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
