using CodeM.Common.Orm;

namespace CodeM.FastApi.Services
{
    public class OrgProdService
    {
        public static dynamic GetOrgProdByCode(string orgCode, string prodCode)
        {
            return OrmUtils.Model("OrgProduct").Equals("Org", orgCode).Equals("Product", prodCode).QueryFirst();
        }
    }
}
