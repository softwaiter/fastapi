using CodeM.FastApi.Services;

namespace CodeM.FastApi.System.Runtime
{
    public class Utils
    {
        public static dynamic GetUserById(string id)
        {
            return UserService.GetUserById(id);
        }

        public static dynamic GetUserByCode(string code)
        {
            return UserService.GetUserByCode(code);
        }

        public static dynamic GetOrgById(string id)
        {
            return OrgService.GetOrgById(id);
        }

        public static dynamic GetOrgByCode(string code)
        {
            return OrgService.GetOrgByCode(code);
        }

        public static dynamic GetUserProductById(string id)
        {
            return UserProdService.GetUserProdById(id);
        }
    }
}
