using CodeM.Common.Orm;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using System.Collections.Generic;

namespace CodeM.FastApi.System.Utils
{
    public class PermissionUtils
    {
        private class Permission
        {
            public TemplateMatcher Matcher { get; set; }

            public dynamic Settings { get; set; }
        }

        private static Dictionary<string, List<Permission>> sPermissions = new Dictionary<string, List<Permission>>();

        private static string[] sMethods = new string[] { "GET", "POST", "PUT", "DELETE" };

        public static void Load()
        {
            List<dynamic> permissions = OrmUtils.Model("Permission").Equals("Actived", true).Equals("Deleted", false).Query();
            permissions.Sort((left, right) =>
            {
                if (left.Path.Length > right.Path.Length)
                {
                    return -1;
                }
                else if (left.Path.Length < right.Path.Length)
                {
                    return 1;
                }
                return 0;
            });

            Dictionary<string, List<Permission>> _temp = new Dictionary<string, List<Permission>>();

            permissions.ForEach((item) =>
            {
                string key = sMethods[item.Method];
                if (!_temp.ContainsKey(key))
                {
                    _temp.Add(key, new List<Permission>());
                }

                RoutePattern pattern = RoutePatternFactory.Parse(item.Path);
                RouteTemplate template = new RouteTemplate(pattern);
                TemplateMatcher matcher = new TemplateMatcher(template, new RouteValueDictionary());
                _temp[key].Add(new Permission
                {
                    Matcher = matcher,
                    Settings = item
                });
            });

            sPermissions = _temp;
        }

        public static dynamic GetPermission(HttpRequest req)
        {
            dynamic result = null;
            if (sPermissions.ContainsKey(req.Method))
            {
                RouteValueDictionary emptyValues = new RouteValueDictionary();
                sPermissions[req.Method].ForEach((item) =>
                {
                    if (item.Matcher.TryMatch(req.Path, emptyValues))
                    {
                        result = item.Settings;
                        return;
                    }
                });
            }
            return result;
        }

        public static bool HasPermission(string userCode, string permissionCode)
        {
            List<dynamic> modulePermissions = OrmUtils.Model("ModulePermission")
                .Equals("Permission", permissionCode)
                .Equals("Module.Actived", true)
                .Equals("Module.Deleted", false)
                .Query();
            List<string> moduleCodes = new List<string>();
            modulePermissions.ForEach(item =>
            {
                moduleCodes.Add(item.Module);
            });

            long count = 0;

            if (moduleCodes.Count > 0)
            {
                count = OrmUtils.Model("UserModule")
                    .Equals("User", userCode)
                    .In("Module", moduleCodes.ToArray())
                    .Count();
                if (count == 0)
                {
                    List<dynamic> roleModules = OrmUtils.Model("RoleModule")
                        .In("Module", moduleCodes.ToArray())
                        .Equals("Role.Actived", true)
                        .Equals("Role.Deleted", false)
                        .Query();
                    List<string> roleCodes = new List<string>();
                    roleModules.ForEach(item =>
                    {
                        roleCodes.Add(item.Role);
                    });

                    if (roleCodes.Count > 0)
                    {
                        count = OrmUtils.Model("UserRole")
                            .Equals("User", userCode)
                            .In("Role", roleCodes.ToArray())
                            .Count();
                    }
                }
            }

            return count > 0;
        }

    }
}
