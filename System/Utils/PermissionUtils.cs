﻿using CodeM.FastApi.Services;
using CSScriptLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using System;
using System.Collections.Generic;

namespace CodeM.FastApi.System.Utils
{
    public class PermissionUtils
    {
        /// <summary>
        /// 数据权限参数值表达式
        /// </summary>
        private static Dictionary<string, dynamic> sPermissionDataParamValueExprs = new Dictionary<string, dynamic>();

        private class Permission
        {
            public TemplateMatcher Matcher { get; set; }

            public dynamic Settings { get; set; }
        }

        private static Dictionary<string, List<Permission>> sPermissions = new Dictionary<string, List<Permission>>();

        private static string[] sMethods = new string[] { "GET", "POST", "PUT", "DELETE" };

        public static void Load()
        {
            List<dynamic> permissions = PermissionService.GetEffectiveList();
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

            Dictionary<string, dynamic> _temp2 = new Dictionary<string, dynamic>();
            List<dynamic> permissionDataParams = PermissionDataParamService.GetListWithActivedPermissionData();
            permissionDataParams.ForEach(item =>
            {
                dynamic caller = CSScript.Evaluator.LoadMethod(@"
                                                         public string Execute(dynamic source, dynamic global) {" +
                                                            string.Concat("return ", item.Value, ";") +
                                                         "}");
                string key = GetDataPermissionParamKey(item);
                _temp2.Add(key, caller);
            });

            sPermissions = _temp;
            sPermissionDataParamValueExprs = _temp2;
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

        public static bool HasPermission(string userCode, string platform, string permissionCode)
        {
            List<dynamic> modulePermissions = ModulePermissionService.GetEffectiveListByProductAndPermission(platform, permissionCode);
            List<string> moduleCodes = new List<string>();
            modulePermissions.ForEach(item =>
            {
                moduleCodes.Add(item.Module);
            });

            long count = 0;

            if (moduleCodes.Count > 0)
            {
                count = UserModuleService.GetUserModuleCount(userCode, moduleCodes.ToArray());
                if (count == 0)
                {
                    List<dynamic> roleModules = RoleModuleService.GetEffectiveListByModule(moduleCodes.ToArray());
                    List<string> roleCodes = new List<string>();
                    roleModules.ForEach(item =>
                    {
                        roleCodes.Add(item.Role);
                    });

                    if (roleCodes.Count > 0)
                    {
                        count = UserRoleService.GetUserRoleCount(userCode, roleCodes.ToArray());
                    }
                }
            }

            return count > 0;
        }

        private static string GetDataPermissionParamKey(dynamic item)
        {
            return string.Concat(item.PermissionData, "_", item.Name);
        }

        public static dynamic ExecDataPermissionParamValue(string key)
        {
            dynamic value;
            if (sPermissionDataParamValueExprs.TryGetValue(key, out value))
            {
                return value;
            }
            throw new Exception(string.Concat("参数表达式未找到: ", key));
        }

        public static dynamic ExecDataPermissionParamValue(dynamic item)
        {
            string key = GetDataPermissionParamKey(item);
            return ExecDataPermissionParamValue(key);
        }

    }
}
