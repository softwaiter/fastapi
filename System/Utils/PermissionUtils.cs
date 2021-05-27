using CodeM.FastApi.Context;
using CodeM.FastApi.Services;
using CSScriptLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using System;
using System.Collections.Generic;
//using CodeM.Common.Orm.Serialize.ModelObject;

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

        private static Dictionary<string, dynamic> sPathPermissionCaches = new Dictionary<string, dynamic>();


        /// <summary>
        /// 数据权限规则判断函数
        /// </summary>
        private static Dictionary<string, dynamic> sPermissionDataRules = new Dictionary<string, dynamic>();

        /// <summary>
        /// 数据权限参数值表达式
        /// </summary>
        private static Dictionary<string, dynamic> sPermissionDataParamValueExprs = new Dictionary<string, dynamic>();

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
            List<dynamic> permissionDatas = PermissionDataService.GetEffectiveList();
            permissionDatas.ForEach(item =>
            {
                if (!string.IsNullOrWhiteSpace(item.CheckRules))
                {
                    dynamic expr = CSScript.Evaluator.LoadMethod(@"
                                                        using CodeM.FastApi.Context;
                                                        public bool Check(ControllerContext cc, dynamic globalData) {" +
                                                        item.CheckRules +
                                                        "}");
                    _temp2.Add(item.UnionIdent, expr);
                }
            });

            Dictionary<string, dynamic> _temp3 = new Dictionary<string, dynamic>();
            List<dynamic> permissionDataParams = PermissionDataParamService.GetListWithActivedPermissionData();
            permissionDataParams.ForEach(item =>
            {
                if (!string.IsNullOrWhiteSpace(item.Value))
                {
                    dynamic expr = CSScript.Evaluator.LoadMethod(@"
                                                         public string Call(dynamic oldData, dynamic globalData) {" +
                                                                string.Concat("return ", item.Value, ";") +
                                                             "}");
                    string key = GetDataPermissionParamKey(item);
                    _temp3.Add(key, expr);
                }
            });

            sPermissions = _temp;
            sPermissionDataRules = _temp2;
            sPermissionDataParamValueExprs = _temp3;

            sPathPermissionCaches = new Dictionary<string, dynamic>();
        }

        public static void Reload()
        {
            Load();
        }

        public static dynamic GetPermission(HttpRequest req)
        {
            string key = string.Concat(req.Method, "_", req.Path);
            if (sPathPermissionCaches.ContainsKey(key))
            {
                return sPathPermissionCaches[key];
            }

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

            sPathPermissionCaches[key] = result;
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
            return GetDataPermissionParamKey(item.PermissionData, item.Name);
        }

        private static string GetDataPermissionParamKey(string permissionDataCode,
            string permissionDataParamName)
        {
            return string.Concat(permissionDataCode, "_", permissionDataParamName);
        }

        public static bool CheckPermissionDataRule(string pdUnionIdent,
            ControllerContext cc, dynamic globalData)
        {
            dynamic expr;
            if (sPermissionDataRules.TryGetValue(pdUnionIdent, out expr))
            {
                return expr.Check(cc, globalData);
            }
            return false;
        }

        public static dynamic ExecDataPermissionParamValue(string permissionDataCode, 
            string permissionDataParamName, dynamic oldData, dynamic globalData)
        {
            string key = GetDataPermissionParamKey(permissionDataCode, permissionDataParamName);
            dynamic expr;
            if (sPermissionDataParamValueExprs.TryGetValue(key, out expr))
            {
                return expr.Call(oldData, globalData);
            }
            throw new Exception(string.Concat("参数表达式未找到: ", key));
        }

    }
}
