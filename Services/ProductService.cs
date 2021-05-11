using CodeM.Common.Orm;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class ProductService
    {
        public static dynamic GetProductByCode(string code)
        {
            return OrmUtils.Model("Product").Equals("Code", code).QueryFirst();
        }

        public static List<dynamic> GetBasicEffectiveList()
        {
            List<dynamic> result = OrmUtils.Model("Product")
                .Equals("IsBasic", true)
                .Equals("Actived", true)
                .Equals("Deleted", false)
                .Query();
            return result;
        }
    }
}
