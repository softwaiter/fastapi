using CodeM.Common.Orm;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class UserRoleService
    {
        public static List<dynamic> GetListByUser(string userCode)
        {
            List<dynamic> result = OrmUtils.Model("UserRole").Equals("User", userCode).Query();
            return result;
        }
    }
}
