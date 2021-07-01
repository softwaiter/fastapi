using CodeM.Common.Orm;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class UserProdService
    {
        public static dynamic GetUserProdById(string id)
        {
            return OrmUtils.Model("UserProduct").Equals("Id", id).QueryFirst();
        }

        public static List<dynamic> GetUserProdByIds(params string[] ids)
        {
            return OrmUtils.Model("UserProduct").In("Id", ids).Query();
        }

        public static dynamic GetUserProdByCode(string userCode, string prodCode)
        {
            return OrmUtils.Model("UserProduct").Equals("User", userCode).Equals("Product", prodCode).QueryFirst();
        }

        public static long GetUserProdCountByCode(string orgCode, string prodCode)
        {
            return OrmUtils.Model("UserProduct").Equals("User.Org", orgCode).Equals("Product", prodCode).Count();
        }
    }
}
