using CodeM.Common.Orm;

namespace CodeM.FastApi.Services
{
    public class UserProdService
    {
        public static dynamic GetUserProdById(string id)
        {
            return OrmUtils.Model("UserProduct").Equals("Id", id).QueryFirst();
        }

        public static dynamic GetUserProdByCode(string userCode, string prodCode)
        {
            return OrmUtils.Model("UserProduct").Equals("User", userCode).Equals("Product", prodCode).QueryFirst();
        }
    }
}
