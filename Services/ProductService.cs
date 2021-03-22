using CodeM.Common.Orm;

namespace CodeM.FastApi.Services
{
    public class ProductService
    {
        public static dynamic GetProductByCode(string code)
        {
            return OrmUtils.Model("Product").Equals("Code", code).QueryFirst();
        }
    }
}
