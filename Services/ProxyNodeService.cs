using CodeM.Common.Orm;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class ProxyNodeService
    {
        public static List<dynamic> GetEffectiveListWithoutLaodan()
        {
            List<dynamic> result = OrmUtils.Model("ProxyNode")
                .NotEquals("Proxy", "s_laodan")
                .Equals("Actived", true)
                .Query();
            return result;
        }
    }
}
