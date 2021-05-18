using CodeM.Common.Orm;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class PermissionDataParamService
    {
        public static List<dynamic> GetListWithActivedPermissionData()
        {
            List<dynamic> result = OrmUtils.Model("PermissionDataParam")
                .Equals("PermissionData.Actived", true)
                .Equals("PermissionData.Deleted", false)
                .Query();
            return result;
        }

        public static List<dynamic> GetListByPermissionData(params string[] codes)
        {
            List<dynamic> result = OrmUtils.Model("PermissionDataParam")
                .In("PermissionData", codes)
                .Query();
            return result;
        }
    }
}
