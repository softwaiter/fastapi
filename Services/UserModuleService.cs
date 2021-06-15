using CodeM.Common.Orm;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class UserModuleService
    {
        public static List<dynamic> GetEffectiveListByUserAndProduct(string userCode, string prodCode)
        {
            List<dynamic> result = OrmUtils.Model("UserModule")
                .Equals("User", userCode)
                .Equals("Product", prodCode)
                .Equals("Module.Actived", true)
                .Equals("Module.Deleted", false)
                .GetValue("Id", "User", "Product", "Module.Id", "Module.Code", "Module.Name", "CreateTime", "UpdateTime")
                .Query();
            return result;
        }

        public static List<dynamic> GetListByIds(params string[] ids)
        {
            List<dynamic> result = OrmUtils.Model("UserModule")
                .In("Id", ids)
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

        public static long GetUserModuleCount(string userCode, string prodCode, params string[] moduleCodes)
        {
            long count = OrmUtils.Model("UserModule")
                    .Equals("User", userCode)
                    .Equals("Product", prodCode)
                    .In("Module", moduleCodes)
                    .Count();
            return count;
        }
    }
}
