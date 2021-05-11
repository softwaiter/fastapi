using CodeM.Common.Orm;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class ProxyService
    {
        public static List<dynamic> GetEffectiveListWithoutLaodan()
        {
            List<dynamic> result = OrmUtils.Model("Proxy")
                .NotEquals("Code", "s_laodan")
                .Equals("Actived", true)
                .Equals("Deleted", false)
                .Query();
            return result;
        }
    }
}
