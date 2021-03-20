using CodeM.Common.Orm;

namespace CodeM.FastApi.Services
{
    public class OrgService
    {
        public static dynamic GetOrgByCode(string code)
        { 
            return OrmUtils.Model("Organization").Equals("Code", code).QueryFirst();
        }
    }
}
