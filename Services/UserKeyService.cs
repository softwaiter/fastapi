using CodeM.Common.Orm;
using System;
using System.Collections.Generic;
using XC.RSAUtil;

namespace CodeM.FastApi.Services
{
    public class UserKeyService
    {
        public static dynamic GetOpenApiByUserId(string userid)
        {
            dynamic user = UserService.GetUserById(userid);
            if (user != null)
            {
                dynamic result = OrmUtils.Model("UserKey").Equals("User", user.Code).QueryFirst();
                return result;
            }
            return null;
        }

        public static dynamic NewOpenApi(string userid)
        {
            dynamic user = UserService.GetUserById(userid);
            if (user != null)
            {
                List<string> result = RsaKeyGenerator.Pkcs1Key(2048, true);

                string[] prefixItems = new string[] { "6", "8", "9" };
                string timeStr = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string openid = prefixItems[new Random().Next(3)] + timeStr.Substring(1);

                dynamic newUserKey = OrmUtils.Model("UserKey").NewObject();
                newUserKey.User = user.Code;
                newUserKey.OpenId = openid;
                newUserKey.PrivateKey = result[0];
                newUserKey.PublicKey = result[1];

                OrmUtils.Model("UserKey").SetValues(newUserKey).Save();

                return newUserKey;
            }
            return null;
        }

        public static dynamic ReplaceSecret(string userid)
        {
            dynamic userKey = GetOpenApiByUserId(userid);
            if (userKey != null)
            {
                List<string> result = RsaKeyGenerator.Pkcs1Key(2048, true);
                userKey.PrivateKey = result[0];
                userKey.PublicKey = result[1];
                OrmUtils.Model("UserKey").SetValues(userKey).Equals("Id", userKey.Id).Update();
                return userKey;
            }
            return null;
        }

        public static bool Enable(string userid, bool enable)
        {
            dynamic userKey = GetOpenApiByUserId(userid);
            if (userKey != null)
            {
                userKey.Actived = enable;
                OrmUtils.Model("UserKey").SetValues(userKey).Equals("Id", userKey.Id).Update();
                return true;
            }
            return false;
        }
    }
}
