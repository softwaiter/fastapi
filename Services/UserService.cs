using CodeM.Common.Orm;
using CodeM.Common.Tools.Security;
using System;

namespace CodeM.FastApi.Services
{
    public class UserService
    {
        public static dynamic GetUserById(string id)
        {
            return OrmUtils.Model("User").Equals("Id", id).QueryFirst();
        }

        public static void UpdatePassword(string userId, string newPass)
        {
            dynamic updateObj = OrmUtils.Model("User").NewObject();
            updateObj.Id = userId;
            updateObj.Salt = HashUtils.MD5(DateTime.Now.ToString());
            updateObj.Password = HashUtils.SHA256(newPass + updateObj.Salt);
            updateObj.MustChangePassNow = true;
            OrmUtils.Model("User").Equals("Id", userId).SetValues(updateObj).Update();
        }
    }
}
