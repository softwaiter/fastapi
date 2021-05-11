using CodeM.Common.Orm;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class UserModuleService
    {
        public static List<dynamic> GetListByUserProduct(string userCode, string prodCode)
        {
            List<dynamic> result = OrmUtils.Model("UserModule")
                .Equals("User", userCode)
                .Equals("Product", prodCode)
                .Query();
            return result;
        }

        public static long GetUserModuleCount(string userCode, params string[] moduleCodes)
        {
            long count = OrmUtils.Model("UserModule")
                    .Equals("User", userCode)
                    .In("Module", moduleCodes)
                    .Count();
            return count;
        }
    }
}
