using CodeM.Common.Tools;
using CodeM.FastApi.Context;
using CodeM.FastApi.Services;
using System;
using System.Security.Cryptography;
using System.Text;
using XC.RSAUtil;

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

        public static string ParseLoginToken(string openid, string sign)
        {
            dynamic userKey = UserKeyService.GetOpenApiByOpenId(openid);
            if (userKey != null)
            {
                try
                {
                    RsaPkcs1Util rsa = new RsaPkcs1Util(Encoding.UTF8, userKey.PublicKey, userKey.PrivateKey, 2048);
                    string ming = rsa.Decrypt(sign, RSAEncryptionPadding.Pkcs1);
                    string[] payloads = ming.Split("+");
                    if (payloads.Length == 3)
                    {
                        if (userKey.User == payloads[0])
                        {
                            long tt;
                            if (payloads[1].Length == 13 &&
                                long.TryParse(payloads[1], out tt))
                            {
                                DateTime signTime = DateTimeUtils.GetLocalDateTimeFromUtcTimestamp13(tt);
                                TimeSpan ts = signTime - DateTime.Now;
                                if (Math.Abs(ts.TotalMinutes) < 5)
                                {
                                    return payloads[0];
                                }
                                else
                                {
                                    throw new Exception("非法签名。");
                                }
                            }
                            else
                            {
                                throw new Exception("非法签名。");
                            }
                        }
                        else
                        {
                            throw new Exception("非法签名。");
                        }
                    }
                }
                catch
                {
                    throw new Exception("非法签名！");
                }
            }
            return null;
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
