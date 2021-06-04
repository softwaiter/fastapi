using CodeM.FastApi.Context;
using CodeM.FastApi.Services;
using System;

namespace CodeM.FastApi.System.Utils
{
    public class LoginUtils
    {
        private static string sLoginedUserCode = "_$login_user_code$_";

        public static bool CheckUserValidity(dynamic userObj, string platform, out string error)
        {
            error = string.Empty;

            if (!userObj.Actived)
            {
                error = "用户已禁用。";
                return false;
            }
            if (userObj.Deleted)
            {
                error = "用户已冻结。";
                return false;
            }
            if (userObj.Expires < DateTime.Now)
            {
                error = "用户使用期限已到期。";
                return false;
            }

            dynamic orgObj = OrgService.GetOrgByCode(userObj.Org);
            if (orgObj == null)
            {
                error = "用户所属机构不存在。";
                return false;
            }
            if (!orgObj.Actived)
            {
                error = "用户所属机构已禁用。";
                return false;
            }
            if (orgObj.Deleted)
            {
                error = "用户所属机构已冻结。";
                return false;
            }

            dynamic prodObj = ProductService.GetProductByCode(platform);
            if (!prodObj.Actived)
            {
                error = "应用系统已禁用。";
                return false;
            }
            if (prodObj.Deleted)
            {
                error = "应用系统已冻结。";
                return false;
            }

            dynamic orgprodObj = OrgProdService.GetOrgProdByCode(userObj.Org, platform);
            if (orgprodObj == null)
            {
                error = "缺少应用系统权限。";
                return false;
            }
            if (!orgprodObj.Actived)
            {
                error = "应用系统权限已禁用。";
                return false;
            }
            if (orgprodObj.Deleted)
            {
                error = "应用系统权限已冻结。";
                return false;
            }
            if (orgprodObj.Expires < DateTime.Now)
            {
                error = "应用系统权限已到期。";
                return false;
            }

            dynamic userprodObj = UserProdService.GetUserProdByCode(userObj.Code, platform);
            if (userprodObj == null)
            {
                error = "缺少应用系统权限！";
                return false;
            }
            if (!userprodObj.Actived)
            {
                error = "应用系统权限已禁用！";
                return false;
            }
            if (userprodObj.Deleted)
            {
                error = "应用系统权限已冻结！";
                return false;
            }
            if (userprodObj.Expires < DateTime.Now)
            {
                error = "应用系统权限已到期！";
                return false;
            }

            return true;
        }

        public static void SetLoginUserCode(ControllerContext cc, string token)
        {
            cc.Session.SetString(sLoginedUserCode, token);
        }

        public static string GetLoginUserCode(ControllerContext cc)
        {
            return cc.Session.GetString(sLoginedUserCode);
        }

        public static bool IsLogined(ControllerContext cc)
        {
            string userCode = cc.Session.GetString(sLoginedUserCode);
            return !string.IsNullOrWhiteSpace(userCode);
        }

    }
}
