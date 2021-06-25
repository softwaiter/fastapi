﻿using CodeM.Common.Orm;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class ModulePermissionService
    {
        public static List<dynamic> GetEffectiveListByPermission(string permissionCode)
        {
            List<dynamic> result = OrmUtils.Model("ModulePermission")
                .Equals("Permission", permissionCode)
                .Equals("Module.Actived", true)
                .Equals("Module.Deleted", false)
                .Query();
            return result;
        }

        public static List<dynamic> GetEffectiveListByProductAndPermission(string prodCode, string permissionCode)
        {
            List<dynamic> result = OrmUtils.Model("ModulePermission")
                .Equals("Module.Product", prodCode)
                .Equals("Permission", permissionCode)
                .Equals("Module.Actived", true)
                .Equals("Module.Deleted", false)
                .Query();
            return result;
        }

        public static long GetEffectiveCountByProductAndPermission(string prodCode, string permissionCode)
        {
            long result = OrmUtils.Model("ModulePermission")
                .Equals("Module.Product", prodCode)
                .Equals("Permission", permissionCode)
                .Equals("Module.Actived", true)
                .Equals("Module.Deleted", false)
                .Count();
            return result;
        }
    }
}